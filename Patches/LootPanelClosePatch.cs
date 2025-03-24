using SPT.Reflection.Patching;
using EFT.UI;
using EFT;
using System;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using HarmonyLib;
using EFT.InventoryLogic;
using DrakiaXYZ.LootRadius.Helpers;
using UnityEngine;

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

            LootRadiusStashGrid grid = _stash.Grids[0] as LootRadiusStashGrid;
            foreach (var item in grid.ItemCollection.Keys.ToList())
            {
                // If the item is actually inside the radius grid, toss it
                if (item.CurrentAddress?.Container.ID == grid.ID)
                {
                    var player = Singleton<GameWorld>.Instance.MainPlayer;
                    item.CurrentAddress = player.InventoryController.CreateItemAddress();
                    player.InventoryController.ThrowItem(item, true);
                }
            }

            // Clear all the items from the loot radius grid
            grid.RemoveAll();
            grid.GridViews = null;
        }
    }
}
