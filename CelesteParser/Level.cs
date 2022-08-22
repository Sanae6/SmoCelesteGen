using System.Xml;
using SixLabors.ImageSharp;

namespace CelesteParser;

public class Level {
    public Rectangle Bounds { get; }
    public int X => Bounds.X;
    public int Y => Bounds.Y;
    public int Width => Bounds.Width;
    public int Height => Bounds.Height;
    public Tilemap Tilemap { get; }
    public Level(Rectangle bounds, Parser.Element levelElement) {
        Bounds = bounds;
        
        foreach (Parser.Element levelData in levelElement.Children) {
            switch (levelData.Name) {
                case "solids": {
                    // Console.WriteLine(levelData.Value);
                    Tilemap = new Tilemap(bounds, (string) levelData.Value!);
                    
                    break;
                }
            }
        }
    }
}