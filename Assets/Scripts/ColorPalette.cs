using UnityEngine;

[CreateAssetMenu(fileName = "NewColorPalette", menuName = "Color Palette")]
public class ColorPalette : ScriptableObject
{
    public enum PaletteColor { Color1, Color2, Color3, Color4, Color5, Color6, Color7, Color8, Color9, Color10}

    [Header("Global Color Pickers")]
    public  Color color1 = Color.red;
    public  Color color2 = Color.green;
    public  Color color3 = Color.blue;
    public Color color4 = Color.yellow;
    public Color color5 = Color.white;
    public Color color6 = Color.white;
    public Color color7 = Color.white;
    public Color color8 = Color.white;
    public Color color9 = Color.white;
    public Color color10 = Color.white;
    public Texture2D iceTexture;

    public Color GetColor(PaletteColor slot)
    {
        return slot switch
        {
            PaletteColor.Color1 => color1,
            PaletteColor.Color2 => color2,
            PaletteColor.Color3 => color3,
            PaletteColor.Color4 => color4,
            PaletteColor.Color5 => color5,
            PaletteColor.Color6 => color6,
            PaletteColor.Color7 => color7,
            PaletteColor.Color8 => color7,
            PaletteColor.Color9 => color7,
            PaletteColor.Color10 => color7,
            _ => Color.white
        };
    }
}