using UnityEngine;

namespace TUX;

internal class TUXColor : TUXProperty<Color>
{
    internal int r => Mathf.RoundToInt(value.r * 255);
    internal int g => Mathf.RoundToInt(value.g * 255);
    internal int b => Mathf.RoundToInt(value.b * 255);
    internal int a => Mathf.RoundToInt(value.a * 255);

    public TUXColor(string name) : base(name, Color.white)
    {
    }
    public TUXColor(string name, Color defaultValue) : base(name, defaultValue)
    {
    }
    public override void Apply(ref Material material)
    {
        material.SetColor(name, value);
    }
    public override Color Read(Material material)
    {
        return material.GetColor(name);
    }

    public override bool Draw()
    {
        GUILayout.Label($"{name} ({value})");

        int r = this.r, g = this.g, b = this.b, a = this.a;

        GUILayout.BeginHorizontal();
        GUILayout.Label("r");
        r = GUIHelpers.IntField(r, 0, 255);
        //r = GUILayout.VerticalSlider(r, 0, 1);
        GUILayout.Label("g");
        g = GUIHelpers.IntField(g, 0, 255);
        GUILayout.Label("b");
        b = GUIHelpers.IntField(b, 0, 255);
        GUILayout.Label("a");
        a = GUIHelpers.IntField(a, 0, 255);
        var colorPreview = new Texture2D(32, 32);
        Color color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                colorPreview.SetPixel(x, y, color);
            }
        }
        colorPreview.Apply(false, false);
        GUILayout.Box(colorPreview, GUILayout.Width(32), GUILayout.Height(32));
        GUILayout.EndHorizontal();

        bool different = false;

        if (Math.Abs(r - this.r) > 0.005f)
        {
            different = true;
        }
        if (Math.Abs(g - this.g) > 0.005f)
        {
            different = true;
        }
        if (Math.Abs(b - this.b) > 0.005f)
        {
            different = true;
        }
        if (Math.Abs(a - this.a) > 0.005f)
        {
            different = true;
        }

        if (different)
        {
            this.SetValue(color);
            return true;
        }
        return false;
    }

}