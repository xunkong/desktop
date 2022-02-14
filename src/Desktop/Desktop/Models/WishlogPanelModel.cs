using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunkong.Core.Wish;

namespace Xunkong.Desktop.Models
{
    public class WishlogPanelModel : ObservableObject
    {

        public int Uid { get; set; }


        private string? _NickName;
        public string? NickName
        {
            get { return _NickName; }
            set
            {
                _NickName = value;
                OnPropertyChanged();
            }
        }



        private int _WishlogCount;
        public int WishlogCount
        {
            get => _WishlogCount;
            set => SetProperty(ref _WishlogCount, value);
        }



        private DateTimeOffset _LastUpdateTime;
        public DateTimeOffset LastUpdateTime
        {
            get => _LastUpdateTime;
            set => SetProperty(ref _LastUpdateTime, value);
        }



        private ObservableCollection<WishlogPanelWishTypeStatsModel> _WishTypeStats = new();
        public ObservableCollection<WishlogPanelWishTypeStatsModel> WishTypeStats
        {
            get => _WishTypeStats;
            set => SetProperty(ref _WishTypeStats, value);
        }



        private bool _IsRunning;
        public bool IsRunning
        {
            get => _IsRunning;
            set => SetProperty(ref _IsRunning, value);
        }


        private string _RunningStep;
        public string RunningStep
        {
            get => _RunningStep;
            set => SetProperty(ref _RunningStep, value);
        }


        public int RandomId { get; set; }


    }



    public class WishlogPanelWishTypeStatsModel
    {

        public WishType WishType { get; set; }


        public bool AnyData => TotalCount > 0;


        public int TotalCount { get; set; }


        public int Rank5Count { get; set; }


        public string Rank5Ratio => ((double)Rank5Count / TotalCount).ToString("P3");


        public int Rank4Count { get; set; }


        public string Rank4Ratio => ((double)Rank4Count / TotalCount).ToString("P3");


        public int Rank5Guarantee { get; set; }


        public int Rank4Guarantee { get; set; }


        public string? LastRank5ItemName { get; set; }


        public string? LastRank5ItemIcon { get; set; }


        public int LastRank5ItemGuaranteeIndex { get; set; }


        public DateTimeOffset LastRank5ItemTime { get; set; }

    }





}
