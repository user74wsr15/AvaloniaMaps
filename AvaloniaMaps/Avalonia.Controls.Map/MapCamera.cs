using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;

namespace AvaloniaMaps.Avalonia.Controls.Map;

/// <summary>
/// This class handles camera that looks at some point on map and has viewing bounds.
/// </summary>
public class MapCamera
{
    /// <summary>
    /// Gets a rect that defines view position and number of tiles that can possibly fit in.
    /// </summary>
    public Rect TileExtents
    {
        get => _tileExtents;
        private set
        {
            _tileExtents = value;
            TileRenderExtents = new Rect(
                TileExtents.Left - 2.0,
                TileExtents.Top - 2.0,
                TileExtents.Width + 2.0,
                TileExtents.Height + 2.0);
        }
    }

    /// <summary>
    /// Gets a rect within the area where all tiles are rendered.
    /// <para>It's 2.0 bigger on each side than <see cref="TileExtents"/>.</para>
    /// </summary>
    public Rect TileRenderExtents { get; private set; }

    /// <summary>
    /// Gets a rect within the area where all tiles are generated.
    /// </summary>
    public Rect TileGeneratedExtents { get; private set; }
    
    /// <summary>
    /// Gets Map Point in center of the view.
    /// </summary>
    public MapPoint ViewCenter
    {
        get => _viewCenter;
        set
        {
            // Translate view by delta between required and current position
            var source = TileExtents.Center;
            var target = MapHelper.WorldToTilePos(value, Level);
            var destination = target - source;
            Translate(destination);

            _viewCenter = value;
        }
    }

    /// <summary>
    /// Gets or sets level of detail (also know as zoom). Higher is closer.
    /// </summary>
    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            TileSize = MapTileInfo.GetTileSizeForLevel(Level);
        }
    }

    /// <summary>
    /// Gets or sets a rect that defines screen (view) bounds.
    /// </summary>
    public Rect ViewBounds
    {
        get => _viewBounds;
        set
        {
            _viewBounds = value;
            Update();
        }
    }

    /// <summary>
    /// Gets tile size in pixels on current level.
    /// </summary>
    public int TileSize { get; private set; }

    /// <summary>
    /// Gets a tuple of Image and Rect of tiles that are ready to be rendered.
    /// </summary>
    public IEnumerable<(IImage, Rect)> VisibleTiles => _tiles.Values;

    public delegate void OnGenerationFinishedHandler();
    public event OnGenerationFinishedHandler OnGenerationFinished;
    
    private MapPoint _viewCenter;
    private Rect _tileExtents;
    private Rect _viewBounds;
    private int _level;
    private readonly Dictionary<MapTileInfo, (IImage, Rect)> _tiles = new();

    public MapCamera()
    {
        TileExtents = Rect.Empty;
        Level = 0;

        Update();
    }

    public void Refresh()
    {
        _tiles.Clear();
        UpdateGenerator();
    }

    /// <summary>
    /// Translates view by given point.
    /// </summary>
    /// <param name="p">Point to translate view on.</param>
    public void Translate(Point p)
    {
        TileExtents = new Rect(
            TileExtents.Left + p.X,
            TileExtents.Right + p.Y,
            TileExtents.Width,
            TileExtents.Height);
        
        UpdateGenerator();
    }

    /// <summary>
    /// Gets a value indicating whether tile is inside rendering area. 
    /// </summary>
    /// <returns></returns>
    public bool IsTileVisible(MapTileInfo tileInfo)
    {
        return tileInfo.Rect.Intersects(TileRenderExtents);
    }

    /// <summary>
    /// Converts given screen position to map coordinates.
    /// </summary>
    /// <param name="point">Screen point to convert.</param>
    /// <returns>A Map point that is relative to given screen position.</returns>
    public MapPoint ScreenPointToWorldCoordinates(Point point)
    {
        double xNorm = point.X / ViewBounds.Width;
        double yNorm = point.Y / ViewBounds.Height;

        // Tile extents are linear so we can simply interpolate
        double x = xNorm.Remap(0.0, 1.0, TileExtents.Left, TileExtents.Right);
        double y = yNorm.Remap(0.0, 1.0, TileExtents.Top, TileExtents.Bottom);

        return MapHelper.TileToWorldPos(new Point(x, y), Level);
    }

    /// <summary>
    /// Converts given coordinates to relative point on screen.
    /// </summary>
    /// <param name="point">Map point to convert.</param>
    /// <returns>A Point that is relative world position within screen space.</returns>
    public Point WorldPointToScreenCoordinates(MapPoint point)
    {
        (double x, double y) = MapHelper.WorldToTilePos(point, Level);

        // Shift point to screen space
        x -= TileExtents.Left;
        y -= TileExtents.Top;

        // Multiply on 256 to get screen size
        x *= 256;
        y *= 256;

        return new Point(x, y);
    }

    /// <summary>
    /// Converts position of given tile to screen coordinates.
    /// </summary>
    /// <param name="tile">Tile which position to convert.</param>
    /// <returns>A Point that is relative to tile position within screen space.</returns>
    public Point TileToScreenPoint(MapTileInfo tile)
    {
        // Simply remap from TileExtents to ViewBounds
        double x = tile.Point.X.Remap(
            TileExtents.Left,
            TileExtents.Right,
            ViewBounds.Left,
            ViewBounds.Right);
        double y = tile.Point.X.Remap(
            TileExtents.Top,
            TileExtents.Bottom,
            ViewBounds.Top,
            ViewBounds.Bottom);

        return new Point(x, y);
    }

    private void Update()
    {
        UpdateExtents();
        UpdateViewCenter();
        UpdateGenerator();
    }

    private async void UpdateGenerator()
    {
        if (!IsGenerationDirty())
            return;
        
        await UpdateTiles();

        OnGenerationFinished?.Invoke();
    }

    /// <summary>
    /// Checks if difference between render extents and generated extents
    /// is more than one tile.
    /// </summary>
    /// <returns>True if generation is not valid anymore, otherwise false.</returns>
    private bool IsGenerationDirty()
    {
        if (TileGeneratedExtents.Left.Delta(TileRenderExtents.Left) > 1.0)
            return true;
        if (TileGeneratedExtents.Top.Delta(TileRenderExtents.Top) > 1.0)
            return true;
        if (TileGeneratedExtents.Width.Delta(TileRenderExtents.Width) > 1.0)
            return true;
        if (TileGeneratedExtents.Height.Delta(TileRenderExtents.Height) > 1.0)
            return true;

        return false;
    }

    private async Task UpdateTiles()
    {
        int xStart = (int)Math.Floor(TileRenderExtents.Left);
        int xNum = (int)Math.Ceiling(TileRenderExtents.Width);

        int yStart = (int)Math.Floor(TileRenderExtents.Top);
        int yNum = (int)Math.Ceiling(TileRenderExtents.Height);

        TileGeneratedExtents = new Rect(xStart, yStart, xNum, yNum);
        
        for (int x = xStart; x < xNum; x++)
        {
            for (int y = yStart; y < yNum; y++)
            {
                var tileInfo = new MapTileInfo(new Point(xStart + x, yStart + y), Level);

                var width = tileInfo.Rect.Width;
                var height = tileInfo.Rect.Height;
                var xPos = x * width;
                var yPos = y * height;
                var imageRect = new Rect(xPos, yPos, width, height);

                if (_tiles.ContainsKey(tileInfo))
                {
                    // If image is outside bounds - remove it
                    if (!IsTileVisible(tileInfo))
                    {
                        _tiles.Remove(tileInfo);
                        continue;
                    }

                    // Otherwise just update rect
                    _tiles[tileInfo] = (_tiles[tileInfo].Item1, imageRect);
                    continue;
                }

                // If image is new, download it and set rect
                _tiles[tileInfo] = (await new OsmTileProvider().GetTile(tileInfo), imageRect);
            }
        }
    }

    private void UpdateExtents()
    {
        double numTilesX = ViewBounds.Width / 256;
        double numTilesY = ViewBounds.Height / 256;

        TileExtents = new Rect(
            TileExtents.Left,
            TileExtents.Top,
            numTilesX,
            numTilesY);
    }

    private void UpdateViewCenter()
    {
        // Set value directly to avoid setter logic
        _viewCenter = MapHelper.TileToWorldPos(TileExtents.Center, Level);
    }
}