namespace CelesteParser;

public class FullMapTilemap : Tilemap {
    private Map Map;
    public override int Width { get; }
    public override int Height { get; }

    public FullMapTilemap(Map map) : base(map.Bounds) {
        Map = map;
        Initialize();
        foreach (Level level in map.Levels) {
            
        }
    }
}