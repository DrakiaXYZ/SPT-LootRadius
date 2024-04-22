using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System.Collections;
using System.Reflection;

namespace DrakiaXYZ.LootRadius.Helpers
{
    internal class Utils
    {
        private static FieldInfo _lootListField;

        public static LootItem FindLootById(string id)
        {
            if (_lootListField == null)
            {
                _lootListField = AccessTools.Field(typeof(GameWorld), "LootList");
            }

            IList lootList = _lootListField.GetValue(Singleton<GameWorld>.Instance) as IList;
            foreach (var loot in lootList)
            {
                // We only care about loot items
                if (!(loot is LootItem))
                {
                    continue;
                }

                LootItem lootItem = (LootItem)loot;
                if (lootItem.ItemId == id)
                {
                    return lootItem;
                }
            }

            return null;
        }
    }
}
