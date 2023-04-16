using HarmonyLib;
using UnityEngine;
using KSP.Modules;
using RTG;
using KSP.Sim.impl;
using KSP.Sim.Definitions;
using KSP.Game;

namespace TUX;
[HarmonyPatch]
class Patch
{
    [HarmonyPatch(typeof(Module_Color), nameof(Module_Color.OnModuleUpdate))]
    public static void Prefix(Module_Color __instance)
    {
        if (TUXPlugin.textures is null)
            return;
        if (!__instance.part.Name.Contains(TUXPlugin.partName))
        { return; }

        if (TUXPlugin.Autoupdate || TUXPlugin.dirty)
        {
            __instance.SomeColorUpdated();
            TUXPlugin.dirty = false;
        }

    }

    public static List<GameObject> lookedIds = new();
    [HarmonyPatch(typeof(PartBehaviourModule), nameof(PartBehaviourModule.OnStart))]
    public static void Postfix(PartBehaviourModule __instance)
    {
        if (__instance.part is null && __instance.OABPart is null)
            return;
        foreach (GameObject go in __instance.gameObject.GetAllChildren())
        {

            if (lookedIds.Contains(go))
                continue;
            lookedIds.Add(go.gameObject);
            if (!go.TryGetComponent<Renderer>(out Renderer _))
                continue;

            foreach (var key in TUXPlugin.OverrideLookup.Keys)
            {
                if(go.name == key.partGO)
                {
                    TUXPlugin.Instance.ModLogger.LogInfo($"Applying override based on {TUXPlugin.OverrideLookup[key].baseShader.shaderPath} with {TUXPlugin.OverrideLookup[key].overrides.Count} changes to {go.name}");
                    go.GetComponent<Renderer>().sharedMaterial = TUXPlugin.OverrideLookup[key].GetMaterial();
                }
            }
        }
    }
}