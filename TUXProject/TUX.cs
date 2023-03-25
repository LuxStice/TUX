using BepInEx;
using HarmonyLib;
using KSP.UI.Binding;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Game;
using SpaceWarp.API.Game.Extensions;
using SpaceWarp.API.UI;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;
using RTG;
using static iT;

namespace TUX;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class TUXPlugin : BaseSpaceWarpPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    public const string ModName = MyPluginInfo.PLUGIN_NAME;
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;
    
    private bool _isWindowOpen;
    private Rect _windowRect;

    private const string ToolbarFlightButtonID = "BTN-TUXFlight";
    private const string ToolbarOABButtonID = "BTN-TUXOAB";

    public static TUXPlugin Instance { get; set; }
    public static bool Autoupdate = true;

    static float mMetalicSmoothness = 1, mSmoothnessScale = 1, mMipBias = 0.8f, nDetailNormalScale = 1, nDetailNormalTiling = 1f, oOcclusionStrenght = 1,
        timeOfDayMin = -0.005f, timeOfDayMax = 0.005f, pmSmoothnessScale = 1, pmRimFalloff = 1f;
    static bool useTimeOfDay, pmSmoothnessOverride, useDetailMask, useDetailMap, disableNormalTexture;
    public static bool dirty;
    private static Shader shader;

    public static int[] propertyIds;

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;

        shader = Shader.Find("KSP2/Scenery/Standard (Opaque)");

        // Register Flight AppBar button
        Appbar.RegisterAppButton(
            "Textures Unlimited eXpanded",
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
            isOpen =>
            {
                _isWindowOpen = isOpen;
                GameObject.Find(ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(isOpen);
            }
        );

        // Register all Harmony patches in the project
        Harmony.CreateAndPatchAll(typeof(TUXPlugin).Assembly);

        propertyIds = new int[]
        {
            Shader.PropertyToID("_MainTex"),
            Shader.PropertyToID("_MetallicGlossMap"),
            Shader.PropertyToID("_BumpMap"),
            Shader.PropertyToID("_OcclusionMap"),
            Shader.PropertyToID("_EmissionMap"),
            Shader.PropertyToID("_PaintMaskGlossMap"),

            Shader.PropertyToID("_Metallic"),
            Shader.PropertyToID("_GlossMapScale"),
            Shader.PropertyToID("_MipBias"),

            Shader.PropertyToID("_DetailBumpMap"),
            Shader.PropertyToID("_DetailMask"),
            Shader.PropertyToID("_DetailBumpScale"),
            Shader.PropertyToID("_DetailBumpTiling"),

            Shader.PropertyToID("_OcclusionStrength"),

            Shader.PropertyToID("_UseTimeOfDay"),
            Shader.PropertyToID("_TimeOfDayDotMin"),
            Shader.PropertyToID("_TimeOfDayDotMax"),

            Shader.PropertyToID("_PaintGlossMapScale"),
            Shader.PropertyToID("_SmoothnessOverride"),
            Shader.PropertyToID("_RimFalloff")
        };
    }

    void Start()
    {
    }

    static bool doSetNormalMap = true;
    /// <summary>
    /// Draws a simple UI window when <code>this._isWindowOpen</code> is set to <code>true</code>.
    /// </summary>
    private void OnGUI()
    {
        // Set the UI
        GUI.skin = Skins.ConsoleSkin;

        if (_isWindowOpen)
        {
            _windowRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                _windowRect,
                FillWindow,
                "Textures Unlimited eXpanded",
                GUILayout.Height(600),
                GUILayout.Width(400)
            );
        }
    }

    private static void SetDefaults()
    {

        mMetalicSmoothness = 1;
        mSmoothnessScale = 1;
        mMipBias = 0.8f;
        nDetailNormalScale = 1;
        nDetailNormalTiling = 1f;
        oOcclusionStrenght = 1;
        timeOfDayMin = -0.005f;
        timeOfDayMax = 0.005f;
        pmSmoothnessScale = 1;
        pmRimFalloff = 1f;
        useTimeOfDay = false;
        pmSmoothnessOverride = false;
        useDetailMask = false;
        useDetailMap = false;
        disableNormalTexture = false;
    }

    public static string partName = "";
    public static Texture[] textures;

    /// <summary>
    /// Defines the content of the UI window drawn in the <code>OnGui</code> method.
    /// </summary>
    /// <param name="windowID"></param>
    private static void FillWindow(int windowID)
    {
        GUILayout.Label("modify textures in game");
        GUI.DragWindow(new Rect(0, 0, 10000, 40));

        partName = GUILayout.TextField(partName);
        
        if (ColorsPatch.partHash.ContainsKey(partName))
            DrawField();
        else
        {
            textures = null;
            doSetNormalMap = true;
            GUILayout.Label("Insert a valid partName above!");
        }
    }


    static bool enableConvert = false;

    public static void DrawField()
    {
        if (GUILayout.Button("Load textures"))
        {
            ReloadTextures();
            if (doSetNormalMap)
            {
                SetNormalMap((Texture2D)textures[ColorsPatch.BUMP]);
                doSetNormalMap = false;
            }
        }

        if (textures is null)
            return;

        GUILayout.Label("Mettalic/Smoothness");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Mettalic ({mMetalicSmoothness:0.00})");
        mMetalicSmoothness = GUILayout.HorizontalSlider(mMetalicSmoothness, 0f, 1f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Scale ({mSmoothnessScale:0.00})");
        mSmoothnessScale = GUILayout.HorizontalSlider(mSmoothnessScale, 0f, 1f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Mip Bias ({mMipBias:0.00})");
        mMipBias = GUILayout.HorizontalSlider(mMipBias, 0f, 1f);
        GUILayout.EndHorizontal();



        GUILayout.Label("Normal/Bump");
        enableConvert = GUILayout.Toggle(enableConvert, new GUIContent("Should Convert map?"), GUI.skin.toggle);
        disableNormalTexture = GUILayout.Toggle(disableNormalTexture, new GUIContent("Ignore Normal Map"), GUI.skin.toggle);
        useDetailMap = GUILayout.Toggle(useDetailMap, new GUIContent("Use Detail Map"), GUI.skin.toggle);
        useDetailMask = GUILayout.Toggle(useDetailMask, new GUIContent("Use Detail Mask"), GUI.skin.toggle);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Scale ({nDetailNormalScale:0.00})");
        nDetailNormalScale = GUILayout.HorizontalSlider(nDetailNormalScale, 0f, 1f);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Tiling ({nDetailNormalTiling:0.00})");
        nDetailNormalTiling = GUILayout.HorizontalSlider(nDetailNormalTiling, 0.01f, 10f);
        GUILayout.EndHorizontal();



        GUILayout.Label("Occlusion");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Strenght ({oOcclusionStrenght:0.00})");
        oOcclusionStrenght = GUILayout.HorizontalSlider(oOcclusionStrenght, 0f, 1f);
        GUILayout.EndHorizontal();



        GUILayout.Label("TimeOfDay");
        useTimeOfDay = GUILayout.Toggle(useTimeOfDay, new GUIContent("Use Time of Day"), GUI.skin.toggle);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Min ({timeOfDayMin:0.00})");
        timeOfDayMin = GUILayout.HorizontalSlider(timeOfDayMin, -1f, 1f);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Max ({timeOfDayMax:0.00})");
        timeOfDayMax = GUILayout.HorizontalSlider(timeOfDayMax, -1f, 1f);
        GUILayout.EndHorizontal();

        GUILayout.Label("Paint Map");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Smth Scale ({pmSmoothnessScale:0.00})");
        pmSmoothnessScale = GUILayout.HorizontalSlider(pmSmoothnessScale, 0f, 1f);
        GUILayout.EndHorizontal();
        pmSmoothnessOverride = GUILayout.Toggle(pmSmoothnessOverride, new GUIContent("Smth Ovrrd"), GUI.skin.toggle);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Rim Falloff ({pmRimFalloff:0.00})");
        pmRimFalloff = GUILayout.HorizontalSlider(pmRimFalloff, 0.01f, 5f);
        GUILayout.EndHorizontal();


        Autoupdate = GUILayout.Toggle(Autoupdate, new GUIContent("Auto-Update"), GUI.skin.toggle);
        if (GUILayout.Button("Update"))
        {
            TUXPlugin.dirty = true;
        }

        if (GUILayout.Button("Reset"))
            SetDefaults();
    }

    public static Texture normalMap;

    private static void SetNormalMap(Texture2D texture)
    {
        Texture2D convertedNormalMap = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false, true);

        Graphics.CopyTexture(texture, convertedNormalMap);

        Debug.Log($"Setting Normal Map" +
            $"\t{convertedNormalMap.activeTextureColorSpace}\t{convertedNormalMap.format}");

        normalMap = convertedNormalMap;
    }

    private static void ReloadTextures()
    {
        textures = ColorsPatch.GetTextures(partName);
        dirty = true;
    }

    public static Material GetMaterial()
    {
        if(textures is null || textures.Length == 0)
            return null;

        Material material = new(shader);

        for (int i = 0; i < 6; i++)
        {
            if(i == 2)
            {
                if (disableNormalTexture)
                material.SetTexture(propertyIds[i], Texture2D.normalTexture);
            else
                material.SetTexture(propertyIds[i], normalMap);
            }
            else
                material.SetTexture(propertyIds[i], textures[i]);
        }

        material.SetFloat(propertyIds[6], mMetalicSmoothness);
        material.SetFloat(propertyIds[7], mSmoothnessScale);
        material.SetFloat(propertyIds[8], mMipBias);


        if (useDetailMap)
            material.SetTexture(propertyIds[9], normalMap);
        else
            material.SetTexture(propertyIds[9], Texture2D.normalTexture);

        if (useDetailMask)
            material.SetTexture(propertyIds[10], normalMap);
        else
            material.SetTexture(propertyIds[10], Texture2D.whiteTexture);

        material.SetFloat(propertyIds[11], nDetailNormalScale);
        material.SetFloat(propertyIds[12], nDetailNormalTiling);

        material.SetFloat(propertyIds[13], oOcclusionStrenght);

        if (useTimeOfDay)
            material.SetFloat(propertyIds[14], Convert.ToSingle(useTimeOfDay));
        material.SetFloat(propertyIds[15], timeOfDayMin);
        material.SetFloat(propertyIds[16], timeOfDayMax);

        material.SetFloat(propertyIds[17], pmSmoothnessScale);
        if (pmSmoothnessOverride)
            material.SetFloat(propertyIds[18], Convert.ToSingle(pmSmoothnessOverride));
        material.SetFloat(propertyIds[19], pmRimFalloff);


        return material;
    }
}
