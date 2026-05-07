using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "Color Palette")]
public class ColorPalette : ScriptableObject
{
    public enum PaletteColor { Color1, Color2, Color3, Color4 }

    [Header("Global Color Pickers")]
    public  Color color1 = Color.red;
    public  Color color2 = Color.green;
    public  Color color3 = Color.blue;
    public Color color4 = Color.yellow;
    public Texture2D iceTexture;

    public Color GetColor(PaletteColor slot)
    {
        return slot switch
        {
            PaletteColor.Color1 => color1,
            PaletteColor.Color2 => color2,
            PaletteColor.Color3 => color3,
            PaletteColor.Color4 => color4,
            _ => Color.white
        };
    }
}