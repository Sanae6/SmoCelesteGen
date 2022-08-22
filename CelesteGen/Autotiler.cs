using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml;
using BfresLibrary;
using SixLabors.ImageSharp;
using Buffer = BfresLibrary.Buffer;
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

    public Model[] GenerateModels(Map map) {
        FullMapTilemap tilemap = new FullMapTilemap(map);
        List<Model> models = new List<Model>();
        
        foreach (Level level in map.Levels) {
            Model model = new Model();
            Material material = new Material();
            VertexBuffer buffer = new VertexBuffer();
            Buffer posBuffer = new Buffer();
            buffer.Buffers.Add(posBuffer);
            Shape shape = new Shape {
                VertexBufferIndex = 0,
                Flags = ShapeFlags.HasVertexBuffer
            };
            model.Materials.Add("basicMat", material);
            model.VertexBuffers.Add(buffer);
            model.Shapes.Add("BaseShape", shape);

            List<Vector3> vertices = new List<Vector3>();
            for (int x = level.X; x < level.X + level.Width; x++) {
                for (int y = level.Y; y < level.Y + level.Height; y++) {
                    vertices.Add(new Vector3());
                }
            }

            posBuffer.Data = new byte[vertices.Count][];
            posBuffer.Stride = 12;
            for (int i = 0; i < vertices.Count; i++) {
                Vector3 vertex = vertices[i];
                posBuffer.Data[i] = MemoryMarshal.Cast<Vector3, byte>(MemoryMarshal.CreateReadOnlySpan(ref vertex, 1)).ToArray();
            }

            models.Add(model);
        }

        return models.ToArray();
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
}
