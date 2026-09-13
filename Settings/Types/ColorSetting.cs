using System.Numerics;

public class ColorSetting : Setting
{
    public Vector4 Value { get; set; }

    public ColorSetting(string name, Vector4 color, string description = "")
        : base(name, description)
    {
        Value = color;
    }
}