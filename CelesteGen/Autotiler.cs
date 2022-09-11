using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using BfresLibrary;
using BfresLibrary.GX2;
using BfresLibrary.Helpers;
using BfresLibrary.Switch;
using CelesteGen;
using SixLabors.ImageSharp;
using Syroot.Maths;
using Syroot.NintenTools.NSW.Bntx;
using Buffer = BfresLibrary.Buffer;
using ByteOrder = Syroot.BinaryData.ByteOrder;
using RgbaImage = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace CelesteParser;

public class Autotiler {
    public static string ContentBase { get; set; } = null!;
    private const string TilesetBase = "tilesets/";
    private Dictionary<char, TerrainType> TileLookup = new Dictionary<char, TerrainType>();
    public Autotiler(string filename) {
        XmlDocument document = new XmlDocument();
        document.LoadXml(File.ReadAllText(Path.Combine(ContentBase, filename)));
        Dictionary<char, XmlElement> dictionary = new Dictionary<char, XmlElement>();
        foreach (object elObj in document.GetElementsByTagName("Tileset")) {
            XmlElement tilesetElement = (XmlElement) elObj;
            char tileId = tilesetElement.AttrChar("id");
            Tileset tileset = new Tileset($"{tilesetElement.GetAttribute("path")}_{tileId}", Atlas.FgTiles[$"tilesets/{tilesetElement.GetAttribute("path")}"], 8, 8);
            TerrainType type = new TerrainType();
            type.Tileset = tileset;
            ReadInto(type, tileset, tilesetElement);
            if (tilesetElement.HasAttribute("copy")) {
                ReadInto(type, tileset, dictionary[tilesetElement.AttrChar("copy")]);
            }

            if (tilesetElement.HasAttribute("ignores"))
                foreach (string text in tilesetElement.GetAttribute("ignores").Split(new[] {','}))
                    if (text.Length > 0)
                        type.Ignores.Add(text[0]);
            dictionary.Add(tileId, tilesetElement);
            TileLookup.Add(tileId, type);
        }
    }

    private void ReadInto(TerrainType data, Tileset tileset, XmlElement element) {
        foreach (object els in element.ChildNodes) {
            if (els is XmlElement set) {
                string mask = set.GetAttribute("mask");
                Tiles tiles;
                switch (mask) {
                    case "center":
                        tiles = data.Center;
                        break;
                    case "padding":
                        tiles = data.Padded;
                        break;
                    default: {
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
                        break;
                    }
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

    public void GenerateModel(ResFile bfres, BntxFile bntx, Model model, Map map) {
        FullMapTilemap tilemap = new FullMapTilemap(map);
        VertexBufferHelper helper = new VertexBufferHelper(model.VertexBuffers[0], ByteOrder.LittleEndian);
        (int X, int Y, char Value)[] validTiles = tilemap.ToArray();
        helper.Attributes[0].Data = new Vector4F[4 * validTiles.Length];
        helper.Attributes[1].Data = Enumerable.Repeat(new Vector4F(0f, 0f, 1f, 0f), 4 * validTiles.Length).ToArray();
        helper.Attributes[2].Data = Enumerable.Repeat(Vector4F.One, 4 * validTiles.Length).ToArray();
        helper.Attributes[3].Data = new Vector4F[4 * validTiles.Length];
        Shape templateShape = model.Shapes[0];
        model.Materials.Clear();
        model.Shapes.Clear();
        int shapeIndex = 0;
        int index = 0;
        uint lastVertex = 0;
        Dictionary<char,List<Point>> tileDict = validTiles.GroupBy(x => x.Value)
            .ToDictionary(x => x.Key, y => y.Select(x => new Point(x.X, x.Y)).ToList());
        foreach ((char value, List<Point> points) in tileDict) {
            
            TerrainType type = TileLookup[value];
            Shape shape = templateShape.ShallowCopy();
            shape.Name = $"Shape_{shapeIndex++}";
            shape.MaterialIndex = model.AddMaterial(bfres, bntx, type.Tileset.MainTexture, type.Tileset.Name, out Size textureSize);
            model.Shapes.Add(shape.Name, shape);
            shape.Meshes[0].FirstVertex = lastVertex;
            shape.Meshes[0].PrimitiveType = GX2PrimitiveType.Triangles;
            List<uint> indices = new List<uint>();
            foreach ((int x, int y) in points) {
                const float scale = 100f;
                const float texSize = 8f;

                helper.Attributes[0].Data[index + 0] = new Vector4F(x * scale, -y * scale, 0f, 0f);
                helper.Attributes[0].Data[index + 1] = new Vector4F(x * scale + scale, -y * scale, 0f, 0f);
                helper.Attributes[0].Data[index + 2] = new Vector4F(x * scale, -y * scale + scale, 0f, 0f);
                helper.Attributes[0].Data[index + 3] = new Vector4F(x * scale + scale, -y * scale + scale, 0f, 0f);
                helper.Attributes[3].Data[index + 0] = new Vector2F(0f, 0f).GetScaledPosition(textureSize);
                helper.Attributes[3].Data[index + 1] = new Vector2F(texSize, 0f).GetScaledPosition(textureSize);
                helper.Attributes[3].Data[index + 2] = new Vector2F(0f, texSize).GetScaledPosition(textureSize);
                helper.Attributes[3].Data[index + 3] = new Vector2F(texSize, texSize).GetScaledPosition(textureSize);
                
                indices.Add((uint) (index + 0));
                indices.Add((uint) (index + 1));
                indices.Add((uint) (index + 2));
                indices.Add((uint) (index + 2));
                indices.Add((uint) (index + 1));
                indices.Add((uint) (index + 3));

                index+= 4;
                lastVertex += 4;
                
            }
            shape.Meshes[0].SetIndices(indices, GX2IndexFormat.UInt32LittleEndian);
        }

        model.VertexBuffers[0] = helper.ToVertexBuffer();
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
        public Tileset Tileset;
        public Tiles Center = new Tiles();
        public List<Tiles> CustomFills = new List<Tiles>();
        public string Debris;
        public char ID;
        public HashSet<char> Ignores = new HashSet<char>();
        public List<Masked> Masked = new List<Masked>();
        public Tiles Padded = new Tiles();
        public int ScanHeight;
        public int ScanWidth;
    }
}