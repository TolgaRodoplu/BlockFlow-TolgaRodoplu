using System;

[Serializable]
public class FloorEntry
{
    public int x;
    public int y;
}

[Serializable]
public class PlacedObjectEntry : FloorEntry
{
    public string typeName;
    public string direction;
}

[Serializable]
public class ColoredObjectEntry : PlacedObjectEntry
{
    public string color;
}

[Serializable]
public class BlockEntry : ColoredObjectEntry
{
    public int iceCounter;        
    public string restrictedAxis; 
}

[Serializable]
public class LevelData
{
    public int gridWidth;
    public int gridHeight;
    public FloorEntry[] floors;
    public PlacedObjectEntry[] walls;
    public ColoredObjectEntry[] grinders;
    public BlockEntry[] blocks;
    public int seconds;
}
