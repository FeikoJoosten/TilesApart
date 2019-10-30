using UnityEngine;

[System.Serializable]
public struct TileDirections {
    public TileDirections(bool n, bool e, bool s, bool w) {
        north = n;
        east = e;
        south = s;
        west = w;
    }

    public bool[] GetDirections() {
        return new bool[4] { north, east, south, west };
    }

    public bool north;
    public bool east;
    public bool south;
    public bool west;

    // A bit overkill, but it is a lot more clear.
}

[CreateAssetMenu(menuName = "ScriptableObjects/Tile Data")]
public class TileData : ScriptableObject {
    [Header("General")]
    public string[] materialColorNames = new string[4] { "color1", "color2", "color3", "color4" };
    private int? materialColorNamesLength;
    public int MaterialColorNamesLength {
        get {
            if (!materialColorNamesLength.HasValue)
                materialColorNamesLength = materialColorNames.Length;
            return materialColorNamesLength.Value;
        }
    }
    public string fragmentationName = "Vector1_DB7200D4";
    public string playerFragmentationName = "Vector1_E54EF551";
    public int materialCount = 4;

    [Header("Straight")]
    public Mesh straightTileMesh;
    public Material[] straightTileMaterials = new Material[1];
    public Color[] straightTileLowlightColors = new Color[4];
    private int? straightTileLowlightColorsLength;
    public int StraightTileLowlightColorsLength {
        get {
            if(!straightTileLowlightColorsLength.HasValue)
                straightTileLowlightColorsLength = materialColorNames.Length;
            return straightTileLowlightColorsLength.Value;
        }
    }
    public TileDirections straightDirections = new TileDirections(true, false, true, false);

    [Header("Curve")]
    public Mesh curveTileMesh;
    public Material[] curveTileMaterials = new Material[1];
    public Color[] curveTileLowlightColors = new Color[4];
    private int? curveTileLowlightColorsLength;
    public int CurveTileLowlightColorsLength{
        get {
            if(!curveTileLowlightColorsLength.HasValue)
                curveTileLowlightColorsLength = materialColorNames.Length;
            return curveTileLowlightColorsLength.Value;
        }
    }
    public TileDirections curveDirections = new TileDirections(true, false, false, true);

    [Header("Crossing")]
    public Mesh crossingTileMesh;
    public Material[] crossingTileMaterials = new Material[1];
    public Color[] crossingTileLowlightColors = new Color[4];
    private int? crossingTileLowlightColorsLength;
    public int CrossingTileLowlightColorsLength {
        get {
            if(!crossingTileLowlightColorsLength.HasValue)
                crossingTileLowlightColorsLength = materialColorNames.Length;
            return crossingTileLowlightColorsLength.Value;
        }
    }
    public TileDirections crossingDirections = new TileDirections(true, true, false, true);

    [Header("Start")]
    public Mesh startTileMesh;
    public Material[] startTileMaterials = new Material[1];
    public Color[] startTileLowlightColors = new Color[4];
    private int? startTileLowlightColorsLength;
    public int StartTileLowlightColorsLength {
        get {
            if(!startTileLowlightColorsLength.HasValue)
                startTileLowlightColorsLength = materialColorNames.Length;
            return startTileLowlightColorsLength.Value;
        }
    }
    public TileDirections startDirections = new TileDirections(true, false, false, false);

    [Header("End")]
    public Mesh endTileMesh;
    public Material[] endTileMaterials = new Material[1];
    public Color[] endTileLowlightColors = new Color[4];
    private int? endTileLowlightColorsLength;
    public int EndTileLowlightColorsLength {
        get {
            if(!endTileLowlightColorsLength.HasValue)
                endTileLowlightColorsLength = materialColorNames.Length;
            return endTileLowlightColorsLength.Value;
        }
    }
    public TileDirections endDirections = new TileDirections(true, false, false, false);

    [Header("Pathless")]
    public Mesh pathlessTileMesh;
    public Material[] pathlessTileMaterials = new Material[1];
    public Color[] pathlessTileLowlightColors = new Color[4];
    private int? pathlessTileLowlightColorsLength;
    public int PathlessTileLowlightColorsLength {
        get {
            if(!pathlessTileLowlightColorsLength.HasValue)
                pathlessTileLowlightColorsLength = materialColorNames.Length;
            return pathlessTileLowlightColorsLength.Value;
        }
    }
    public TileDirections pathlessDirection = new TileDirections(false, false, false, false);
}
