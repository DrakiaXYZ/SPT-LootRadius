using Comfort.Common;
using EFT;
using EFT.Interactive;

namespace DrakiaXYZ.LootRadius.Helpers
{
    internal class Utils
    {
        public static LootItem FindLootById(string id)
        {
            foreach (var loot in Singleton<GameWorld>.Instance.LootList)
            {
                // We only care about loot items
                if (loot is LootItem lootItem && lootItem.ItemId == id)
                {
                    return lootItem;
                }
            }

            return null;
        }
    }
}
