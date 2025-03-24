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
                foreach (var player in Singleton<GameWorld>.Instance.AllAlivePlayersList)
                {
                    if (player.IsAI) continue;

                    var stash = Singleton<ItemFactoryClass>.Instance.CreateFakeStash(player.ProfileId);
                    var stashGridClass = new LootRadiusStashGrid("lootRadiusGrid", stash);
                    stash.Grids = new StashGridClass[] { stashGridClass };

                    var traderController = new TraderControllerClass(stash, LootRadiusStashGrid.GRIDID, "Nearby Items", false, EOwnerType.Profile);
                    Singleton<GameWorld>.Instance.ItemOwners.Add(traderController, default(GameWorld.GStruct126));

                    if (player.PlayerId == GamePlayerOwner.MyPlayer.PlayerId)
                    {
                        _stash = stash;
                    }
                }
            }
        }
    }
}
