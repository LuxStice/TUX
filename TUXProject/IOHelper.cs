using AwesomeTechnologies.External.ClipperLib;
using KSP.Game;
using System;
using UnityEngine;
using UnityEngine;
using static TUX.TUXPlugin;
using static TUX.TUXShaders;

namespace TUX;

public static class IOHelper
{
    public static void LoadPrefabAsync(string partName)
    {
        bool calledBack = false;
        while (!calledBack) { }
    }
    public static TUXOverride ParseOverride(string[] toParse)
    {
        TUXOverride Override = null;
        ParseStage stage = ParseStage.None;
        for (int i = 0; i < toParse.Length; i++)
        {
            if (string.IsNullOrEmpty(toParse[i]))
                continue;
            string[] reading = toParse[i].Trim().Split('=');

            KeyValuePair<string, string> kvp = new(reading[0].Trim(), reading.Length <= 1 ? string.Empty : reading[1].Trim());

            switch (kvp.Key)
            {
                case "SHADER":
                    Override = new TUXOverride(TUXShaders.Shaders.First(a => a.shaderPath == kvp.Value.Trim()));
                    stage = ParseStage.SettingShader;
                    break;
                case "OVERRIDE":
                    string target = kvp.Value.Trim();
                    (string partName, string goName) partGoPair = new();
                    partGoPair.partName = target.Split(',')[0].Replace("(", string.Empty).Trim();
                    partGoPair.goName = target.Split(',')[1].Replace(")", string.Empty).Trim();

                    string key = partNameToPrefab(partGoPair.partName);
                    DateTime callTime = DateTime.Now;
                    GameManager.Instance.Game.Assets.Load<GameObject>(key, (a) =>
                    {
                        Debug.Log($"loading prefab {key} took {(DateTime.Now - callTime).TotalSeconds}s to load");
                        TUXPlugin.LoadPrefabCallback(key, a);
                        Renderer renderer = a.GetComponentInChildren<Renderer>();
                        if (renderer.material.shader.name == Override.baseShader.shaderPath)
                            Override.baseShader.CopyFrom(renderer.sharedMaterial);
                    });
                    Override.target = partGoPair;
                    stage = ParseStage.OverrideMaterial;
                    break;
                case "{":
                case "}":
                    break;
                default:
                    try
                    {
                        if (stage == ParseStage.OverrideMaterial)
                        {
                            if (kvp.Value.Trim().EndsWith("f") && float.TryParse(kvp.Value.Trim().Replace("f", string.Empty), out float FLOAT))
                            {
                                Override.overrides.Add(new TUXFloat(kvp.Key.Trim(), FLOAT));
                            }
                            else if (int.TryParse(kvp.Value, out int INT))
                            {
                                Override.overrides.Add(new TUXInt(kvp.Key.Trim(), INT));
                            }
                            else if (bool.TryParse(kvp.Value, out bool BOOL))
                            {
                                Override.overrides.Add(new TUXBool(kvp.Key.Trim(), BOOL));
                            }
                            else if (kvp.Value.Count(a => a == ',') == 3)
                            {
                                string colorParse = kvp.Value;
                                string[] rgba = colorParse.Split(',');
                                float r, g, b, a;

                                r = Mathf.Clamp(float.Parse(rgba[0].Trim()), 0, 255);
                                g = Mathf.Clamp(float.Parse(rgba[1].Trim()), 0, 255);
                                b = Mathf.Clamp(float.Parse(rgba[2].Trim()), 0, 255);
                                a = Mathf.Clamp(float.Parse(rgba[3].Trim()), 0, 255);

                                r /= 255;
                                g /= 255;
                                b /= 255;
                                a /= 255;

                                Override.overrides.Add(new TUXColor(kvp.Key.Trim(), new Color(r, g, b, a)));
                            }
                            else
                            {
                                if (kvp.Value.ToLower() == "null")
                                    Override.overrides.Add(new TUXTexture(null));
                                else if (SpaceWarp.API.Assets.AssetManager.TryGetAsset(kvp.Value.Trim(), out Texture2D texture))
                                {
                                    texture.name = kvp.Value.Trim();
                                    Override.overrides.Add(new TUXTexture(kvp.Key.Trim(), texture,
                                        kvp.Value.Trim().ToLower().EndsWith("_n.png") ? TUXTexture.TextureFormat.Bump : TUXTexture.TextureFormat.Color));
                                }
                                else
                                {
                                    string textureID = string.Empty;
                                    string partName = kvp.Value;

                                    if (kvp.Value.ToLower().EndsWith("_d"))
                                    {
                                        textureID = DIFFUSE_ID;
                                        partName = partName.Remove(partName.Length - 2);
                                    }
                                    else if (kvp.Value.ToLower().EndsWith("_m"))
                                    {
                                        textureID = METALLIC_ID;
                                        partName = partName.Remove(partName.Length - 2);
                                    }
                                    else if (kvp.Value.ToLower().EndsWith("_n"))
                                    {
                                        textureID = BUMP_ID;
                                        partName = partName.Remove(partName.Length - 2);
                                    }
                                    else if (kvp.Value.ToLower().EndsWith("_ao"))
                                    {
                                        textureID = OCCLUSION_ID;
                                        partName = partName.Remove(partName.Length - 3);
                                    }
                                    else if (kvp.Value.ToLower().EndsWith("_e"))
                                    {
                                        textureID = EMISSION_ID;
                                        partName = partName.Remove(partName.Length - 2);
                                    }
                                    else if (kvp.Value.ToLower().EndsWith("_pm"))
                                    {
                                        textureID = PAINTMAP_ID;
                                        partName = partName.Remove(partName.Length - 3);
                                    }

                                    string tkey = partNameToPrefab(partName);

                                    GameManager.Instance.Game.Assets.Load<GameObject>(tkey, (a) =>
                                    {
                                        if (a != default && a is not null)
                                        {
                                            Texture2D texture = GetTextureFromPrefab(a, textureID);
                                            Override.overrides.Add(new TUXTexture(kvp.Key.Trim(), texture));

                                            TUXPlugin.Instance.ModLogger.LogDebug($"Found texture {texture.name}");
                                        }
                                        else
                                        {
                                            TUXPlugin.Instance.ModLogger.LogWarning($"Couldn't find texture with {tkey}");
                                        }
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    break;
            }
        }

        return Override;
    }

    public static TUXShaders.TUXShader ParseShader(string[] toParse)
    {
        TUXShaders.TUXShader tUXShader = null;
        ParseStage stage = ParseStage.None;
        for (int i = 0; i < toParse.Length; i++)
        {
            if (string.IsNullOrEmpty(toParse[i]))
                continue;
            string[] reading = toParse[i].Trim().Split('=');

            KeyValuePair<string, string> kvp = new(reading[0].Trim(), reading.Length <= 1 ? string.Empty : reading[1].Trim());

            switch (kvp.Key)
            {
                case "SHADER":
                    tUXShader = new TUXShaders.TUXShader(kvp.Value);
                    stage = ParseStage.SettingShader;
                    break;
                case "PROPERTIES":
                    stage = ParseStage.SettingProperties;
                    break;
                case "{":
                case "}":
                    break;
                default:
                    try
                    {
                        if (stage == ParseStage.SettingProperties)
                        {
                            string[] kvp2Source = kvp.Key.Split(new char[0]);
                            KeyValuePair<string, string> kvp2 = new(kvp2Source[0], kvp2Source[1]);
                            List<TUXProperty> properties = tUXShader.properties is null ? new() : tUXShader.properties.ToList();

                            switch (kvp2.Key.ToLower())
                            {
                                case "int":
                                    if (int.TryParse(kvp.Value, out int INT))
                                    {
                                        TUXInt tuxInt = new(kvp2.Value, INT);
                                        properties.Add(tuxInt);
                                    }
                                    else if (kvp.Value.Count(c => c == ':') == 2)
                                    {
                                        int defaultInt = -1, minInt = 0, maxInt = 1;
                                        for (int j = 0; j < 3; j++)
                                        {
                                            string s = kvp.Value.Split(':')[j];
                                            int.TryParse(s, out int INT2);
                                            if (j == 0)
                                            {
                                                defaultInt = INT2;
                                            }
                                            if (j == 1)
                                                minInt = INT2;
                                            if (j == 2)
                                                maxInt = INT2;
                                        }
                                        TUXInt tuxInt = new(kvp2.Value, defaultInt, minInt, maxInt);
                                        properties.Add(tuxInt);
                                    }

                                    break;
                                case "float":
                                    if (float.TryParse(kvp.Value, out float FLOAT))
                                    {
                                        TUXFloat tuxFloat = new(kvp2.Value, FLOAT);
                                        properties.Add(tuxFloat);
                                    }
                                    else if (kvp.Value.Count(c => c == ':') == 2)
                                    {
                                        float defaultFloat = -1, minFloat = 0, maxFloat = 1;
                                        for (int j = 0; j < 3; j++)
                                        {
                                            string s = kvp.Value.Split(':')[j];
                                            float.TryParse(s, out float FLOAT2);
                                            if (j == 0)
                                            {
                                                defaultFloat = FLOAT2;
                                            }
                                            if (j == 1)
                                                minFloat = FLOAT2;
                                            if (j == 2)
                                                maxFloat = FLOAT2;
                                        }
                                        TUXFloat tuxFloat = new(kvp2.Value, defaultFloat, minFloat, maxFloat);
                                        properties.Add(tuxFloat);
                                    }

                                    break;
                                case "bool":
                                    if (bool.TryParse(kvp.Value.ToLower(), out bool BOOL))
                                    {
                                        TUXBool tuxBool = new(kvp2.Value, BOOL);
                                        properties.Add(tuxBool);
                                    }
                                    break;
                                case "color":
                                    string colorParse = kvp.Value;
                                    string[] rgba = colorParse.Split(',');
                                    float r, g, b, a;

                                    r = Mathf.Clamp(float.Parse(rgba[0].Trim()), 0, 255);
                                    g = Mathf.Clamp(float.Parse(rgba[1].Trim()), 0, 255);
                                    b = Mathf.Clamp(float.Parse(rgba[2].Trim()), 0, 255);
                                    a = Mathf.Clamp(float.Parse(rgba[3].Trim()), 0, 255);

                                    r /= 255;
                                    g /= 255;
                                    b /= 255;
                                    a /= 255;

                                    TUXColor tuxColor = new TUXColor(kvp2.Value, new Color(r, g, b, a));
                                    properties.Add(tuxColor);
                                    break;
                                case "texture":
                                    TUXTexture tuxTexture = null;
                                    string textureWithExtension = kvp.Value.Trim() + ".png";
                                    if (kvp.Value.ToLower() == "null")
                                        tuxTexture = null;
                                    else if (kvp.Value.ToLower() == "white" || kvp.Value.ToLower() == "unitywhite")
                                    {
                                        tuxTexture = new TUXTexture(kvp2.Value);
                                    }
                                    else if (kvp.Value.ToLower() == "grey" || kvp.Value.ToLower() == "unitygrey")
                                    {
                                        tuxTexture = new TUXTexture(kvp2.Value, Texture2D.grayTexture);
                                    }
                                    else if (kvp.Value.ToLower() == "black" || kvp.Value.ToLower() == "unityblack")
                                    {
                                        tuxTexture = new TUXTexture(kvp2.Value, Texture2D.blackTexture);
                                    }
                                    else if (kvp.Value.ToLower() == "normal" || kvp.Value.ToLower() == "unitynormalmap")
                                    {
                                        tuxTexture = new TUXTexture(kvp2.Value, Texture2D.normalTexture, TUXTexture.TextureFormat.Bump);
                                    }
                                    else if (kvp.Value.ToLower() == "bump" || kvp.Value.ToLower() == "unitynormalmap")
                                    {
                                        tuxTexture = new TUXTexture(kvp2.Value, Texture2D.normalTexture, TUXTexture.TextureFormat.Bump);
                                    }
                                    else if (SpaceWarp.API.Assets.AssetManager.TryGetAsset(textureWithExtension, out Texture2D texture))
                                    {
                                        texture.name = kvp.Value;
                                        tuxTexture = new TUXTexture(kvp2.Value, texture);
                                    }
                                    else
                                    {
                                        GameManager.Instance.Game.Assets.Load<Texture2D>(textureWithExtension, (a) =>
                                        {
                                            if (a != default && a is not null)
                                            {
                                                TUXPlugin.Instance.ModLogger.LogDebug($"Found texture with {textureWithExtension}, not set yet!");
                                            }
                                            else
                                            {
                                                TUXPlugin.Instance.ModLogger.LogWarning($"Couldn't find texture with {textureWithExtension}");
                                            }
                                        });
                                        //TUXPlugin.Instance.ModLogger.LogWarning($"No texture was found at {kvp.Value}");
                                        continue;
                                    }

                                    properties.Add(tuxTexture);
                                    break;
                            };
                            tUXShader.properties = properties.ToArray();
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    break;
            }
        }

        return tUXShader;
    }

    public static string partNameToPrefab(string partName) => partName + ".prefab";
    public static Texture2D GetTextureFromPrefab(GameObject prefab, string TextureID)
    {
        Renderer renderer = prefab.GetComponent<Renderer>();
        renderer ??= prefab.GetComponentInChildren<Renderer>();

        return (Texture2D)renderer.material.GetTexture(TextureID);
    }

    public const string DIFFUSE_ID = "_MainTex";
    public const string METALLIC_ID = "_MetallicGlossMap";
    public const string BUMP_ID = "_BumpMap";
    public const string OCCLUSION_ID = "_OcclusionMap";
    public const string EMISSION_ID = "_EmissionMap";
    public const string PAINTMAP_ID = "_PaintMaskGlossMap";

    public static string[] Deserialize(TUXShaders.TUXShader toDeserialize)
    {
        List<string> result = new List<string>();

        result.Add($"SHADER = {toDeserialize.shaderPath}");
        if (toDeserialize.properties.Length > 0)
        {
            result.Add("PROPERTIES");
            foreach (TUXProperty property in toDeserialize.properties)
            {
                switch (property)
                {
                    case TUXInt INT:
                        result.Add($"int {INT.name} = {INT.defaultValue} : {INT.Min} : {INT.Max}");
                        break;
                    case TUXFloat FLOAT:
                        result.Add($"float {FLOAT.name} = {FLOAT.defaultValue} : {FLOAT.Min} : {FLOAT.Max}");
                        break;
                    case TUXBool BOOL:
                        result.Add($"bool {BOOL.name} = {BOOL.defaultValue}");
                        break;
                    case TUXColor COLOR:
                        result.Add($"Color {COLOR.name} = {COLOR.r}, {COLOR.g}, {COLOR.b}, {COLOR.a}");
                        break;
                    case TUXTexture TEXTURE:
                        result.Add($"Texture {TEXTURE.name} = {TEXTURE.TextureName}");
                        break;
                }
            }
        }


        return result.ToArray();
    }

    public static string[] DeserializeAsSettings(TUXShaders.TUXShader toDeserialize)
    {
        List<string> result = new List<string>();

        result.Add($"SHADER = {toDeserialize.shaderPath}");
        if (toDeserialize.properties.Length > 0)
        {
            result.Add("OVERRIDE = " + toDeserialize.target);
            foreach (TUXProperty property in toDeserialize.properties)
            {
                switch (property)
                {
                    case TUXInt INT:
                        result.Add($"{INT.name} = {INT.value}");
                        break;
                    case TUXFloat FLOAT:
                        result.Add($"{FLOAT.name} = {FLOAT.value}f");
                        break;
                    case TUXBool BOOL:
                        result.Add($"{BOOL.name} = {BOOL.value}");
                        break;
                    case TUXColor COLOR:
                        result.Add($"{COLOR.name} = {COLOR.r}, {COLOR.g}, {COLOR.b}, {COLOR.a}");
                        break;
                    case TUXTexture TEXTURE:
                        result.Add($"{TEXTURE.name} = {TEXTURE.TextureName}");
                        break;
                }
            }
        }


        return result.ToArray();
    }

    private enum ParseStage
    {
        None,
        SettingShader,
        SettingProperties,
        OverrideMaterial
    }
}
