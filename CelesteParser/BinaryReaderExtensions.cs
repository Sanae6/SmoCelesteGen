using System.Text;

namespace CelesteParser;

public static class BinaryReaderExtensions {
    public static string ReadRLD(this BinaryReader reader) {
        StringBuilder builder = new StringBuilder();
        byte[] data = reader.ReadBytes(reader.ReadInt16());
        for (int i = 0; i < data.Length; i += 2)
            builder.Append((char) data[i + 1], data[i]);
        return builder.ToString();
    }
}