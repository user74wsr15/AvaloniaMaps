using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace AvaloniaMaps.Avalonia.Controls.Map;

public class OsmTileProvider : ITileProvider
{
    private static readonly HttpClient Client = new();
    private const string TileUrl = "https://tile.openstreetmap.org/{0}/{1}/{2}.png";

    public async Task<IImage> GetTile(MapTileInfo tileInfo)
    {
        // We have to set user agent because otherwise we will get 403 http error
        // https://stackoverflow.com/questions/46604840/403-response-with-httpclient-but-not-with-browser
        var url = string.Format(TileUrl, tileInfo.Level, tileInfo.WrappedPoint.X, tileInfo.WrappedPoint.Y);
        var request = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers =
            {
                {
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) " +
                    "Chrome/23.0.1271.95 Safari/537.11"
                }
            }
        };
        var result = await Client.SendAsync(request);
        var bitmap = WriteableBitmap.Decode(await result.Content.ReadAsStreamAsync());

        return bitmap;
    }
}