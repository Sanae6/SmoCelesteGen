﻿using System.Buffers;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CelesteParser;

public class Atlas {
    public static string ContentBase { get; set; }
    public static Atlas FgTiles;
    private Dictionary<string, string> NameFileMap = new Dictionary<string, string>();
    private Dictionary<string, Image<Rgba32>> ImageCache = new Dictionary<string, Image<Rgba32>>();
    public static Atlas FromPath(string atlasPath, DataFormat dataFormat) {
        Atlas atlas = new Atlas();
        switch (dataFormat) {
            case DataFormat.Packer: {
                string path = $"{atlasPath}.meta";
                if (!File.Exists(path)) throw new FileNotFoundException($"{path} doesn't exist!");
                using Stream fs = new MemoryStream(File.ReadAllBytes(path));
                using BinaryReader reader = new BinaryReader(fs);
                reader.ReadInt32();
                reader.ReadString();
                reader.ReadInt32();
                int len = reader.ReadInt16();
                for (int i = 0; i < len; i++) {
                    string vtexPath = reader.ReadString();
                    Console.WriteLine($"Loading vtex {vtexPath}");
                    Image<Rgba32> vtex = Image.Load<Bgra32>(File.ReadAllBytes(Path.Combine(ContentBase, $"{vtexPath}.data")), new DataDecoder()).CloneAs<Rgba32>();
                    Console.WriteLine("Generating subimages");
                    short subLen = reader.ReadInt16();
                    for (int j = 0; j < subLen; j++) {
                        string baseName = reader.ReadString().Replace('\\', '/');
                        string texName = $"{baseName}.png";
                        atlas.NameFileMap[baseName] = texName;

                        Rectangle crop = new Rectangle(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                        Point drawOffset = new Point(reader.ReadInt16(), reader.ReadInt16());
                        Size drawSize = new Size(reader.ReadInt16(), reader.ReadInt16());

                        // if (File.Exists(texName)) continue;
                        //
                        // string? dirName = Path.GetDirectoryName(texName);
                        // if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
                        // Image<Rgba32> subTex = vtex.Clone();
                        // subTex.Mutate(x => x.Crop(crop));
                        // subTex.SaveAsPng(texName);
                    }
                }
                break;
            }
        }
        return atlas;
    }

    public Image<Rgba32> this[string location] {
        get {
            string file = NameFileMap[location];
            if (ImageCache.TryGetValue(file, out Image<Rgba32>? image))
                return image;
            return ImageCache[file] = Image.Load<Rgba32>(file);
        }
    }

    private class DataDecoder : IImageDecoder {
        public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel> {
            BinaryReader reader = new BinaryReader(stream);
            Image<TPixel> image = new Image<TPixel>(configuration, reader.ReadInt32(), reader.ReadInt32());
            bool hasAlpha = reader.ReadBoolean();

            Abgr32 val = default;
            TPixel finalVal = default;
            int repeats = 0;
            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    if (repeats == 0) {
                        repeats = reader.ReadByte() - 1;

                        if (hasAlpha) {
                            val.A = reader.ReadByte();
                            if (val.A > 0)
                                (val.B, val.G, val.R) = (reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            else
                                val = default;
                        } else {
                            val.A = 0;
                            (val.B, val.G, val.R) = (reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        }
                        finalVal.FromAbgr32(val);
                    } else repeats--;
                    image[x, y] = finalVal;
                }
            }
            return image;
        }
        public Image Decode(Configuration configuration, Stream stream, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }
    }

    public enum DataFormat {
        Packer,
        PackerNoAtlas
    }
}