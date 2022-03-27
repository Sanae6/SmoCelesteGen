using System.Xml;
using SixLabors.ImageSharp;
using RgbaImage = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace CelesteParser;

public class Autotiler {
    private const string TilesetBase = "tilesets/";
    private Dictionary<char, TerrainType> TileLookup = new Dictionary<char, TerrainType>();
    public Autotiler(string filename) {
        XmlDocument document = new XmlDocument();
        document.LoadXml(File.ReadAllText(Path.Combine(ContentBase, filename)));
        foreach (object elObj in document.GetElementsByTagName("Tileset")) {
            XmlElement tilesetElement = (XmlElement) elObj;
            char tileId = tilesetElement.AttrChar("id");
            if (tilesetElement.HasAttribute("copy")) {
                char attrChar = tilesetElement.AttrChar("copy");
            }
            tilesetElement.GetAttribute("ignores");
            tilesetElement.GetAttribute("path");
        }
    }
    public static string ContentBase { get; set; }

    private void ReadInto(TerrainType data, Tileset tileset, XmlElement element) {
        foreach (object els in element.ChildNodes) {
            if (els is XmlElement set) {
                string mask = set.GetAttribute("mask");
                Tiles tiles;
                if (mask == "center") tiles = data.Center;
                else if (mask == "padding") tiles = data.Padded;
                else {
                    Masked masked = new Masked();
                    tiles = masked.Tiles;
                    int n = 0;
                    foreach (char t in mask) {
                        switch (t) {
                            case '0':
                                masked.Mask[n++] = 0;
                                break;
                            case '1':
                                masked.Mask[n++] = 1;
                                break;
                            case 'x' or 'X':
                                masked.Mask[n++] = 2;
                                break;
                            default:
                                continue;
                        }
                    }
                    data.Masked.Add(masked);
                }
                foreach (string tile in set.GetAttribute("tiles").Split(';')) {
                    string[] pointStr = tile.Split(',');
                    Point point = new Point(int.Parse(pointStr[0]), int.Parse(pointStr[1]));
                    RgbaImage? image = tileset[point];
                    tiles.Textures.Add(image!);
                }
                if (set.HasAttribute("sprites")) {
                    foreach (string item2 in set.GetAttribute("sprites").Split(','))
                        tiles.OverlapSprites.Add(item2);
                    tiles.HasOverlays = true;
                }
            }
        }

        data.Masked.Sort((a, b) => {
            int num2 = 0;
            int num3 = 0;
            for (int k = 0; k < 9; k++) {
                if (a.Mask[k] == 2) num2++;
                if (b.Mask[k] == 2) num3++;
            }
            return num2 - num3;
        });
    }

    public Generated Generate() {
        TileGrid tileGrid = new TileGrid(8, 8, tilesX, tilesY);
        AnimatedTiles animatedTiles = new AnimatedTiles(tilesX, tilesY, GFX.AnimatedTilesBank);
        Rectangle empty = Rectangle.Empty;
        if (forceSolid) {
            empty = new Rectangle(startX, startY, tilesX, tilesY);
        }
        if (mapData != null) {
            for (int i = startX; i < startX + tilesX; i += 50) {
                for (int j = startY; j < startY + tilesY; j += 50) {
                    if (!mapData.AnyInSegmentAtTile(i, j)) {
                        j = j / 50 * 50;
                    } else {
                        int k = i;
                        int num = Math.Min(i + 50, startX + tilesX);
                        while (k < num) {
                            int l = j;
                            int num2 = Math.Min(j + 50, startY + tilesY);
                            while (l < num2) {
                                Autotiler.Tiles tiles = this.TileHandler(mapData, k, l, empty, forceID, behaviour);
                                if (tiles != null) {
                                    tileGrid.Tiles[k - startX, l - startY] = Calc.Random.Choose(tiles.Textures);
                                    if (tiles.HasOverlays) {
                                        animatedTiles.Set(k - startX, l - startY, Calc.Random.Choose(tiles.OverlapSprites), 1f, 1f);
                                    }
                                }
                                l++;
                            }
                            k++;
                        }
                    }
                }
            }
        } else {
            for (int m = startX; m < startX + tilesX; m++) {
                for (int n = startY; n < startY + tilesY; n++) {
                    Autotiler.Tiles tiles2 = this.TileHandler(null, m, n, empty, forceID, behaviour);
                    if (tiles2 != null) {
                        tileGrid.Tiles[m - startX, n - startY] = Calc.Random.Choose(tiles2.Textures);
                        if (tiles2.HasOverlays) {
                            animatedTiles.Set(m - startX, n - startY, Calc.Random.Choose(tiles2.OverlapSprites), 1f, 1f);
                        }
                    }
                }
            }
        }
        return new Autotiler.Generated {
            TileGrid = tileGrid,
            SpriteOverlay = animatedTiles
        };
    }

    private class Masked {
        public byte[] Mask;
        public Tiles Tiles;
        public Masked() {
            Mask = new byte[9];
            Tiles = new Tiles();
        }
    }

    private class Tiles {
        public bool HasOverlays;
        public List<string> OverlapSprites;
        public List<RgbaImage> Textures;

        public Tiles() {
            Textures = new List<RgbaImage>();
            OverlapSprites = new List<string>();
        }
    }

    private class TerrainType {
        public Tiles Center;
        public List<Tiles> CustomFills;
        public string Debris;
        public char ID;
        public HashSet<char> Ignores;
        public List<Masked> Masked;
        public Tiles Padded;
        public int ScanHeight;
        public int ScanWidth;
    }
    public struct Generated
    {
        public TileGrid TileGrid;
        public AnimatedTiles SpriteOverlay;
    }
}