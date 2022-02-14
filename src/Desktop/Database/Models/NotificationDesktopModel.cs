using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.Models
{

    [Table("Notifications")]
    [Index(nameof(Category))]
    [Index(nameof(HasRead))]
    public class NotificationDesktopModel : NotificationModelBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private bool _HasRead;
        public bool HasRead
        {
            get { return _HasRead; }
            set
            {
                _HasRead = value;
                OnPropertyChanged();
            }
        }

        //public bool HasRead { get; set; }


    }
}
