using UnityEngine;

namespace TUX;

public class TUXBool : TUXProperty<bool>
{
    public TUXBool(string name) : base(name, false)
    {
    }
    public TUXBool(string name, bool defaultValue) : base(name, defaultValue)
    {
    }

    public override void Apply(ref Material material)
    {
        material.SetFloat(name, value == false ? 0 : 1);
    }
    public override bool Draw()
    {
        bool currentBool = value;
        GUILayout.Label($"{name} ({value})");
        bool newBool = GUILayout.Toggle(currentBool, new GUIContent(name), GUI.skin.toggle);

        if (newBool != currentBool)
        {
            SetValue(newBool);
            return true;
        }
        return false;
    }
    public override bool Read(Material material)
    {
        return material.GetFloat(name) == 1 ? true : false;
    }
}
