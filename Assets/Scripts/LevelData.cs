using System;

[Serializable]
public class PlacedObjectEntry
{
    public string typeName;
    public int x;
    public int y;
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
    public PlacedObjectEntry[] walls;
    public ColoredObjectEntry[] grinders;
    public BlockEntry[] blocks;
}
