using System.Threading.Tasks;
using Avalonia.Media;

namespace AvaloniaMaps.Avalonia.Controls.Map;

/// <summary>
/// Defines a method that gets <see cref="IImage"/> for requested tile.
/// </summary>
public interface ITileProvider
{
    public Task<IImage> GetTile(MapTileInfo tileInfo);
}