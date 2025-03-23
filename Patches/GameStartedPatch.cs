using SPT.Reflection.Patching;
using EFT.InventoryLogic;
using EFT;
using System;
using System.Reflection;
using Comfort.Common;
using DrakiaXYZ.LootRadius.Helpers;

namespace DrakiaXYZ.LootRadius.Patches
{
    public class GameStartedPatch : ModulePatch
    {
        private static StashItemClass _stash
        {
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

                var stashGridClass = new LootRadiusStashGrid("lootRadiusGrid", _stash);
                _stash.Grids = new StashGridClass[] { stashGridClass };

                var traderController = new TraderControllerClass(_stash, "RadiusStash", "Nearby Items", false, EOwnerType.Profile);
                Singleton<GameWorld>.Instance.ItemOwners.Add(traderController, default(GameWorld.GStruct126));

                traderController.AddItemEvent += (GEventArgs2 args) =>
                {
                    // Only trigger on Success
                    if (args.Status != CommandStatus.Succeed)
                    {
                        return;
                    }

                    // If the item is coming from somewhere other than this container, throw it into the world, as it's the player moving an item from their inventory
                    if (args.Item.CurrentAddress?.Container != args.To.Container)
                    {
                        var lootItem = Singleton<GameWorld>.Instance.ThrowItem(args.Item, Singleton<GameWorld>.Instance.MainPlayer, null);

                        // Handle item removal
                        lootItem.ItemOwner.RemoveItemEvent += stashGridClass.OwnerRemoveItemEvent;
                    }
                };
            }
        }
    }
}
