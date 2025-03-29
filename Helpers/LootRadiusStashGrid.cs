using EFT.InventoryLogic;
using System;

using StashGridCollectionClass = GClass2924;
using FreeSpaceInventoryErrorClass = StashGridClass.GClass3784;
using FilterInventoryErrorClass = StashGridClass.GClass3785;
using RemoveInventoryErrorClass = StashGridClass.GClass3786;
using MaxCountInventoryErrorClass = StashGridClass.GClass3788;
using ContainerRemoveEventClass = GClass3205;
using ContainerAddEventClass = GClass3207;
using ContainerRemoveEventResultStruct = GStruct455<GClass3205>;
using ContainerAddEventResultStruct = GStruct455<GClass3207>;
using EFT.UI.DragAndDrop;
using EFT;
using Comfort.Common;


namespace DrakiaXYZ.LootRadius.Helpers
{
    /**
     * Custom StashGrid implementation that doesn't do parent ownership validation, and only allows removing items
     */
    class LootRadiusStashGrid : StashGridClass
    {
        public static string GRIDID = "67e0b18aeef9ae200b0495f0";
        public GridView[] GridViews { get; set; } = null;

        public override StashGridCollectionClass ItemCollection { get; } = new LootRadiusStashGridCollection();

        public LootRadiusStashGrid(string id, CompoundItem parentItem) : 
            base(id, 10, 10, true, false, Array.Empty<ItemFilter>(), parentItem, -1) { }

        /**
         * Don't allow moving items around in the custom grid, but allow adding new items
         */
        public override bool CheckCompatibility(Item item)
        {
            return !this.Contains(item);
        }

        /**
         * Simplified item adding, as we know the incoming data is sane. This removes any chance of accidentally changing the item address
         */
        public override ContainerAddEventResultStruct AddInternal(Item item, LocationInGrid location, bool simulate, bool ignoreRestrictions)
        {
            if (location == null)
            {
                return new FreeSpaceInventoryErrorClass(item, this);
            }

            if (!ignoreRestrictions && !this.CheckCompatibility(item))
            {
                return new FilterInventoryErrorClass(item, this);
            }

            GInterface381 resizeResult = default(GStruct395);
            var newAddress = this.CreateItemAddress(location);
            if (simulate)
            {
                return new ContainerAddEventClass(this, item, newAddress, item.StackObjectsCount, resizeResult, true);
            }

            XYCellSizeStruct originalGridSize = new XYCellSizeStruct(this.GridWidth, this.GridHeight);
            this.method_9(item, location);
            XYCellSizeStruct newGridSize = new XYCellSizeStruct(this.GridWidth, this.GridHeight);
            
            if (originalGridSize != newGridSize)
            {
                resizeResult = new GStruct396(this, originalGridSize, newGridSize);
            }

            return new ContainerAddEventClass(this, item, newAddress, item.StackObjectsCount, resizeResult, false);
        }

        /**
         * More simple item removal handling
         */
        public override ContainerRemoveEventResultStruct RemoveInternal(Item item, bool simulate, bool ignoreRestrictions)
        {
            if (!base.Contains(item))
            {
                return new RemoveInventoryErrorClass(item, this);
            }

            LocationInGrid locationInGrid = this.ItemCollection[item];
            if (!simulate)
            {
                base.method_10(item, locationInGrid, true);
            }
            return new ContainerRemoveEventClass(item, base.CreateItemAddress(locationInGrid), simulate);
        }

        public void OwnerRemoveItemEvent(GEventArgs3 args)
        {
            if (args.Status != CommandStatus.Succeed)
            {
                return;
            }

            // Child items of items in the grid, we don't want to actually remove them, let the grid handle it
            if (args.From.Container.ParentItem != args.Item)
            {
                return;
            }

            var owner = Singleton<GameWorld>.Instance.FindOwnerById(args.OwnerId);
            owner.RemoveItemEvent -= this.OwnerRemoveItemEvent;

            // If we have GridViews we can update, try to remove this item from them
            if (GridViews != null && this.ItemCollection.ContainsKey(args.Item))
            {
                var locationInGrid = this.ItemCollection[args.Item];
                var item = args.Item;
                var location = CreateItemAddress(locationInGrid);

                foreach (var gridView in GridViews)
                {
                    gridView.OnItemRemoved(new GEventArgs3(item, location, CommandStatus.Begin, owner));
                    gridView.OnItemRemoved(new GEventArgs3(item, location, CommandStatus.Succeed, owner));
                }
            }

            this.RemoveInternal(args.Item, false, false);
        }

        /**
         * Custom grid collection that doesn't do address validation
         */
        internal class LootRadiusStashGridCollection : StashGridCollectionClass
        {
            public override void Add(Item item, StashGridClass grid, LocationInGrid location)
            {
                this.dictionary_0[item] = location;
                this.list_0.Add(item);

                if (item.CurrentAddress == null)
                {
                    item.CurrentAddress = grid.CreateItemAddress(location);
                }
            }

            public override void Remove(Item item, StashGridClass grid)
            {
                this.dictionary_0.Remove(item);
                this.list_0.Remove(item);

                if (item.CurrentAddress?.Container?.ID == grid.ID)
                {
                    item.CurrentAddress = null;
                }
            }
        }
    }
}
