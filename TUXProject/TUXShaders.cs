using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TUX;

[Serializable]
public static partial class TUXShaders
{
    internal static List<string> defaultShadersPaths => defaultShaders.Select(a => a.shaderPath).ToList();
    public static readonly List<TUXShader> defaultShaders = new()
    {
        new("KSP2/Parts/Standard Damaged")
        {
            properties = new TUXProperty[]{
                new TUXTexture("_MainTex"),
                new TUXTexture("_MetallicGlossMap"),
                new TUXFloat("_Metallic"),
                new TUXFloat("_GlossMapScale", 1),
                new TUXFloat("_MipBias" , 0.8f),
                new TUXTexture("_BumpMap" , TUXTexture.TextureFormat.Bump),
                new TUXTexture("_OcclusionMap"),
                new TUXFloat("_OcclusionStrength", 1),
                new TUXTexture("_EmissionMap"),
                new TUXColor("_EmissionColor"),
                new TUXTexture("_PaintMaskGlossMap"),
                new TUXColor("_PaintA", Color.white),
                new TUXColor("_PaintB", Color.black),
                new TUXBool("_SmoothnessOverride"),
                new TUXFloat("_PaintGlossMapScale",1),
                new TUXTexture("_DamageMap"),
                new TUXFloat("_DamageFade", 1),
                new TUXFloat("_DamageScale", 1),
                new TUXFloat("_RimFalloff",0.1f, 0.01f, 5),
                new TUXColor("_RimColor")
            }
        },
        new("KSP2/Scenery/Standard (Opaque)")
        {
            properties = new TUXProperty[]{
                new TUXTexture("_MainTex"),
                new TUXTexture("_MetallicGlossMap"),
                new TUXFloat("_Metallic"),
                new TUXFloat("_GlossMapScale", 1),
                new TUXFloat("_MipBias" , 0.8f),
                new TUXTexture("_BumpMap" , TUXTexture.TextureFormat.Bump),
                new TUXTexture("_DetailBumpMap"),
                new TUXTexture("_DetailMask"),
                new TUXFloat("_DetailBumpScale", 1),
                new TUXFloat("_DetailBumpTiling", 1, 0.01f, 10),
                new TUXTexture("_OcclusionMap"),
                new TUXFloat("_OcclusionStrength", 1),
                new TUXTexture("_EmissionMap"),
                new TUXColor("_EmissionColor"),
                new TUXTexture("_PaintMaskGlossMap"),
                new TUXColor("_PaintA", Color.white),
                new TUXColor("_PaintB", Color.black),
                new TUXBool("_SmoothnessOverride"),
                new TUXFloat("_PaintGlossMapScale",1),
                new TUXFloat("_RimFalloff",0.1f, 0.01f, 5),
                new TUXColor("_RimColor")
            }
        },
        new("KSP2/VFX/Particles/VFX_Exhaust")
        {
            properties = new TUXProperty[]
            {
                new TUXFloat("_Alpha"),
                new TUXFloat("_ScrollSpeedX", -100, 100),
                new TUXFloat("_ScrollSpeedY", -100, 100),
                new TUXFloat("_ColorTintBoost"),
                new TUXColor("_ColorTintStart", new Color(1,0,0,1)),
                new TUXColor("_ColorTintMiddle", new Color(0,1,0,0)),
                new TUXColor("_ColorTintEnd", new Color(0,0,1,1)),
                new TUXFloat("_ColorTintOffset"),
                new TUXFloat("_ColorTintFalloff",1),
                new TUXFloat("_ColorTintMiddlePos",0.5f),
                new TUXFloat("_ColorTintEndOffset"),
                new TUXFloat("_ColorTintEndGradient",1),
                new TUXFloat("_NoiseAmount",1),
                new TUXFloat("_NoiseStrength",0.5f, 0.5f,5),
                new TUXFloat("_TextureOffsetX",0),
                new TUXFloat("_TextureOffsetY",0),
                new TUXFloat("_TextureScaleX",1),
                new TUXFloat("_TextureScaleY",1),
                new TUXTexture("_NoiseTexture"),
                new TUXFloat("_TdotVScale",3, 0.01f, 10),
                new TUXFloat("_FresnelOuter",10 , 0, 10),
                new TUXFloat("_FresnelOuterBeneath", 0, 10),
                new TUXFloat("_FresnelOuterErosionAmount",1),
                new TUXFloat("_FresnelOuterErosionOffset",0.282353f),
                new TUXFloat("_FresnelOuterErosionFalloff",0.282353f),
                new TUXFloat("_FresnelInner",10, 0, 10),
                new TUXFloat("_FresnelInnerBeneath",0, 0, 10),
                new TUXFloat("_TopGradientPosOffset"),
                new TUXFloat("_TopGradientFalloff",0.2439874f, 0,1f),
                new TUXFloat("_ErosionAmount",1),
                new TUXFloat("_ErosionPosOffset",0.2945153f, -1f, 1f),
                new TUXFloat("_ErosionFalloffGradient",0.2384171f, 0,5),
                new TUXFloat("_VertexDispScale",0, 10),
                new TUXFloat("_VertexDispContrast",.5f, .5f, 5),
                new TUXFloat("_VertexDispPosOffset"),
                new TUXFloat("_VertexDispFalloffGradient",0,3),
                new TUXTexture("_DistortionTexture"),
                new TUXFloat("_TracesLength", 1, 1,20),
                new TUXInt("_TracesCount", 3),
                new TUXFloat("_TracesThickness",2,.1f,4),
                new TUXFloat("_TracesStrength", 1,0,5),
                new TUXFloat("_TracesTopPosOffset",0.282353f, 0,1),
                new TUXFloat("_TracesTopFalloffGradient", .25f,0,2),
                new TUXTexture("_TracesTexture"),
                new TUXFloat("_CameraDistanceFadeLength", 50, 0, 50),
                new TUXFloat("_CameraDistanceFalloff",1, .01f, 2),
                new TUXFloat("_CameraDistanceTopGradient"),
                //"_AccelerationDir" ("AccelerationDir", Vector) = (0,0,0,0)
                new TUXFloat("_AccelerationScaleFactor", -100, 100),
                new TUXFloat("_BendCenterOffset",-100, 100),
                new TUXFloat("_BendRotationOffset",-100, 100),
                new TUXFloat("_BendCenterOffsetMultipler",-100, 100)
            }
        }
    };
    public static List<TUXShader> Shaders = new(); 

    public class TUXShader
    {
        public string shaderPath;
        public (string partName, string goName) target;
        public Shader shader => Shader.Find(shaderPath);
        public TUXProperty[] properties;

        public TUXShader(string shaderPath)
        {
            this.shaderPath = shaderPath;
        }

        public TUXShader Clone()
        {
            return this.MemberwiseClone() as TUXShader;
        }
        public void ApplyAll(Material material)
        {
            foreach (TUXProperty property in properties)
            {
                property.Apply(ref material);
            }
        }
        internal Material GetMaterial()
        {
            Material mat = new Material(shader);
            ApplyAll(mat);
            return mat;
        }

        internal void CopyFrom(Material m)
        {
            foreach (TUXProperty property in properties)
            {
                property.ReadAndSet(m);
            }
        }

        public override string ToString()
        {
            return shaderPath;
        }
        public override int GetHashCode()
        {
            return shaderPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            return Equals(obj as TUXShader);
        }
        private bool Equals(TUXShader other)
        {
            if (string.IsNullOrEmpty(other.shaderPath))
                return false;
            return shaderPath == other.shaderPath;
        }
    }

    internal static bool Contains(Material mat)
    {
        foreach (TUXShader shader in defaultShaders)
        {
            if (shader.shaderPath == mat.shader.name)
                return true;
        }
        return false;
    }
    internal static bool Contains(Shader shader)
    {
        foreach (TUXShader tuxShader in defaultShaders)
        {
            if (tuxShader.shaderPath == shader.name)
                return true;
        }
        return false;
    }

    internal static bool TryGetTUXEquivalent(Shader s, out TUXShader tuxS)
    {
        tuxS = null;
        if (Contains(s))
        {
            tuxS = GetTUXShader(s);
            return true;
        }
        return false;
    }

    internal static TUXShader GetTUXShader(Shader s)
    {
        var shader = defaultShaders.Find(a => a.shaderPath == s.name).Clone();

        return shader;
    }
    internal static TUXShader ReadAndApplyToNew(Material m)
    {
        var shader = defaultShaders.Find(a => a.shaderPath == m.shader.name).Clone();

        shader.CopyFrom(m);

        return shader;
    }

    internal static TUXShader GetTUXShader(string shaderPath)
    {
        return defaultShaders.Find(a => a.shaderPath == shaderPath).Clone();
    }
}
public static class GUIHelpers
{
    public static readonly char[] SignSymbols = "+-".ToCharArray();
    public static readonly char[] Numbers = "0123456789".ToCharArray();
    public static readonly char FloatSeparator = ',';
    public static readonly char[] NumericSignals = "-+.,".ToCharArray();
    public static int IntField(int value, float Min, float Max)
    {
        int f = value;
        string toParse = GUILayout.TextField(f.ToString());

        if (string.IsNullOrEmpty(toParse))
        {
            return 0;
        }

        toParse = toParse.Trim();
        string newString = string.Empty;
        for (int i = 0; i < toParse.Length; i++)
        {
            char c = toParse[i];

            if (Numbers.Contains(c))
            {
                newString += c;
                continue;
            }

            if (SignSymbols.Contains(c) && i == 0)
            {
                newString += c;
                continue;
            }
        }

        f = Mathf.RoundToInt(Mathf.Clamp(int.Parse(newString), Min, Max));

        return f;
    }
    public static float FloatField(float value, float Min, float Max)
    {
        float f = value;
        string toParse = GUILayout.TextField(f.ToString());

        if (string.IsNullOrEmpty(toParse))
        {
            return 0;
        }

        toParse = toParse.Trim();
        string newString = string.Empty;
        bool firstCharIsSign = false;
        bool hasSeparator = false;

        for (int i = 0; i < toParse.Length; i++)
        {
            char c = toParse[i];

            if (Numbers.Contains(c))
            {
                newString += c;
                continue;
            }


            if (SignSymbols.Contains(c) && i == 0)
            {
                newString += c;
                firstCharIsSign = true;
                continue;
            }


            if ((c == FloatSeparator && i != 0) || (c == FloatSeparator && i > 1 && firstCharIsSign) && !hasSeparator)
            {
                newString += c;
                hasSeparator = true;
                continue;
            }
        }
        if (toParse.EndsWith(","))
            toParse.Remove(toParse.Length - 1);

        f = Mathf.Clamp(float.Parse(newString), Min, Max);

        return f;
    }

    public static bool Dropdown(bool dropdownState)
    {
        if (GUILayout.Button(dropdownState ? "▼" : "▲", new GUIStyle(SpaceWarp.API.UI.Skins.ConsoleSkin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter }))
            return !dropdownState;
        return dropdownState;
    }
    public static bool Dropdown(string label, bool dropdownState)
    {
        if (GUILayout.Button(dropdownState ? $"{label}▼" : $"{label}▲", new GUIStyle(SpaceWarp.API.UI.Skins.ConsoleSkin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter }))
            return !dropdownState;
        return dropdownState;
    }
    public static bool Dropdown(string label, bool dropdownState, bool BothSides)
    {
        if (GUILayout.Button(dropdownState ? $"▼{label}▼" : $"▲{label}▲", new GUIStyle(SpaceWarp.API.UI.Skins.ConsoleSkin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter }))
            return !dropdownState;
        return dropdownState;
    }
}