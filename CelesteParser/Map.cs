using SixLabors.ImageSharp;

namespace CelesteParser;

public class Map {
    public Rectangle Bounds;
    public List<Level> Levels = new List<Level>();
    // todo: styles (need to use ahorn or a pre-existing map)

    public void AddLevel(Level level) {
        Levels.Add(level);
        // Bounds.X = Math.Max(level.X, Bounds.X);
        // Bounds.Y = Math.Max(level.Y, Bounds.Y);
        // if (level.Bounds.Right > Bounds.Right) 
    }
}