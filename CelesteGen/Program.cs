using CelesteParser;
using Newtonsoft.Json;
using Syroot.NintenTools.Byaml;

Parser.Level level = Parser.Parse(File.ReadAllBytes(args[0]));

File.WriteAllText($"{args[0]}.json", JsonConvert.SerializeObject(level, Formatting.Indented));