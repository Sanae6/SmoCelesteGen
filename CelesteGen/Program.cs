using BfresLibrary;
using CelesteParser;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

Parser.Level levelRoot = Parser.Parse(File.ReadAllBytes(args[1]));

File.WriteAllText($"{args[1]}.json", JsonConvert.SerializeObject(levelRoot, Formatting.Indented));
Atlas.ContentBase = Path.Combine(args[0], "Graphics", "Atlases");
Autotiler.ContentBase = Path.Combine(args[0], "Graphics");
Atlas.FgTiles = Atlas.FromPath($"{args[0]}/Graphics/Atlases/Gameplay", Atlas.DataFormat.Packer);
Autotiler fgTiler = new Autotiler("ForegroundTiles.xml");

List<GamePoint> list = new List<GamePoint>();
Point minPoint = Point.Empty;
Point maxPoint = Point.Empty;
Map map = new Map();
foreach (Parser.Element rootChild in levelRoot.Root.Children) {
    switch (rootChild.Name) {
        case "levels": {
            foreach (Parser.Element level in rootChild.Children) {
                Point point = new Point((int) level.Attributes["x"] / 8, (int) level.Attributes["y"] / 8);
                Size size = new Size((int) level.Attributes["width"] / 8, (int) level.Attributes["height"] / 8);
                Point segmentMax = point + size;
                Console.WriteLine($"level {point}, {size}, {segmentMax}");
                if (point.X > minPoint.X) minPoint.X = point.X;
                if (point.Y > minPoint.Y) minPoint.Y = point.Y;
                if (size.Width > maxPoint.X) maxPoint.X = segmentMax.X;
                if (size.Height > maxPoint.Y) maxPoint.Y = segmentMax.Y;
                map.Levels.Add(new Level(new Rectangle(point, size), level));
            }
            break;
        }
    }
}

Size finalSize = (Size) maxPoint + (Size) minPoint;
Console.WriteLine($"{maxPoint}, {minPoint}, {finalSize}");
// using Image<Rgba32> image = new Image<Rgba32>(Math.Abs(finalSize.Width), Math.Abs(finalSize.Height));
// foreach (((int x, int y), byte sprite) in list) {
//     image[x, y] = new Rgba32(sprite, sprite, sprite, 255);
// }
// image.SaveAsPng($"{args[1]}.png");
foreach (Level level in map.Levels) {
    Console.WriteLine(level.Tilemap);
}

// ResFile file = new ResFile {
//     IsPlatformSwitch = true,
//     Name = "testfres",
//     VersionMajor = 0,
//     VersionMajor2 = 8,
//     VersionMinor = 0,
//     VersionMinor2 = 0
// };
// Model[] models = fgTiler.GenerateModels(map);
// int piss = 0;
// // foreach (Model model in models) file.Models.Add($"piss-{piss++}", model);
// file.Save("testfres.bfres");
// new ResFile("testfres.bfres");

public record GamePoint(Point Point, byte Sprite);
