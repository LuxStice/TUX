using UnityEngine;

namespace TUX;

public abstract class TUXProperty
{
    public string name { get; internal set; }
    public int ID { get; internal set; }
    public object defaultObject { get; internal set; }
    public object valueObject { get; internal set; }
    public abstract void Apply(ref Material material);
    public abstract bool Draw();
    public abstract void ReadAndSet(Material material);
    public abstract void Set(object other);

    public abstract void OnGUI();
}
