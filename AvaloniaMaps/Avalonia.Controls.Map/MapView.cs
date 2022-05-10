using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaMaps.Avalonia.Controls.Map;

public class MapView : Panel
{
    public MapRender MapRender { get; }

    // TODO: Move these properties to MapControl
    
    public int Level
    {
        get => GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }
    
    public MapPoint ViewCenter
    {
        get => GetValue(ViewCenterProperty);
        set => SetValue(ViewCenterProperty, value);
    }

    public static MapPoint GetMapPosition(IControl element)
    {
        return element.GetValue(MapPositionProperty);
    }

    public static void SetMapPosition(IControl element, MapPoint position)
    {
        element.SetValue(MapPositionProperty, position);
    }

    public static readonly DirectProperty<MapView, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<MapView, int>(
            nameof(Level),
            map => map.MapRender.MapCamera.Level,
            (map, value) => map.MapRender.MapCamera.Level = value);

    public static readonly DirectProperty<MapView, MapPoint> ViewCenterProperty =
        AvaloniaProperty.RegisterDirect<MapView, MapPoint>(
            nameof(ViewCenter),
            map => map.MapRender.MapCamera.ViewCenter,
            (map, value) => map.MapRender.MapCamera.ViewCenter = value);

    public static readonly AttachedProperty<MapPoint> MapPositionProperty =
        AvaloniaProperty.RegisterAttached<MapView, MapPoint>(
            "MapPosition", typeof(MapView), MapPoint.Zero);

    private bool _boundsSet = false;
    static MapView()
    {
        AffectsRender<MapView>(BoundsProperty);
        AffectsRender<MapView>(LevelProperty);
        AffectsRender<MapView>(ViewCenterProperty);
        
        AffectsArrange<MapView>(BoundsProperty);
        AffectsArrange<MapView>(MapPositionProperty);
        AffectsArrange<MapView>(ViewCenterProperty);
    }
    
    public MapView()
    {
        MapRender = new MapRender();
        
        // Update camera view bound on screen resize
        // TODO: Check why this invokes like 10 times on startup...
        BoundsProperty.Changed.Subscribe(() =>
        {
            MapRender.MapCamera.ViewBounds = Bounds;

            // We need to set ViewCenter after bounds set first time
            if (_boundsSet || Bounds.Width == 0)
                return;
            
            MapRender.MapCamera.ViewCenter = ViewCenter; // Doesn't work...
            _boundsSet = true;
        });
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            var pos = MapRender.MapCamera.WorldPointToScreenCoordinates(GetMapPosition(child));
            child.Arrange(new Rect(pos, child.DesiredSize));
        }
        
        return finalSize;
    }

    public override void Render(DrawingContext dc)
    {
        MapRender.Render(dc);
    }
}