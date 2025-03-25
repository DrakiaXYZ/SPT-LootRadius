using SPT.Reflection.Patching;
using EFT.InventoryLogic;
using EFT;
using System.Reflection;
using Comfort.Common;
using DrakiaXYZ.LootRadius.Helpers;
using System.Text;
using System.Security.Cryptography;

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
                foreach (var player in Singleton<GameWorld>.Instance.RegisteredPlayers)
                {
                    if (player.IsAI) continue;

                    // We will use a stash ID generated based on the profile ID, so it's constant, but doesn't collide with BSG's uses
                    string stashId = GetProfileStashId(player.ProfileId);
                    var stash = Singleton<ItemFactoryClass>.Instance.CreateFakeStash(stashId);
                    var stashGridClass = new LootRadiusStashGrid("lootRadiusGrid", stash);
                    stash.Grids = new StashGridClass[] { stashGridClass };

                    var traderController = new TraderControllerClass(stash, LootRadiusStashGrid.GRIDID, "Nearby Items", false, EOwnerType.Profile);
                    Singleton<GameWorld>.Instance.ItemOwners.Add(traderController, default(GameWorld.GStruct126));

                    if (player.ProfileId == GamePlayerOwner.MyPlayer.ProfileId)
                    {
                        _stash = stash;
                    }
                }
            }
        }

        private static string GetProfileStashId(string profileId)
        {
            byte[] encodedProfileId = Encoding.UTF8.GetBytes(profileId);
            byte[] hashBytes = (new SHA256Managed()).ComputeHash(encodedProfileId);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString().Substring(0, 24).ToLower();
        }
    }
}
