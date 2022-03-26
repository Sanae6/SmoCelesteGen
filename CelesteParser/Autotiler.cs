using System.Xml;

namespace CelesteParser; 

public class Autotiler {
    public static string ContentBase { get; set; }
    public Dictionary<char, Tileset> TileLookup = new Dictionary<char, Tileset>();
    public Autotiler(string filename) {
        XmlDocument document = new XmlDocument();
        document.LoadXml(File.ReadAllText(Path.Combine(ContentBase, filename)));
        foreach (object elObj in document.GetElementsByTagName("Tileset")) {
            XmlElement tilesetElement = (XmlElement) elObj;
            char tileId = tilesetElement.AttrChar("id");
            if (tilesetElement.HasAttribute("copy")) {
                tilesetElement.GetAttribute("copy");
            }
            tilesetElement.GetAttribute("ignores");
            tilesetElement.GetAttribute("path");
        }
    }

    public class Tileset {
        
    }
}