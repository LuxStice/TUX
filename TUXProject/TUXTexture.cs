using UnityEngine;

namespace TUX;

internal class TUXTexture : TUXProperty<Texture2D>
{
    public enum TextureFormat { Color, Bump };
    public TextureFormat Format { get => _format; set => OnTextureFormatChange(value); }
    private TextureFormat _format = TextureFormat.Color;

    public string originPartName, originMesh;

    public string TextureName
    {
        get
        {
            string s = string.Empty;
            if (value is null)
                return "null";
            if (value is Texture2D)
                return value.name;
            return s;
        }
    }

    //GUI
    private bool isDroppedDown = false;
    private bool isTextureSelectionWindowOpen;
    private Rect textureSelectionRect;
    private bool changed;
    private Vector2 selectTextureScrollRect;

    public TUXTexture(string name) : base(name, Texture2D.whiteTexture)
    {

    }
    public TUXTexture(string name, TextureFormat Format) : base(name, Format == TextureFormat.Color ? Texture2D.whiteTexture : Texture2D.normalTexture)
    {

    }
    public TUXTexture(string name, Texture2D defaultValue) : base(name, defaultValue)
    {

    }
    public TUXTexture(string name, Texture2D defaultValue, TextureFormat Format) : base(name, defaultValue)
    {
        this.Format = Format;
    }

    public void SetValue(Texture2D value, bool format = true)
    {
        this.value = value;
        if (format)
            FormatTexture();
    }

    private void OnTextureFormatChange(TextureFormat format)
    {
        _format = format;
        FormatTexture();
    }

    private void FormatTexture()
    {
        Texture2D convertedTexture = new Texture2D(value.width, value.height, value.format, false, _format == TextureFormat.Bump);

        Graphics.CopyTexture(value, convertedTexture);

        value = convertedTexture;
    }

    public override bool Draw()
    {
        if (value is null)
        {
            GUILayout.Label($"{name}: null");
            if (GUILayout.Button("Set"))
            {
                isTextureSelectionWindowOpen = !isTextureSelectionWindowOpen;
                TUXPlugin.Instance.UpdateTextures();
            }
            return changed;
        }
        GUILayout.Label($"{name}: {(string.IsNullOrEmpty(value.name) ? "unnamed" : value.name)}");
        if (GUILayout.Button("Change"))
        {
            isTextureSelectionWindowOpen = !isTextureSelectionWindowOpen;
            TUXPlugin.Instance.UpdateTextures();
        }
        if (GUILayout.Button(isDroppedDown ? "▼" : "▲"))
        {
            isDroppedDown = !isDroppedDown;
        }

        if (isDroppedDown)
            GUILayout.Box(value, GUILayout.Width(512), GUILayout.Height(512));
        return changed;
    }

    private void FillTextureSelectionWindow(int id)
    {
        if (GUI.Button(new Rect(textureSelectionRect.width - 18, 2, 16, 16), "x"))
        {
            isTextureSelectionWindowOpen = false;
        }
        GUI.DragWindow(new Rect(0, 0, 10000, 40));

        selectTextureScrollRect = GUILayout.BeginScrollView(selectTextureScrollRect, GUI.skin.verticalScrollbar,
            GUILayout.Width(270), GUILayout.Height(500));
        foreach (var KeyValuePair in TUXPlugin.allTextures)
        {
            if (GUILayout.Button(KeyValuePair.Key))
            {
                KeyValuePair.Value.name = KeyValuePair.Key;
                SetValue(KeyValuePair.Value);
                changed = true;
                isTextureSelectionWindowOpen = false;
            }
            GUILayout.Box(KeyValuePair.Value, GUILayout.Width(256), GUILayout.Height(256));
        }
        GUILayout.EndScrollView();
    }

    public override void Apply(ref Material material)
    {
        material.SetTexture(name, value);
        changed = false;
    }
    public override Texture2D Read(Material material)
    {
        return (Texture2D)material.GetTexture(name);
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (isTextureSelectionWindowOpen)
        {
            GUILayout.Window(
        GUIUtility.GetControlID(FocusType.Keyboard),
        textureSelectionRect,
        FillTextureSelectionWindow,
        $"Select texture for {name}",
        GUILayout.Height(500),
        GUILayout.Width(270));
        }
    }

    public override string ToString()
    {
        return $"{name}: {TextureName} ({GetType().Name})";
    }
}
