﻿using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.BidControls.ViewModels
{
    public class AuctionViewModel : ObservableObject
    {
        public Auction Auction { get; set; } = new();

        public void UpdateAuction(Auction auction)
        {
            Auction = ObjectCloner.ObjectCloner.DeepClone(auction);
            OnPropertyChanged(nameof(Auction));
        }
    }
}
