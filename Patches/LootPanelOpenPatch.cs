using SPT.Reflection.Patching;
using EFT.Interactive;
using EFT.UI;
using EFT;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Comfort.Common;
using DrakiaXYZ.LootRadius.Helpers;
using EFT.InventoryLogic;

namespace DrakiaXYZ.LootRadius.Patches
{
    public class LootPanelOpenPatch : ModulePatch
    {
        private static FieldInfo _rightPaneField;
        private static LayerMask _interactiveLayerMask = 1 << LayerMask.NameToLayer("Interactive");

        private static StashItemClass _stash
        {
            get { return LootRadiusPlugin.RadiusStash; }
            set { LootRadiusPlugin.RadiusStash = value; }
        }

        protected override MethodBase GetTargetMethod()
        {
            // Find the variable that stores the right hand grid in the ItemUiContext, so we can Ctrl+Click
            _rightPaneField = AccessTools.GetDeclaredFields(typeof(ItemUiContext)).Single(x => x.FieldType == typeof(CompoundItem[]));

            return typeof(ItemsPanel).GetMethod(nameof(ItemsPanel.Show));
        }

        [PatchPostfix]
        public static async void PatchPostfix(
            ItemsPanel __instance,
            Task __result,
            ItemContextAbstractClass sourceContext,
            CompoundItem lootItem,
            InventoryController inventoryController,
            ItemsPanel.EItemsTab currentTab,
            SimpleStashPanel ____simpleStashPanel,
            AddViewListClass ___UI
        )
        {
            // Wait for original to finish
            await __result;

            // If lootItem isn't null, don't do anything, it means there's a right hand panel already
            if (lootItem != null)
            {
                return;
            }

            LootRadiusStashGrid grid = _stash.Grids[0] as LootRadiusStashGrid;
            Vector3 playerPosition = Singleton<GameWorld>.Instance.MainPlayer.Position;

            // First find any items directly near the player's feet, to allow them to loot things like items slightly under the floor
            Collider[] floorItemColliders = Physics.OverlapSphere(playerPosition, 0.35f, _interactiveLayerMask);
            AddAllowedItems(grid, floorItemColliders, true);

            // Then collect items around the player body, based on the loot radius
            playerPosition += (Vector3.up * 0.5f);
            Collider[] nearbyItemColliders = Physics.OverlapSphere(playerPosition, Settings.LootRadius.Value, _interactiveLayerMask);
            AddAllowedItems(grid, nearbyItemColliders, false);

            // Show the stash in the inventory panel
            ____simpleStashPanel.Show(_stash, inventoryController, sourceContext.CreateChild(_stash), true, inventoryController, currentTab);
            ___UI.AddDisposable<SimpleStashPanel>(____simpleStashPanel);

            _rightPaneField.SetValue(ItemUiContext.Instance, new CompoundItem[] { _stash });
        }

        private static void AddAllowedItems(LootRadiusStashGrid grid, Collider[] colliders, bool ignoreLineOfSight)
        {
            foreach (Collider collider in colliders)
            {
                var item = collider.gameObject.GetComponentInParent<LootItem>();
                if (item != null && item.Item.Parent.Container.ID != grid.ID && (ignoreLineOfSight || IsLineOfSight(item.transform.position)))
                {
                    item.ItemOwner.RemoveItemEvent += grid.OwnerRemoveItemEvent;

                    grid.AddInternal(item.Item, grid.FindFreeSpace(item.Item), false, true);
                }
            }
        }

        /**
         * Return true if the end position is within line of sight of the player
         */
        private static bool IsLineOfSight(Vector3 endPos)
        {
            // Start at the player's head
            Vector3 startPos = Singleton<GameWorld>.Instance.MainPlayer.MainParts[BodyPartType.head].Position;

            // LineCast returns true if it hits a HighPolyCollider, indicating the item isn't within line of sight of the player's head
            if (Physics.Linecast(startPos, endPos, LayerMaskClass.HighPolyWithTerrainMask))
            {
                return false;
            }

            return true;
        }
    }
}
