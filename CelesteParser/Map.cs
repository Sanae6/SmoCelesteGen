using SixLabors.ImageSharp;

namespace CelesteParser;

public class Map {
    public Point Min, Max;
    public List<Level> Levels = new List<Level>();
    public Rectangle Bounds => new Rectangle(Min, (Size) (Max - (Size) Min));
    // todo: styles (need to use ahorn or a pre-existing map)

    public void AddLevel(Level level) {
        Levels.Add(level);
        if (level.X < Min.X) Min.X = level.X;
        if (level.Y < Min.Y) Min.Y = level.Y;
        if (level.Bounds.Right > Max.X) Max.X = level.Bounds.Right;
        if (level.Bounds.Right > Max.Y) Max.Y = level.Bounds.Right;
    }
}