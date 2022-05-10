using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace AvaloniaMaps.Avalonia.Controls.Map;

public class DemoTileProvider : ITileProvider
{
    private static readonly SKPaint TextPaint = new(new SKFont(SKTypeface.Default));
    
    public async Task<IImage> GetTile(MapTileInfo tileInfo)
    {
        return await Task.Run(() =>
        {
            using var skImage = SKImage.Create(new SKImageInfo(256, 256));
            using var skBitmap = SKBitmap.FromImage(skImage);
            using var skCanvas = new SKCanvas(skBitmap);
        
            skCanvas.DrawText("", 0.0f, 0.0f, TextPaint);

            var memStream = new MemoryStream();
            skImage
                .Encode()
                .AsStream()
                .CopyTo(memStream);
        
            return new Bitmap(memStream);
        });
    }
}