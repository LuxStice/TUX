using UnityEngine;

namespace TUX;

public class TUXFloat : TUXProperty<float>
{
    public float rangeMin = 0, rangeMax = 1;
    public float Min => rangeMin;
    public float Max => rangeMax;

    public TUXFloat(string name) : base(name, 0)
    {
        this.name = name;
    }

    public TUXFloat(string name, float defaultValue) : base(name, defaultValue)
    {
        this.name = name;
    }
    public TUXFloat(string name, float rangeMin, float rangeMax) : base(name, 0)
    {
        this.name = name;
        this.rangeMin = rangeMin;
        this.rangeMax = rangeMax;
    }
    public TUXFloat(string name, float defaultValue, float rangeMin, float rangeMax) : base(name, defaultValue)
    {
        this.name = name;
        this.rangeMin = rangeMin;
        this.rangeMax = rangeMax;
    }

    public override void SetValue(float value)
    {
        this.value = Mathf.Clamp(value, rangeMin, rangeMax);
    }

    public override void Apply(ref Material material)
    {
        material.SetFloat(name, value);
    }
    public override float Read(Material material)
    {
        return material.GetFloat(name);
    }

    public override bool Draw()
    {
        float currentFloat = value;
        GUILayout.Label($"{name} ({value})");
        float newFloat = GUILayout.HorizontalSlider(currentFloat, Min, Max);

        if (newFloat != currentFloat)
        {
            SetValue(newFloat);
            return true;
        }
        return false;
    }
}