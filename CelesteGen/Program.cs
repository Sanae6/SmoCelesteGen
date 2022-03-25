using System.Xml;
using BfresLibrary;
using CelesteParser;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Syroot.NintenTools.Byaml;
using Formatting = Newtonsoft.Json.Formatting;

Parser.Level levelRoot = Parser.Parse(File.ReadAllBytes(args[1]));

File.WriteAllText($"{args[1]}.json", JsonConvert.SerializeObject(levelRoot, Formatting.Indented));
Atlas.ContentBase = Path.Combine(args[0], "Graphics", "Atlases");
Autotiler.ContentBase = Path.Combine(args[0], "Graphics");
Atlas gameAtlas = Atlas.FromPath($"{args[0]}/Graphics/Atlases/Gameplay", Atlas.DataFormat.Packer);
Autotiler fgTiler = new Autotiler("ForegroundTiles.xml");

List<GamePoint> list = new List<GamePoint>();
Point minPoint = Point.Empty;
Point maxPoint = Point.Empty;
foreach (Parser.Element rootChild in levelRoot.Root.Children) {
    switch (rootChild.Name) {
        case "levels": {
            foreach (Parser.Element level in rootChild.Children) {
                Point point = new Point((int) level.Attributes["x"], (int) level.Attributes["y"]);
                Size size = new Size((int) level.Attributes["width"], (int) level.Attributes["height"]);
                Point segmentMax = point + size;
                if (point.X > minPoint.X) minPoint.X = point.X;
                if (point.Y > minPoint.Y) minPoint.Y = point.Y;
                if (size.Width > maxPoint.X) maxPoint.X = segmentMax.X;
                if (size.Height > maxPoint.Y) maxPoint.Y = segmentMax.Y;
                foreach (Parser.Element levelData in level.Children) {
                    switch (levelData.Name) {
                        case "solids": {
                            Console.WriteLine(levelData.Value);
                            break;
                        }
                    }
                }
            }
            break;
        }
    }
}

Size finalSize = (Size) maxPoint + (Size) minPoint;
Console.WriteLine($"{maxPoint}, {minPoint}, {finalSize}");
using Image<Rgba32> image = new Image<Rgba32>(Math.Abs(finalSize.Width), Math.Abs(finalSize.Height));
foreach (((int x, int y), byte sprite) in list) {
    image[x, y] = new Rgba32(sprite, sprite, sprite, 255);
}
image.SaveAsPng($"{args[1]}.png");
public record GamePoint(Point Point, byte Sprite);