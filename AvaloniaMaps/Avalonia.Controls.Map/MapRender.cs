using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;

namespace AvaloniaMaps.Avalonia.Controls.Map;

/// <summary>
/// Renders image from <see cref="Map.MapCamera"/>.
/// </summary>
public class MapRender
{
    public MapCamera MapCamera { get; }
    public ITileProvider TileProvider { get; set; }

    public MapRender()
    {
        MapCamera = new MapCamera();
        TileProvider = new OsmTileProvider();

        
    }

    public void Render(DrawingContext dc)
    {
        // var renderRect = MapCamera.TileRenderExtents;
        // var level = MapCamera.Level;
        //
        // int xStart = (int)Math.Floor(renderRect.Left);
        // int xNum = (int)Math.Ceiling(renderRect.Width);
        //
        // int yStart = (int)Math.Floor(renderRect.Top);
        // int yNum = (int)Math.Ceiling(renderRect.Height);
        //
        // for (int x = xStart; x < xNum; x++)
        // {
        //     for (int y = yStart; y < yNum; y++)
        //     {
        //         var tileInfo = new MapTileInfo(new Point(xStart + x, yStart + y), level);
        //
        //         var width = tileInfo.WorldRect.Width;
        //         var height = tileInfo.WorldRect.Height;
        //         var xPos = x * width;
        //         var yPos = y * height;
        //
        //         var imageRect = new Rect(xPos, yPos, width, height);
        //         
        //         // dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Blue), imageRect);
        //         // dc.DrawText(Brushes.Black, new Point(xPos, yPos), new FormattedText(
        //         //     $"{x} {y} {level}",
        //         //     Typeface.Default, 
        //         //     36, 
        //         //     TextAlignment.Left, 
        //         //     TextWrapping.NoWrap, 
        //         //     new Size(width, height)));
        //         //var image = await TileProvider.GetTile(tileInfo);
        //         //dc.DrawImage(, imageRect);
        //
        //         dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Blue), imageRect);
        //         //image.Draw(dc, imageRect, imageRect, BitmapInterpolationMode.HighQuality);
        //
        //         //Image
        //     }
        // }
        
        foreach (var (image, rect) in MapCamera.VisibleTiles)
        {
            dc.DrawImage(image, rect);
        }
    }
}