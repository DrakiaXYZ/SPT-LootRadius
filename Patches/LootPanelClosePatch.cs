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
        private static MethodInfo _addMethod;

        private static StashClass _stash
        {
            get { return LootRadiusPlugin.RadiusStash; }
            set { LootRadiusPlugin.RadiusStash = value; }
        }

        protected override MethodBase GetTargetMethod()
        {
            _addMethod = AccessTools.Method(typeof(ItemAddress), "Add");

            return typeof(ItemsPanel).GetMethod(nameof(ItemsPanel.Close));
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            if (_stash == null)
            {
                return;
            }

            var grid = _stash.Grids[0];

            // Store a copy of the items, so we can restore their state or throw them as loose loot
            var items = grid.Items.ToList();

            // Clear all the items
            grid.RemoveAll();

            // Restore items and throw as loose loot, as necessary
            foreach (var item in items)
            {
                // If the original address is null, or the item isn't in the loose loot pool, it's a discarded item
                if (item.OriginalAddress == null || Helpers.Utils.FindLootById(item.Id) == null)
                {
                    Singleton<GameWorld>.Instance.ThrowItem(item, Singleton<GameWorld>.Instance.MainPlayer, null);
                }
                else
                {
                    _addMethod.Invoke(item.OriginalAddress, new object[] { item, Array.Empty<string>(), false });
                }
            }
        }
    }
}
