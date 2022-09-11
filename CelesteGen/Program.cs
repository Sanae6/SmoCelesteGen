using System.Diagnostics;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared.ImageFiles;
using BfresLibrary;
using BfresLibrary.Switch;
using CelesteGen;
using CelesteParser;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Syroot.NintenTools.NSW.Bntx;

Parser.Level levelRoot = Parser.Parse(File.ReadAllBytes(args[1]));

File.WriteAllText($"{args[1]}.json", JsonConvert.SerializeObject(levelRoot, Formatting.Indented));
Atlas.ContentBase = Path.Combine(args[0], "Graphics", "Atlases");
Autotiler.ContentBase = Path.Combine(args[0], "Graphics");
Atlas.FgTiles = Atlas.FromPath($"{args[0]}/Graphics/Atlases/Gameplay", Atlas.DataFormat.Packer);
Autotiler fgTiler = new Autotiler("ForegroundTiles.xml");

Map map = new Map();
foreach (Parser.Element rootChild in levelRoot.Root.Children) {
    switch (rootChild.Name) {
        case "levels": {
            foreach (Parser.Element level in rootChild.Children) {
                Point point = new Point((int) level.Attributes["x"] / 8, (int) level.Attributes["y"] / 8);
                Size size = new Size((int) level.Attributes["width"] / 8, (int) level.Attributes["height"] / 8);
                Point segmentMax = point + size;
                Console.WriteLine($"level {point}, {size}, {segmentMax}");
                map.AddLevel(new Level(new Rectangle(point, size), level));
            }
            break;
        }
    }

}

ResFile bfres = new ResFile("Templates/Theater2DExGround000.bfres");
BntxFile bntx = ((SwitchTexture) bfres.Textures[0]).BntxFile;
bfres.Textures[0].Export("temp.bftex", bfres);
bfres.Textures.Clear();
bfres.Models[0].Materials[0].Export("temp.bfmat", bfres);
bfres.Models[0].Materials.Clear();
bntx.Textures.Clear();

Model model = bfres.Models[0];

fgTiler.GenerateModel(bfres, bntx, model, map);

using MemoryStream ms = new MemoryStream();
bntx.Save(ms);
File.WriteAllBytes("TheaterTest.bntx", bfres.ExternalFiles[0].Data = ms.ToArray());
new BntxFile("TheaterTest.bntx");
bfres.Save("TheaterTest.bfres");