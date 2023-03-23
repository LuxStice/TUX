using HarmonyLib;
using UnityEngine;
using KSP.Modules;
using RTG;

namespace TUX;

[HarmonyPatch(typeof(Module_Color), nameof(Module_Color.OnModuleUpdate))]
class Patch
{
    public static void Prefix(Module_Color __instance)
    {
        if (TUXPlugin.textures is null)
            return;
        if(__instance.part.Name != TUXPlugin.partName)
        { return; }

        if (TUXPlugin.Autoupdate || TUXPlugin.dirty)
        {
            Renderer[] renderers = __instance.gameObject.GetComponentsInChildren<Renderer>(true);
            Material toSet = TUXPlugin.GetMaterial();

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = toSet;
            }

            __instance.SomeColorUpdated();
            TUXPlugin.dirty = false;
        }

    }
}