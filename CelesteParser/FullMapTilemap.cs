using System.Collections;
using SixLabors.ImageSharp;

namespace CelesteParser;

public class FullMapTilemap : Tilemap, IEnumerable<(int X, int Y, char Value)> {
    private Map Map;

    public override char this[int x, int y] {
        get {
            Level? level = Map.Levels.SingleOrDefault(level => level.Bounds.Contains(x, y));
            return level != null ? level.Tilemap[x - level.X, y - level.Y] : '0';
        }
        set {
            Level? level = Map.Levels.SingleOrDefault(level => level.Bounds.Contains(x, y));
            if (level != null) level.Tilemap[x - level.X, y - level.Y] = value;
        }
    }

    public FullMapTilemap(Map map) : base(new Rectangle(map.Min, (Size) (map.Max - (Size) map.Min))) {
        Map = map;
    }
    public IEnumerator<(int X, int Y, char Value)> GetEnumerator() {
        foreach (Level level in Map.Levels) {
            for (int y = level.Y; y < level.X + level.Height; y++) {
                for (int x = level.X; x < level.X + level.Width; x++) {
                    char value = this[x, y];
                    if (value is not EmptyTile and not AirTile)
                        yield return (x, y, this[x, y]);
                }
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}