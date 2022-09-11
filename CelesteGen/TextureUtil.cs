using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using BfresLibrary;
using BfresLibrary.Switch;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Syroot.Maths;
using Syroot.NintenTools.NSW.Bntx;

namespace CelesteGen;

public static class TextureUtil {
    private const int MipCount = 1;
    private static BcEncoder Encoder = new BcEncoder(CompressionFormat.Bc1WithAlpha) {
        OutputOptions = {MaxMipMapLevel = MipCount, Quality = CompressionQuality.BestQuality}
    };
    private static readonly int MipProd = (int) Math.Floor(Math.Pow(2, MipCount));
    public static Vector4F GetScaledPosition(this Vector2F position, Size textureSize) {
        position.X *= MipProd;
        position.Y *= MipProd;
        position.X /= textureSize.Width;
        position.Y /= textureSize.Height;
        return new Vector4F(position.X, position.Y, 0f, 0f);
    }
    public static void SwizzleMipmap(this SwitchTexture texture, Image<Rgba32> image) {
        texture.Width = (uint) image.Width * (uint) MipProd;
        texture.Height = (uint) image.Height * (uint) MipProd;
        Image<Rgba32> final = image.Clone(x => x.Resize(new ResizeOptions {
            Size = image.Size() * MipProd,
            Sampler = new NearestNeighborResampler()
        }));

        byte[][] mipMaps = Encoder.EncodeToRawBytes(final);
        texture.MipCount = MipCount;
        texture.SwizzleImage(mipMaps.SelectMany(x => x).ToArray());
    }

    public static SwitchTexture AddTexture(this ResFile bfres, BntxFile bntx, Image<Rgba32> image, string name) {
        SwitchTexture test = new SwitchTexture(bntx, new Texture());
        test.Import("temp.bftex", bfres);
        test.Name = name;
        test.SwizzleMipmap(image);
        bfres.Textures.Add(name, test);

        bntx.Textures.Add(test.Texture);
        return test;
    }

    public static ushort AddMaterial(this Model model, ResFile bfres, BntxFile bntx, Image<Rgba32> image,
        string name, out Size textureSize) {
        Material mat = new Material();
        mat.Import("temp.bfmat", bfres);
        mat.Name = name;
        mat.TextureRefs.Clear();
        SwitchTexture texture = bfres.AddTexture(bntx, image, name);
        textureSize = new Size((int) texture.Width, (int) texture.Height);
        mat.TextureRefs.Add(new TextureRef {
            Name = name,
            Texture = texture
        });
        model.Materials.Add(name, mat);
        return (ushort) model.Materials.IndexOf(mat);
    }
}