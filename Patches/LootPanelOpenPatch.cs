using Aki.Reflection.Patching;
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
        private static FieldInfo _stashViewField;
        private static FieldInfo _rightPaneField;
        private static MethodInfo _addMethod;
        private static MethodInfo _removeMethod;
        private static LayerMask _interactiveLayerMask = 1 << LayerMask.NameToLayer("Interactive");

        private static StashClass _stash
        {
            get { return LootRadiusPlugin.RadiusStash; }
            set { LootRadiusPlugin.RadiusStash = value; }
        }

        protected override MethodBase GetTargetMethod()
        {
            _addMethod = AccessTools.Method(typeof(StashGridClass), "Add", new Type[] { typeof(Item) });
            _removeMethod = AccessTools.Method(typeof(ItemAddress), "Remove");

            // Find the stash interface variable, based on the implemented types of the SimpleStashPanel
            Type stashInterfaceType = null;
            Type[] stashInterfaceTypes = typeof(SimpleStashPanel).GetInterfaces();
            foreach (Type type in stashInterfaceTypes)
            {
                if (type.Name.StartsWith("GInterface"))
                {
                    stashInterfaceType = type;
                    break;
                }
            }
            _stashViewField = AccessTools.GetDeclaredFields(typeof(ItemsPanel)).Single(x => x.FieldType == stashInterfaceType);

            // Find the variable that stores the right hand grid in the ItemUiContext, so we can Ctrl+Click
            _rightPaneField = AccessTools.GetDeclaredFields(typeof(ItemUiContext)).Single(x => x.FieldType == typeof(LootItemClass[]));

            return typeof(ItemsPanel).GetMethod(nameof(ItemsPanel.Show));
        }

        [PatchPostfix]
        public static async void PatchPostfix(
            ItemsPanel __instance,
            Task __result,
            ItemContextAbstractClass sourceContext,
            LootItemClass lootItem,
            InventoryControllerClass inventoryController,
            ItemsPanel.EItemsTab currentTab,
            SimpleStashPanel ____simpleStashPanel
        )
        {
            // Wait for original to finish
            await __result;

            // If lootItem isn't null, don't do anything, it means there's a right hand panel already
            if (lootItem != null)
            {
                return;
            }

            // Collect the items around the player, and add them to the fake stash
            var grid = _stash.Grids[0];
            Vector3 playerPosition = Singleton<GameWorld>.Instance.MainPlayer.Position;
            Collider[] colliders = Physics.OverlapSphere(playerPosition, Settings.LootRadius.Value, _interactiveLayerMask);
            if (colliders.Length > 0)
            {
                foreach (Collider collider in colliders)
                {
                    var item = collider.gameObject.GetComponentInParent<LootItem>();
                    if (item != null && item.Item.Parent.Container != grid)
                    {
                        item.Item.OriginalAddress = item.Item.CurrentAddress;
                        _removeMethod.Invoke(item.Item.CurrentAddress, new object[] { item.Item, string.Empty, false });
                        _addMethod.Invoke(grid, new object[] { item.Item });
                    }
                }
            }

            // Show the stash in the inventory panel
            ____simpleStashPanel.Configure(_stash, inventoryController, sourceContext.CreateChild(_stash));
            _stashViewField.SetValue(__instance, ____simpleStashPanel);
            ____simpleStashPanel.Show(inventoryController, currentTab);

            _rightPaneField.SetValue(ItemUiContext.Instance, new LootItemClass[] { _stash });
        }
    }
}
