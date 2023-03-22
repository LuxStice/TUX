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
        if(__instance.part.name == TUXPlugin.partName)
        {
            if (TUXPlugin.dirty)
        {
                Renderer[] renderers = __instance.gameObject.GetComponentsInChildren<Renderer>(true);

                for (int i = 0; i < renderers.Length; i++)
                {
                    TUXPlugin.SetShaderSettings(ref renderers[i]);
                }
            }
            __instance.SomeColorUpdated();
        }

        TUXPlugin.dirty = false;
    }
}