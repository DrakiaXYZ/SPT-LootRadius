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
        private static StashClass _stash {
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
                // Create our fake stash, note we use "fake" here to have the label show as "LOOT"
                _stash = Singleton<ItemFactory>.Instance.CreateFakeStash("fake");
                StashGridClass stashGridClass = new StashGridClass(_stash.Grid.Id, 10, 10, true, false, Array.Empty<ItemFilter>(), _stash);
                _stash.Grids = new StashGridClass[] { stashGridClass };
                var traderController = new TraderControllerClass(_stash, "RadiusStash", "Nearby Items", false, EOwnerType.Profile, null, null);
                Singleton<GameWorld>.Instance.ItemOwners.Add(traderController, default(GameWorld.GStruct118));

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
