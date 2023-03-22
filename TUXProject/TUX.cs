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

    static float mMetalicSmoothness, mSmoothnessScale, mMipBias, nDetailNormalScale, nDetailNormalTiling = 1f, oOcclusionStrenght,
        timeOfDayMin, timeOfDayMax, pmSmoothnessScale, pmRimFalloff = 1f;
    static bool useTimeOfDay, pmSmoothnessOverride;
    public static bool dirty;

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;

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

        // Register OAB AppBar Button
        Appbar.RegisterOABAppButton(
            "Textures Unlimited eXpanded",
            ToolbarOABButtonID,
            AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
            isOpen =>
            {
                _isWindowOpen = isOpen;
                GameObject.Find(ToolbarOABButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(isOpen);
            }
        );

        // Register all Harmony patches in the project
        Harmony.CreateAndPatchAll(typeof(TUXPlugin).Assembly);
    }

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
                GUILayout.Width(350)
            );
        }
        dirty = true;
    }

    public static string partName;
    public static Texture[] textures;

    /// <summary>
    /// Defines the content of the UI window drawn in the <code>OnGui</code> method.
    /// </summary>
    /// <param name="windowID"></param>
    private static void FillWindow(int windowID)
    {
        GUILayout.Label("Textures Unlimited eXpanded - modify textures in game");
        GUI.DragWindow(new Rect(0, 0, 10000, 20));

        partName = GUILayout.TextField(partName);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Mettalic/Smoothness ({mMetalicSmoothness:0.00})");
        mMetalicSmoothness =  GUILayout.HorizontalSlider(mMetalicSmoothness, 0f, 1f);
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
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Scale ({nDetailNormalScale:0.00})");
        nDetailNormalScale = GUILayout.HorizontalSlider(nDetailNormalScale, 0f, 1f);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Tiling ({nDetailNormalTiling:0.00})");
        nDetailNormalTiling = GUILayout.HorizontalSlider(nDetailNormalTiling, 0.01f, 1f);
        GUILayout.EndHorizontal();



        GUILayout.Label("Occlusion");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Strenght ({oOcclusionStrenght:0.00})");
        oOcclusionStrenght = GUILayout.HorizontalSlider(oOcclusionStrenght, 0f, 1f);
        GUILayout.EndHorizontal();



        GUILayout.Label("TimeOfDay?");
        //useTimeOfDay = GUILayout.DoToggle(useTimeOfDay);
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
        GUILayout.Label($"Smoothness Scale ({pmSmoothnessScale:0.00})");
        pmSmoothnessScale = GUILayout.HorizontalSlider(pmSmoothnessScale, 0f, 1f);
        GUILayout.EndHorizontal();
        //pmSmoothnessOverride = GUILayout.DoToggle(pmSmoothnessOverride);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Rim Falloff ({pmRimFalloff:0.00})");
        pmRimFalloff = GUILayout.HorizontalSlider(pmRimFalloff, 0.01f, 5f);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Update"))
            dirty = true;

        if (GUILayout.Button("Reload Textures"))
            ReloadTextures();
    }

    private static void ReloadTextures()
    {
        SpaceWarp.Patching.ColorsPatch.ReloadTextures();
        textures = SpaceWarp.Patching.ColorsPatch.GetTextures(partName);
        dirty = true;
    }

    public static void SetShaderSettings(ref Renderer renderer)
    {
        Shader shader = Shader.Find("KSP2/Scenery/Standard (Opaque)");

        int[] propertyIds = new int[]
        {
            Shader.PropertyToID("_MainTex"),
            Shader.PropertyToID("_MetallicGlossMap"),
            Shader.PropertyToID("_BumpMap"),
            Shader.PropertyToID("_OcclusionMap"),
            Shader.PropertyToID("_EmissionMap"),
            Shader.PropertyToID("_PaintMaskGlossMap")
        };

        Material material = new Material(shader);

        foreach(int i in propertyIds)
        {
            material.SetTexture(i, textures[i]);
        }

        material.SetFloat("_Metallic", mMetalicSmoothness);
        material.SetFloat("_GlossMapScale", mSmoothnessScale);
        material.SetFloat("_MipBias", mMipBias);

        material.SetFloat("_DetailBumpScale", nDetailNormalScale);
        material.SetFloat("_DetailBumpTiling", nDetailNormalTiling);

        material.SetFloat("_OcclusionStrength", oOcclusionStrenght);

        //material.SetFloat("_UseTimeOfDay", false);
        material.SetFloat("_TimeOfDayDotMin", timeOfDayMin);
        material.SetFloat("_TimeOfDayDotMax", timeOfDayMax);

        material.SetFloat("_PaintGlossMapScale", pmSmoothnessScale);
        //material.SetFloat("_SmoothnessOverride", pmSmoothnessOverride);
        material.SetFloat("_RimFalloff", pmRimFalloff);


        renderer.material = material;

        if (renderer.material.shader.name != shader.name)
            renderer.SetMaterial(material); //Sometimes the material Set doesn't work, this seems to be more reliable.
    }
}
