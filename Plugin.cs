using System.Collections.Generic;
using System.Linq;
using BepInEx;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

[assembly: AssemblyProduct("Nested Quick Move")]
[assembly: AssemblyTitle("Nested Quick Move")]
[assembly: AssemblyDescription("Quick move (CTRL+CLICK) searches nested containers for stacks and free slots")]
[assembly: AssemblyCopyright("SLPF")]
[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]

namespace NestedQuickMove;

[BepInPlugin("com.slpf.nestedquickmove", "NestedQuickMove", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        new QuickFindPlacePatch().Enable();
    }
}

public class QuickFindPlacePatch : ModulePatch
{
    private const string StashParentId = "566abbb64bdc2d144c8b457d";

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InteractionsHandlerClass), nameof(InteractionsHandlerClass.QuickFindAppropriatePlace));
    }

    [PatchPrefix]
    [HarmonyPriority(Priority.Low)]
    [HarmonyAfter("xyz.drakia.quickmovetocontainer")]
    public static void Prefix(Item item, ref IEnumerable<CompoundItem> targets)
    {
        if (item == null || targets == null) return;

        var targetList = targets.ToList();
        
        if (!targetList.Any()) return;

        var allContainers = new List<CompoundItem>(targetList);

        foreach (var topContainer in targetList)
        {
            if (topContainer.Template.ParentId == StashParentId) continue;
            
            foreach (var nested in topContainer.GetNotMergedItems())
            {
                if (nested is CompoundItem sub && sub != topContainer && !allContainers.Contains(sub))
                {
                    allContainers.Add(sub);
                }
            }
        }

        if (allContainers.Count > targetList.Count)
        {
            targets = allContainers;
        }
    }
}