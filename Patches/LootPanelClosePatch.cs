using SPT.Reflection.Patching;
using EFT.UI;
using EFT;
using System;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using HarmonyLib;
using EFT.InventoryLogic;

namespace DrakiaXYZ.LootRadius.Patches
{
    public class LootPanelClosePatch : ModulePatch
    {
        private static StashItemClass _stash
        {
            get { return LootRadiusPlugin.RadiusStash; }
            set { LootRadiusPlugin.RadiusStash = value; }
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(ItemsPanel).GetMethod(nameof(ItemsPanel.Close));
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            if (_stash == null)
            {
                return;
            }

            // Clear all the items from the loot radius grid
            _stash.Grids[0].RemoveAll();
        }
    }
}
