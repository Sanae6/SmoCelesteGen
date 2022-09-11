using System.Text;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;

namespace CelesteParser;

public class Tilemap {
    private static readonly Regex NewlineRegex = new Regex("\\r\\n|\\n\\r|\\n|\\r", RegexOptions.Compiled);
    public const char EmptyTile = ' ';
    public const char AirTile = '0';
    public const char Wildcard = '*';
    protected char[]? Tiles { get; private set; }
    public Rectangle Bounds { get; protected set; }
    public int Width => Bounds.Width;
    public int Height => Bounds.Height;

    protected Tilemap(Rectangle bounds) {
        Bounds = bounds;
    }

    public Tilemap(Rectangle bounds, string tilemap) {
        Bounds = bounds;
        Tiles = Enumerable.Repeat(AirTile, Width * Height).ToArray();
        ApplyMap(0, 0, tilemap);
    }

    protected void Initialize() {
        Tiles = Enumerable.Repeat(AirTile, Width * Height).ToArray();
    }

    public void ApplyMap(int startX, int startY, string tilemap) {
        string[] strings = NewlineRegex.Split(tilemap);
        for (int y = 0; y < Math.Clamp(strings.Length, 0, Height); y++) {
            string current = strings[y];
            for (int x = 0; x < Math.Clamp(current.Length, 0, Width); x++) {
                this[startX + x, startY + y] = current[x];
            }
        }
    }

    public virtual char this[int x, int y] {
        get => Tiles![y * Width + x];
        set => Tiles![y * Width + x] = value;
    }

    public override string ToString() {
        StringBuilder builder = new StringBuilder();
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                builder.Append(this[x, y]);
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }
}