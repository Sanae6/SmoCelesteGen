using System.Numerics;
using System.Text;

namespace CelesteGen;

public class ObjGen {
    private readonly StringBuilder Builder = new StringBuilder();
    public void Vertex(Vector3 position) {
        Builder.AppendLine($"v {position.X} {position.Y} {position.Z}");
    }
    public void Face(uint index1, uint index2, uint index3) {
        Builder.AppendLine($"f {index1 + 1} {index2 + 1} {index3 + 1}");
    }
    public override string ToString() {
        return Builder.ToString();
    }
}