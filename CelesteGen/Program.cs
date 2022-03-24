using BfresLibrary;
using CelesteParser;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Syroot.NintenTools.Byaml;

Parser.Level levelRoot = Parser.Parse(File.ReadAllBytes(args[0]));

File.WriteAllText($"{args[0]}.json", JsonConvert.SerializeObject(levelRoot, Formatting.Indented));

List<GamePoint> list = new List<GamePoint>();
Size size = Size.Empty;
// var resFile = new ResFile();
foreach (Parser.Element rootChild in levelRoot.Root.Children) {
    switch (rootChild.Name) {
        case "levels": {
            foreach (Parser.Element level in rootChild.Children) {
                Size subSize = new Size((int) level.Attributes["width"], (int) level.Attributes["height"]);
                if (size.Width < subSize.Width) size.Width = subSize.Width;
                if (size.Width < subSize.Width) size.Width = subSize.Width;
            }
            break;
        }
    }
}

using Image<Rgba32> image = new Image<Rgba32>(size.Width, size.Height);
public record GamePoint(SizeF Size, Point Point, byte Sprite);