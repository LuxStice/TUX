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
using SpaceWarp.Patching;
using static KSP.Api.UIDataPropertyStrings.View.Vessel.Stages;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using System.Runtime.CompilerServices;
using MoonSharp.VsCodeDebugger.SDK;
using KSP;
using KSP.Game.Flow;

namespace TUX;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public partial class TUXPlugin : BaseSpaceWarpPlugin
{
    // These are useful in case some other mod wants to add a dependency to this one
    public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    public const string ModName = MyPluginInfo.PLUGIN_NAME;
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    private bool _isWindowOpen;
    private Rect _windowRect;

    private const string ToolbarFlightButtonID = "BTN-TUXFlight";

    public static TUXPlugin Instance { get; set; }
    public static Dictionary<(string partName, string partGO), TUXOverride> OverrideLookup = new();

    public static bool Autoupdate = true;

    public static bool dirty;
    public static Material cachedMaterial;
    private static TUXShaders.TUXShader currentShader;
    public static PartUnderMouseChanged partUnderMouseChanged;
    private static GameObject SelectedObject;
    private static GameObject OriginalParent;
    internal static Dictionary<string, Texture2D> allTextures = new();
    private const string ConfigPath = "/configs/";
    private static string fullConfigPath = "";
    private List<string> configs = new();
    private List<string> overrides = new();

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();
        GameManager.Instance.LoadingFlow.AddAction(new LoadTUXShadersFlowAction("Load TUX Shaders"));

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
        GameManager.Instance.Game.Messages.Subscribe<PartUnderMouseChanged>(PartChangedUnderMouse);
        GameManager.Instance.Game.Messages.Subscribe<GameStateChangedMessage>(GameStateChanged);
        GameManager.Instance.Game.Messages.Subscribe<VesselRecoveredMessage>(VesselRecovered);
        GameManager.Instance.Game.Messages.Subscribe<PartBehaviourInitializedMessage>(PartBehaviourInitialized);

        // Register all Harmony patches in the project
        Harmony.CreateAndPatchAll(typeof(TUXPlugin).Assembly);
    }
    private List<PartBehavior> parts = new();
    private void PartBehaviourInitialized(MessageCenterMessage obj)
    {
        PartBehaviourInitializedMessage msg = (PartBehaviourInitializedMessage)obj;
        if (parts.Contains(msg.Part))
            return;
        parts.Add(msg.Part);

        foreach(Renderer renderer in msg.Part.Renderers)
        {
            string sanitizedName = renderer.name.Replace("(Clone)", string.Empty).Trim();
            foreach(var key in OverrideLookup.Keys)
            {
                if(key.partName == msg.Part.Name && key.partGO == sanitizedName)
                {
                    TUXPlugin.Instance.ModLogger.LogInfo($"Applying override based on {TUXPlugin.OverrideLookup[key].baseShader.shaderPath} with {TUXPlugin.OverrideLookup[key].overrides.Count} changes to {sanitizedName}");
                    renderer.sharedMaterial = OverrideLookup[key].GetMaterial();
                }
            }
        }

    }

    static bool shadersLoaded;
    public static void LoadShaders()
    {
        if (shadersLoaded)
            return;
        shadersLoaded = true;
        fullConfigPath = Path.Combine(TUXPlugin.Instance.PluginFolderPath + ConfigPath);

        TUXPlugin.OverrideLookup.Clear();
        TUXPlugin.Instance.configs.Clear();
        TUXPlugin.Instance.overrides.Clear();

        foreach (var shader in TUXShaders.defaultShaders)
        {
            string path = Path.Combine(fullConfigPath, shader.shaderPath.Replace('/', '_').Replace(@"\".ToCharArray()[0], '_') + ".cfg");
            if (File.Exists(path))
            {
                TUXPlugin.Instance.configs.Add(path);
                continue;
            }

            TUXPlugin.Instance.ModLogger.LogInfo($"Creating TUXShader {shader.shaderPath} in {path}.");

            Directory.CreateDirectory(fullConfigPath);
            var fs = File.Create(path);
            fs.Dispose();
            fs.Close();
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.NewLine = "\n";
                foreach (string s in IOHelper.Deserialize(shader))
                {
                    sw.WriteLine(s);
                }
            }
        }

        TUXShaders.Shaders.AddRange(TUXShaders.defaultShaders);


        foreach (var filePath in Directory.GetFiles(fullConfigPath, "*.cfg", SearchOption.TopDirectoryOnly))
        {
            if (!File.Exists(filePath))
                continue;
            if (TUXPlugin.Instance.configs.Contains(filePath))
                continue;
            string[] toParse = File.ReadLines(filePath).ToArray();

            TUXShaders.TUXShader shader = IOHelper.ParseShader(toParse);
            if (shader.shader is not null)
            {
                if (!TUXShaders.Shaders.Any(a => a.shaderPath == shader.shaderPath))
                {
                    TUXShaders.Shaders.Add(shader);
                    TUXPlugin.Instance.configs.Add(filePath);
                }
                else
                {
                    TUXPlugin.Instance.ModLogger.LogError($"Shader {shader.shaderPath} already exists.");
                }
            }
            else
            {
                TUXPlugin.Instance.ModLogger.LogError($"Shader {shader.shaderPath} couldn't be found.");
            }
        }
        foreach (var filePath in Directory.GetFiles(Path.Combine(fullConfigPath, "overrides"), "*.cfg", SearchOption.AllDirectories))
        {
            if (!File.Exists(filePath))
                continue;
            if (TUXPlugin.Instance.overrides.Contains(filePath))
                continue;
            string[] toParse = File.ReadLines(filePath).ToArray();
                TUXOverride tuxOverride = IOHelper.ParseOverride(toParse);
            if (tuxOverride.baseShader is not null)
            {
                if (tuxOverride.target != default)
                {
                    TUXPlugin.Instance.overrides.Add(filePath);
                    OverrideLookup.Add(tuxOverride.target, tuxOverride);
                }
                else
                {
                    TUXPlugin.Instance.ModLogger.LogError($"Could not parse override's target.");
                }
            }
            else
            {
                TUXPlugin.Instance.ModLogger.LogError($"Shader {tuxOverride.baseShader.shaderPath} couldn't be found.");
            }
        }
    }

    public static Dictionary<string, GameObject> serializedPrefabs = new();
    public static void LoadPrefabCallback(string partName, GameObject prefab)
    {
        if (serializedPrefabs.ContainsKey(partName))
            return;
        serializedPrefabs.Add(partName, prefab);
    }

    public override void OnPostInitialized()
    {
        base.OnPostInitialized();

    }

    public class LoadTUXShadersFlowAction : FlowAction
    {
        public LoadTUXShadersFlowAction(string name) : base(name)
        {
        }

        public override void DoAction(Action resolve, Action<string> reject)
        {

            try
            {
                TUXPlugin.LoadShaders();
                resolve();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                reject(null);
            }
        }
    }

    public static GUIStyle Selected;
    public static GUIStyle Unavailable;
    public static GUIStyle Available;

    private void VesselRecovered(MessageCenterMessage obj)
    {
        SelectedObject = null;
        OriginalParent = null;
        currentShader = null;
        cachedMaterial = null;
    }

    void OnDestroy()
    {

        GameManager.Instance.Game.Messages.Unsubscribe<PartUnderMouseChanged>(PartChangedUnderMouse);
        GameManager.Instance.Game.Messages.Unsubscribe<GameStateChangedMessage>(GameStateChanged);
        GameManager.Instance.Game.Messages.Unsubscribe<VesselRecoveredMessage>(VesselRecovered);
    }

    private void GameStateChanged(MessageCenterMessage obj)
    {
        GameStateChangedMessage message = obj as GameStateChangedMessage;
        if (message.CurrentState != GameState.FlightView) { _isWindowOpen = false; }
        if(message.CurrentState == GameState.VehicleAssemblyBuilder || message.CurrentState == GameState.FlightView) { Patch.lookedIds.Clear(); }
    }

    void Start()
    {
    }

    private void PartChangedUnderMouse(MessageCenterMessage message)
    {
        partUnderMouseChanged = message as PartUnderMouseChanged;
    }

    public void UpdateTextures()
    {
        foreach (var kayValuePair in AssetManager.AllAssets)
        {
            string name = kayValuePair.Key;
            var asset = kayValuePair.Value;

            if (name.Contains("icon"))
                continue;
            if (name.Contains("spacewarp"))
                continue;

            if ((asset as Texture2D) is not null)
            {
                Texture2D texture = (Texture2D)asset;
                allTextures.Add(name, texture);
                //Debug.Log($"Found {name}.");
            }
        }
        if (allTextures.Count > 0)
        {
            //Debug.Log($"Found {allTextures.Count} textures.");
        }
    }

    void Update()
    {
        if (!_isWindowOpen)
            return;
        if (GameManager.Instance is not null && GameManager.Instance.Game is not null)
        {
            GameStateConfiguration gameStateConfiguration = GameManager.Instance.Game.GlobalGameState.GetGameState();
            if (gameStateConfiguration.IsFlightMode)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        SimulationObjectView nullobj;
                        if (hit.rigidbody is null)
                            return;
                        if (hit.rigidbody.gameObject.TryGetComponent<SimulationObjectView>(out nullobj))
                        {
                            if (nullobj != null)
                            {
                                if (partUnderMouseChanged.newPartUnderMouse.Rigidbody.gameObject is not null)
                                {
                                    var go = partUnderMouseChanged.newPartUnderMouse.Rigidbody.gameObject;
                                    if (go != SelectedObject)
                                    {
                                        if (SelectedObject is not null)
                                            SetAndForget();
                                        SelectedObject = go;
                                        OriginalParent = null;
                                        OnSelectedChanged();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void SetAndForget()
    {
        UpdateSelectedMaterial(true);
    }

    private void OnSelectedChanged()
    {
        Material m;
        if (OriginalParent is not null)
            m = SelectedObject.GetComponent<Renderer>().sharedMaterial;
        else if (SelectedObject.TryGetComponent<Renderer>(out Renderer rootRenderer))
        {
            m = rootRenderer.sharedMaterial;
        }
        else
        {
            m = SelectedObject.GetComponentInChildren<Renderer>().sharedMaterial;
        }

        Shader s = m.shader;

        if (TUXShaders.TryGetTUXEquivalent(s, out TUXShaders.TUXShader tuxEquivalent))
        {
            currentShader = tuxEquivalent;
            currentShader.CopyFrom(m);
            cachedMaterial = currentShader.GetMaterial();

            shaderDirty = dirty = true;
        }
        else
        {
            currentShader = null;
            cachedMaterial = null;
            OriginalParent = null;
        }
    }

    public static void UpdateSelectedMaterial(bool setAndForget = false)
    {
        cachedMaterial = currentShader.GetMaterial();
        Material newInstance = cachedMaterial;
        if (setAndForget)
            newInstance = new(cachedMaterial);

        if (SelectedObject.TryGetComponent<Renderer>(out Renderer rootRenderer))
        {
            rootRenderer.sharedMaterial = newInstance;
            return;
        }
        else
        {
            Renderer renderer = SelectedObject.GetComponentInChildren<Renderer>();
            renderer.sharedMaterial = newInstance;
            return;
        }
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
                GUIUtility.GetControlID(FocusType.Keyboard),
                _windowRect,
                FillWindow,
                "Texture Utilities eXpanded",
                GUILayout.Height(800),
                GUILayout.Width(530)
            );
        }

        if (currentShader is not null)
        {
            foreach (TUXProperty property in currentShader.properties)
            {
                property.OnGUI();
            }
        }
    }

    public static string partName = "";
    public static Texture[] textures;
    public static string shaderSearch;
    public static bool selectRendererDropdown = false;
    public static Vector2 selectShaderScrollRect;

    /// <summary>
    /// Defines the content of the UI window drawn in the <code>OnGui</code> method.
    /// </summary>
    /// <param name="windowID"></param>
    private static void FillWindow(int windowID)
    {
        GUILayout.Label("modify textures in game");
        if (GUI.Button(new Rect(TUXPlugin.Instance._windowRect.width - 18, 2, 16, 16), "x"))
        {
            TUXPlugin.Instance._isWindowOpen = false;
            TUXPlugin.Instance.SetAndForget();
            currentShader = null;
            cachedMaterial = null;
            SelectedObject = null;
            OriginalParent = null;
            GUIUtility.ExitGUI();
        }
        GUI.DragWindow(new Rect(0, 0, 10000, 40));

        if (SelectedObject is not null && currentShader is not null)
        {
            currentShader.ApplyAll(cachedMaterial);
        }

        string objectName;
        string shaderName;
        if (SelectedObject is null)
        {
            GUILayout.Label("Please select a valid part!");
            return;
        }
        Material material;
        if (SelectedObject.TryGetComponent<Renderer>(out Renderer rootRenderer))
        {

            objectName = rootRenderer.gameObject.name;
            shaderName = rootRenderer.sharedMaterial.shader.name;
            material = rootRenderer.sharedMaterial;
        }
        else
        {
            objectName = SelectedObject.GetComponentInChildren<Renderer>().gameObject.name;
            shaderName = SelectedObject.GetComponentInChildren<Renderer>().sharedMaterial.shader.name;
            material = SelectedObject.GetComponentInChildren<Renderer>().sharedMaterial;
        }

        Selected = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        Unavailable = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic };
        Available = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter };

        if (!TUXShaders.Contains(SelectedObject.GetComponentInChildren<Renderer>().sharedMaterial))
        {
            GUILayout.Label($"TUX doesn't contain a definition for {shaderName}");
            return;
        }

        if (SelectedObject is not null && OriginalParent is not null)
        {
            string originalObjectName = OriginalParent.GetComponentInChildren<Renderer>().gameObject.name;
            Material originalMaterial = OriginalParent.GetComponentInChildren<Renderer>().material;

            selectRendererDropdown = GUIHelpers.Dropdown($"{objectName}", selectRendererDropdown, true);
            if (selectRendererDropdown)
            {
                selectShaderScrollRect = GUILayout.BeginScrollView(selectShaderScrollRect, GUI.skin.verticalScrollbar,
                    GUILayout.Width(525), GUILayout.Height(200));

                if (GUILayout.Button($"Back ({originalObjectName})", Available))
                {
                    TUXPlugin.Instance.SetAndForget();
                    SelectedObject = OriginalParent;
                    OriginalParent = null;
                    TUXPlugin.Instance.OnSelectedChanged();
                    UpdateSelectedMaterial();
                    return;
                }

                foreach (Renderer renderer in OriginalParent.gameObject.GetComponentsInChildren<Renderer>(true))
                {
                    string matName = renderer.gameObject.name;
                    string shaderName2 = renderer.sharedMaterial.shader.name;
                    if (renderer.material == originalMaterial)
                        continue;
                    if (renderer.gameObject == SelectedObject)
                    {
                        GUILayout.Label($"{matName}({shaderName2})", Selected);
                        continue;
                    }
                    if (!TUXShaders.Contains(renderer.material))
                    {
                        GUILayout.Label($"{matName}({shaderName2})", Unavailable);
                        continue;
                    }
                    if (GUILayout.Button($"{matName}({shaderName2})", Available))
                    {
                        TUXPlugin.Instance.SetAndForget();
                        SelectedObject = renderer.gameObject;
                        TUXPlugin.Instance.OnSelectedChanged();
                        UpdateSelectedMaterial();
                        continue;
                    }
                }

                GUILayout.EndScrollView();
            }

            DrawShaderFields(currentShader);
            if (GUILayout.Button("Save Override"))
            {
                TUXPlugin.Instance.CreateSettings();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set and Forget"))
            {
                TUXPlugin.Instance.SetAndForget();
            }
            if (GUILayout.Button("Clear"))
            {
                currentShader = null;
                SelectedObject = null;
                OriginalParent = null;
            }
            GUILayout.EndHorizontal();

            return;
        }
        if (SelectedObject is not null && OriginalParent is null)
        {
            selectRendererDropdown = GUIHelpers.Dropdown($"{objectName}", selectRendererDropdown, true);
            if (selectRendererDropdown)
            {
                selectShaderScrollRect = GUILayout.BeginScrollView(selectShaderScrollRect, GUI.skin.verticalScrollbar,
                    GUILayout.Width(525), GUILayout.Height(200));

                foreach (Renderer renderer in SelectedObject.gameObject.GetComponentsInChildren<Renderer>(true))
                {
                    string matName = renderer.gameObject.name;
                    string shaderName2 = renderer.sharedMaterial.shader.name;
                    if (renderer.material == material)
                        continue;
                    if (!TUXShaders.Contains(renderer.material))
                    {
                        GUILayout.Label($"{matName}({shaderName2})", Unavailable);
                        continue;
                    }
                    if (GUILayout.Button($"{matName}({shaderName2})", Available))
                    {
                        TUXPlugin.Instance.SetAndForget();
                        OriginalParent = SelectedObject;
                        SelectedObject = renderer.gameObject;
                        TUXPlugin.Instance.OnSelectedChanged();
                        UpdateSelectedMaterial();
                        continue;
                    }
                }

                GUILayout.EndScrollView();
            }

            DrawShaderFields(currentShader);
            if (GUILayout.Button("Save Override"))
            {
                TUXPlugin.Instance.CreateSettings();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set and Forget"))
            {
                TUXPlugin.Instance.SetAndForget();
            }
            if (GUILayout.Button("Clear"))
            {
                currentShader = null;
                SelectedObject = null;
                OriginalParent = null;
            }
            GUILayout.EndHorizontal();

            return;
        }
    }

    public void CreateSettings()
    {
        string partName;
        string goName;
        partName = SelectedObject.GetComponentInParent<CorePartData>().Data.partName;
        if (SelectedObject.TryGetComponent<Renderer>(out Renderer rootRenderer))
        {
            goName = rootRenderer.gameObject.name;
        }
        else
        {
            goName = SelectedObject.GetComponentInChildren<Renderer>().gameObject.name;
        }

        currentShader.target = new(partName, goName);
        string path = Path.Combine(fullConfigPath, "overrides", ($"{partName} - {goName}").Replace('/', '_').Replace(@"\".ToCharArray()[0], '_') + "_OVERRIDE.cfg");

        Directory.CreateDirectory(Path.Combine(fullConfigPath, "overrides"));
        if (!File.Exists(path))
        {
            var fs = File.Create(path);
            fs.Dispose();
            fs.Close();
        }
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.NewLine = "\n";
            foreach (string s in IOHelper.DeserializeAsSettings(currentShader))
            {
                sw.WriteLine(s);
            }
        }
    }

    static Vector2 scrollPosition;
    internal static void DrawShaderFields(TUXShaders.TUXShader tuxShader)
    {
        GUILayout.Label($"Shader: {tuxShader.shaderPath}");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUI.skin.verticalScrollbar,
            GUILayout.Width(525), GUILayout.Height(700));
        foreach (TUXProperty property in tuxShader.properties)
        {
            if (property.Draw())
            {
                shaderDirty = true;
                property.Apply(ref cachedMaterial);
            }
        }
        GUILayout.EndScrollView();

        if (shaderDirty)
        {
            shaderDirty = false;
            UpdateSelectedMaterial();
            dirty = true;
        }
    }
    private static bool shaderDirty;
}
