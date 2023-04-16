using UnityEngine;

namespace TUX;

public class TUXProperty<T> : TUXProperty
{
    public int ID { get; internal set; }
    public T defaultValue;
    public T value { get; internal set; }

    public TUXProperty(string name, T defaultValue)
    {
        this.name = name;
        this.defaultValue = defaultValue;
        this.defaultObject = defaultValue;
        this.value = defaultValue;
        this.valueObject = defaultValue;
    }

    public virtual T GetValue()
    {
        return value;
    }
    public virtual void SetValue(T value)
    {
        this.value = value;
    }
    public override void Apply(ref Material material)
    {

    }

    public override void Set(object other)
    {
        value = (T)other;
        valueObject = other;
    }
    public override void ReadAndSet(Material material)
    {
        SetValue(Read(material));
    }
    public virtual T Read(Material material)
    {
        throw new NotImplementedException();
    }
    public override bool Draw()
    {
        return false;
    }

    public override void OnGUI() { }

    public override string ToString()
    {
        return $"{name}: {value} ({GetType().Name})";
    }
}