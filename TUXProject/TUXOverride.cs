using UnityEngine;

namespace TUX;

public partial class TUXPlugin
{
    public class TUXOverride
    {
        public TUXShaders.TUXShader baseShader { get; private set; }
        public List<TUXProperty> overrides = new();
        public (string partName, string targetGameObject) target;

        public TUXOverride(TUXShaders.TUXShader tUXShader)
        {
            this.baseShader = tUXShader;
        }

        public Material GetMaterial()
        {
            Material m = baseShader.GetMaterial();
            foreach(var over in overrides)
            {
                over.Apply(ref m);
            }
            return m;
        }
    }
}
