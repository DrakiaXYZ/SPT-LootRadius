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
                _stash = Singleton<ItemFactoryClass>.Instance.CreateFakeStash("67e07d6d4d6c60afff004b41");

                var stashGridClass = new LootRadiusStashGrid("lootRadiusGrid", _stash);
                _stash.Grids = new StashGridClass[] { stashGridClass };

                var traderController = new TraderControllerClass(_stash, LootRadiusStashGrid.GRIDID, "Nearby Items", false, EOwnerType.Profile);
                Singleton<GameWorld>.Instance.ItemOwners.Add(traderController, default(GameWorld.GStruct126));
            }
        }
    }
}
