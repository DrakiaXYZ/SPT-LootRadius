using Aki.Reflection.Patching;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace DrakiaXYZ.LootRadius.Patches
{
    internal class QuestItemDragPatch : ModulePatch
    {
        private static FieldInfo _itemOwnerField;
        protected override MethodBase GetTargetMethod()
        {
            _itemOwnerField = AccessTools.GetDeclaredFields(typeof(GridView)).Single(x => x.FieldType == typeof(IItemOwner));

            return typeof(GridView).GetMethod(nameof(GridView.CanDrag));
        }

        [PatchPrefix]
        public static bool PatchPrefix(GridView __instance, ref bool __result, ItemContextAbstractClass itemContext)
        {
            // If not the RadiusStash GridView, run original
            IItemOwner gridOwner = _itemOwnerField.GetValue(__instance) as IItemOwner;
            if (gridOwner.ID != "RadiusStash")
            {
                return true;
            }

            // If this is a quest item, return false
            if (itemContext.Item.QuestItem)
            {
                __result = false;
                return false;
            }

            // Otherwise allow original function to run
            return true;
        }
    }
}
