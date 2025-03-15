using SPT.Reflection.Patching;
using EFT.InventoryLogic;
using EFT;
using System;
using System.Reflection;
using Comfort.Common;

namespace DrakiaXYZ.LootRadius.Patches
{
    public class GameStartedPatch : ModulePatch
    {
        private static StashItemClass _stash {
            get { return LootRadiusPlugin.RadiusStash; }
            set { LootRadiusPlugin.RadiusStash = value; }
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            // Setup the radius stash on raid start
            if (_stash == null)
            {
                _stash = Singleton<ItemFactoryClass>.Instance.CreateFakeStash();
                StashGridClass stashGridClass = new StashGridClass(_stash.Id, 10, 10, true, false, Array.Empty<ItemFilter>(), _stash);
                _stash.Grids = new StashGridClass[] { stashGridClass };
                var traderController = new TraderControllerClass(_stash, "RadiusStash", "Nearby Items", false, EOwnerType.Profile);
                Singleton<GameWorld>.Instance.ItemOwners.Add(traderController, default(GameWorld.GStruct126));

                // Destroy the loot item from the world when we take it
                traderController.RemoveItemEvent += (GEventArgs3 args) => {
                    // Only trigger on Success
                    if (args.Status != CommandStatus.Succeed)
                    {
                        return;
                    }

                    // Only destroy if it exists, to avoid throwing errors
                    if (Helpers.Utils.FindLootById(args.Item.Id) != null)
                    {
                        Singleton<GameWorld>.Instance.DestroyLoot(args.Item.Id);
                    }
                };
            }
        }
    }
}
