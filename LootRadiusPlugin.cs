using BepInEx;
using DrakiaXYZ.LootRadius.Helpers;
using DrakiaXYZ.LootRadius.Patches;

namespace DrakiaXYZ.LootRadius
{
    [BepInPlugin("xyz.drakia.lootradius", "DrakiaXYZ-LootRadius", "1.4.1")]
    [BepInDependency("com.SPT.core", "3.11.0")]
    public class LootRadiusPlugin : BaseUnityPlugin
    {
        public static StashItemClass RadiusStash;

        private void Awake()
        {
            Settings.Init(Config);

            new GameStartedPatch().Enable();
            new LootPanelOpenPatch().Enable();
            new LootPanelClosePatch().Enable();
            new QuestItemDragPatch().Enable();
            new LootRadiusQuickMovePatch().Enable();
        }
    }
}
