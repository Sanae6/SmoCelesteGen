using System.Text;
using System.Xml;

namespace CelesteParser;

public static class Extensions {
    public static string ReadRLD(this BinaryReader reader) {
        StringBuilder builder = new StringBuilder();
        byte[] data = reader.ReadBytes(reader.ReadInt16());
        for (int i = 0; i < data.Length; i += 2)
            builder.Append((char) data[i + 1], data[i]);
        return builder.ToString();
    }

    public static char AttrChar(this XmlElement element, string name) => Convert.ToChar(element.GetAttribute(name));
}