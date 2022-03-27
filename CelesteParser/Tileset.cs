using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CelesteParser;

using RgbaImage = Image<Rgba32>;

public class Tileset {
    public RgbaImage?[,] Tiles;
    public RgbaImage MainTexture;
    public int TileHeight { get; private set; }
    public int TileWidth { get; private set; }

    public RgbaImage? this[int x, int y] => Tiles[x, y];
    public RgbaImage? this[Point point] => Tiles[point.X, point.Y];
    public RgbaImage? this[int index] => index < 0 ? null : Tiles[index % Tiles.GetLength(0), index / Tiles.GetLength(0)];

    public Tileset(RgbaImage texture, int tileWidth, int tileHeight) {
        MainTexture = texture;
        TileWidth = tileWidth;
        TileHeight = TileHeight;
        Tiles = new RgbaImage[MainTexture.Width / tileWidth, MainTexture.Height / tileHeight];
        for (int i = 0; i < MainTexture.Width / tileWidth; i++) {
            for (int j = 0; j < MainTexture.Height / tileHeight; j++) {
                RgbaImage subtex = texture.Clone();
                subtex.Mutate(x => x.Crop(new Rectangle(i * tileWidth, i * tileHeight, tileWidth, tileHeight)));
                Tiles[i, j] = subtex;
            }
        }
    }
}