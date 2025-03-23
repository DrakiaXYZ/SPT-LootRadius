using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace DrakiaXYZ.LootRadius.Patches
{
    class LootRadiusQuickMovePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(InteractionsHandlerClass).GetMethod(nameof(InteractionsHandlerClass.QuickFindAppropriatePlace));
        }

        [PatchPrefix]
        public static void PatchPrefix(ref IEnumerable<CompoundItem> targets)
        {
            // If we're not in-raid, do nothing
            if (!Singleton<GameWorld>.Instantiated)
            {
                return;
            }

            // Don't do anything if this isn't a loot radius transfer, or if the default inventory already exists
            var defaultInventory = Singleton<GameWorld>.Instance.MainPlayer.Inventory.Equipment;
            if (!targets.Any(target => (target.Grids?.Length > 0 && target.Grids[0].ID == "lootRadiusGrid")) ||
                targets.Any(target => (target == defaultInventory)))
            {
                return;
            }

            // Add the default inventory to the list
            List<CompoundItem> newTargets = targets.ToList();
            newTargets.Add(defaultInventory);
            targets = newTargets;
        }
    }
}
