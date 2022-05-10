using Avalonia;

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
                TileExtents.Left + 2.0,
                TileExtents.Top + 2.0,
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
    public int Level { get; set; }

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

    private MapPoint _viewCenter;
    private Rect _tileExtents;
    private Rect _viewBounds;

    public MapCamera()
    {
        ViewCenter = new MapPoint(0.0, 0.0);
        TileExtents = Rect.Empty;
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
    }

    /// <summary>
    /// Gets a value indicating whether tile is inside rendering area. 
    /// </summary>
    /// <returns></returns>
    public bool IsTileVisible(MapTileInfo tileInfo)
    {
        return tileInfo.WorldRect.Intersects(TileRenderExtents);
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