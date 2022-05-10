using System;
using Avalonia;
using Metsys.Bson;

namespace AvaloniaMaps.Avalonia.Controls.Map;

public readonly struct MapTileInfo : IEquatable<MapTileInfo>
{
    public Point Point { get; }
    public int Level { get; }

    public Rect WorldRect { get; }

    public MapTileInfo(Point point, int level)
    {
        Point = point;
        Level = level;

        // Tiles have different size on every level, for i.e. tile 0, 0 on level 0 will cover
        // whole map, but same time on level 19 will be extremely small
        // Every time each tile divides on two so we multiply by 2^level
        // 0 - 256
        // 1 - 512
        // 2 - 1024
        // 3 - 2048 (2^3 * 256 = 8 * 256 = 2048)
        float size = 256 * MapHelper.GetNumberOfTilesAtZoom(level);
        WorldRect = new Rect(
            Point.X,
            Point.X,
            size,
            size);
    }

    public bool Equals(MapTileInfo other)
    {
        return Point.Equals(other.Point) && Level.Equals(other.Level);
    }

    public override bool Equals(object? obj)
    {
        return obj is MapTileInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Point, Level);
    }

    public static bool operator ==(MapTileInfo left, MapTileInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MapTileInfo left, MapTileInfo right)
    {
        return !(left == right);
    }
}