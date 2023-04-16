using UnityEngine;

namespace TUX;

public class TUXInt : TUXProperty<int>
{
    public int rangeMin = 0, rangeMax = 10;
    public int Min => rangeMin;
    public int Max => rangeMax;

    public TUXInt(string name) : base(name, 0)
    {
        this.name = name;
    }

    public TUXInt(string name, int defaultValue) : base(name, defaultValue)
    {
        this.name = name;
    }
    public TUXInt(string name, int rangeMin, int rangeMax) : base(name, 0)
    {
        this.name = name;
        this.rangeMin = rangeMin;
        this.rangeMax = rangeMax;
    }
    public TUXInt(string name, int defaultValue, int rangeMin, int rangeMax) : base(name, defaultValue)
    {
        this.name = name;
        this.rangeMin = rangeMin;
        this.rangeMax = rangeMax;
    }

    public override void SetValue(int value)
    {
        this.value = Mathf.Clamp(value, rangeMin, rangeMax);
    }

    public override void Apply(ref Material material)
    {
        material.SetInt(name, value);
    }
    public override int Read(Material material)
    {
        return material.GetInt(name);
    }
    public override bool Draw()
    {
        int currentInt = value;
        GUILayout.Label($"{name} ({value})");
        int newInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(currentInt, Min, Max));

        if (newInt != currentInt)
        {
            SetValue(newInt);
            return true;
        }
        return false;
    }
}