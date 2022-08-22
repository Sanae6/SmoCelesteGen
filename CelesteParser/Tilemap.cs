using System.Text;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;

namespace CelesteParser;

public class Tilemap {
    private static readonly Regex NewlineRegex = new Regex("\\r\\n|\\n\\r|\\n|\\r");
    protected readonly char[] Tiles;
    private readonly Rectangle Bounds;
    public virtual int Width => Bounds.Width;
    public virtual int Height => Bounds.Height;

    protected Tilemap(Rectangle bounds) {
    }

    public Tilemap(Rectangle bounds, string tilemap) {
        Bounds = bounds;
        Initialize();
    }

    protected void Initialize() {
        Array.Fill(Tiles, '0');
    }

    public void ApplyMap(int startX, int startY, string tilemap) {
        string[] strings = NewlineRegex.Split(tilemap);
        for (int y = 0; y < strings.Length; y++) {
            string current = strings[y];
            for (int x = 0; x < current.Length; x++) {
                this[startX + x, startY + y] = current[x];
            }
        }
    }

    public char this[int x, int y] {
        get => Tiles[(y - Bounds.Y) * Width + (x - Bounds.X)];
        set => Tiles[(y - Bounds.Y) * Width + (x - Bounds.X)] = value;
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