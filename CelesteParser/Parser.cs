namespace CelesteParser;

public class Parser {
    private string[] StringTable;
    private BinaryReader Reader;
    private string Package;
    public string FileName;
    private Parser() { }

    public static Level Parse(byte[] data) {
        BinaryReader reader = new BinaryReader(new MemoryStream(data));
        Parser parser = new Parser {
            Reader = reader,
            FileName = reader.ReadString(),
            Package = reader.ReadString(),
            StringTable = Enumerable
                .Repeat(0, reader.ReadInt16())
                .Select(_ => reader.ReadString())
                .ToArray(),
        };
        return new Level {
            FileName = parser.FileName,
            Package = parser.Package,
            Root = parser.ReadElement()
        };
    }

    private string GetStringRead() => StringTable[Reader.ReadInt16()];

    private Element ReadElement() {
        Element element = new Element {
            Name = GetStringRead(),
            Attributes = Enumerable
                .Repeat(0, Reader.ReadByte())
                .Select(_ => (
                    name: GetStringRead(),
                    value: Reader.ReadByte() switch {
                        0 => Reader.ReadBoolean(),
                        1 => Convert.ToInt32(Reader.ReadByte()),
                        2 => Convert.ToInt32(Reader.ReadInt16()),
                        3 => Reader.ReadInt32(),
                        4 => Reader.ReadSingle(),
                        5 => GetStringRead(),
                        6 => Reader.ReadString(),
                        7 => (object) Reader.ReadRLD(),
                        _ => throw new ArgumentOutOfRangeException()
                    }))
                .ToDictionary(x => x.name, x => x.value),
            Children = Enumerable
                .Repeat(0, Reader.ReadInt16())
                .Select(_ => ReadElement())
                .ToList()
        };
        if (element.Attributes.TryGetValue("innerText", out object? innerText)) {
            element.Value = innerText;
            element.Attributes.Remove("innerText");
        }

        return element;
    }

    public class Level {
        public string FileName { get; set; }
        public string Package { get; set; }
        public Element Root { get; set; }
    }

    public class Element {
        public string Name { get; set; }
        public object? Value { get; set; }
        public Dictionary<string, object>? Attributes { get; init; }
        public List<Element>? Children { get; init; }
    }
}