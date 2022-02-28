using Microsoft.UI.Xaml;
using Xunkong.Core.Hoyolab;

namespace Xunkong.Desktop.Models
{
    internal partial class UserPanelModel : ObservableObject
    {


        private bool _HasError;
        /// <summary>
        /// 仅在列出所有玩家信息的时候使用
        /// </summary>
        public bool HasError
        {
            get => _HasError;
            set => SetProperty(ref _HasError, value);
        }



        private string? _ErrorMessage;
        /// <summary>
        /// 仅在列出所有玩家信息的时候使用
        /// </summary>
        public string? ErrorMessage
        {
            get => _ErrorMessage;
            set => SetProperty(ref _ErrorMessage, value);
        }


        private bool _IsPinned;
        public bool IsPinned
        {
            get => _IsPinned;
            set => SetProperty(ref _IsPinned, value);
        }



        private UserInfo? _UserInfo;
        public UserInfo? UserInfo
        {
            get => _UserInfo;
            set => SetProperty(ref _UserInfo, value);
        }



        private UserGameRoleInfo? _GameRoleInfo;
        public UserGameRoleInfo? GameRoleInfo
        {
            get => _GameRoleInfo;
            set => SetProperty(ref _GameRoleInfo, value);
        }


        [AlsoNotifyChangeFor(nameof(PinButtonVisibility))]
        private DailyNoteInfo? _DailyNoteInfo;
        public DailyNoteInfo? DailyNoteInfo
        {
            get => _DailyNoteInfo;
            set
            {
                SetProperty(ref _DailyNoteInfo, value);
            }
        }


        public Visibility PinButtonVisibility
        {
            get
            {
                if (Environment.OSVersion.Version >= new Version("10.0.22000.0"))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return DailyNoteInfo is null ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }



    }
}
