﻿using Grinderv2.GUI;
using System.Linq;
using TreeTaskCore;
using ZzukBot.Core.Constants;
using ZzukBot.Core.Game.Frames;
using ZzukBot.Core.Game.Statics;

namespace Grinderv2.TreeTasks
{
    public class Vendor : TTask
    {
        public override int Priority => 501;

        public override bool Activate()
        {
            return Settings.Vendor && CMD.ShouldVendor && ObjectManager.Instance.Units.Any(x => CMD.VendorWhitelist.Any(y => y.Id == x.Id) && !x.IsDead && ObjectManager.Instance.Player.InLosWith(x));
        }

        public override void Execute()
        {
            var vendor = ObjectManager.Instance.Units
                .Where(x => CMD.VendorWhitelist.Any(y => y.Id == x.Id) && !x.IsDead && ObjectManager.Instance.Player.InLosWith(x))
                .OrderBy(x => x.DistanceToPlayer).FirstOrDefault();
            if (!MerchantFrame.IsOpen)
            {
                if (vendor.DistanceToPlayer < 3f)
                    vendor.Interact(false);
                else
                    Navigation.Instance.Traverse(vendor.Position);
            }
            if (MerchantFrame.IsOpen)
            {
                var frame = MerchantFrame.Instance;

                if (frame.CanRepair && frame.TotalRepairCost < ObjectManager.Instance.Player.Money)
                    frame.RepairAll();

                var item = Inventory.Instance.GetAllItems().FirstOrDefault(x =>
                (!Settings.KeepGreens || !x.Quality.Equals(Enums.ItemQuality.Uncommon)) &&
                (!Settings.KeepBlues || !x.Quality.Equals(Enums.ItemQuality.Rare)) &&
                (!Settings.KeepPurples || !x.Quality.Equals(Enums.ItemQuality.Epic)) &&
                !Settings.ProtectedItems.Contains(x.Name) && !x.Info.SellPrice.Equals(0));

                if (item != null)
                    frame.VendorByGuid(item.Guid);
                else
                    CMD.ShouldVendor = false;
            }
        }
    }
}
