using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

    private Point _previousMousePos; // Previous mouse position to calculate drag delta
    private bool _isDragging; // Whether user is currently dragging map

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
        BoundsProperty.Changed.Subscribe(() => { MapRender.MapCamera.ViewBounds = Bounds; });

        PointerMoved += PointerMovedHandler;

        MapRender.MapCamera.OnGenerationFinished += InvalidateVisual;
    }

    private void PointerMovedHandler(object sender, PointerEventArgs e)
    {
        // Start drag if left mouse is held
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = false;
            return;
        }
        var pos = e.GetPosition(this);

        // Init dragging
        if (!_isDragging)
        {
            _previousMousePos = pos;
            _isDragging = true;
        }

        MapRender.MapCamera.Translate(pos - _previousMousePos);

        _previousMousePos = pos;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            var childMapPos = GetMapPosition(child);
            var childScreenPos = MapRender.MapCamera.WorldPointToScreenCoordinates(childMapPos);
            child.Arrange(new Rect(childScreenPos, child.DesiredSize));
        }

        return finalSize;
    }

    public override void Render(DrawingContext dc)
    {
        MapRender.Render(dc);
    }
}