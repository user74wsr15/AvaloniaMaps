using Avalonia.Media;

namespace AvaloniaMaps.Avalonia.Controls.Map;

/// <summary>
/// Renders image from <see cref="Map.MapCamera"/>.
/// </summary>
public class MapRender
{
    public MapCamera MapCamera { get; }

    public MapRender()
    {
        MapCamera = new MapCamera();
    }

    public void Render(DrawingContext dc)
    {
           
    }
}