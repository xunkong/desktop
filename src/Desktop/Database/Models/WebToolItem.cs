using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xunkong.Desktop.Models
{

    [Table("WebToolItems")]
    public class WebToolItem : ObservableObject
    {

        private int _Id;
        public int Id
        {
            get => _Id;
            set => SetProperty(ref _Id, value);
        }


        private string? _Title;
        public string? Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }


        private string? _Icon;
        public string? Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }


        private int _Order;
        public int Order
        {
            get => _Order;
            set => SetProperty(ref _Order, value);
        }


        private string _Url;
        public string Url
        {
            get => _Url;
            set => SetProperty(ref _Url, value);
        }


        private string? _JavaScript;
        public string? JavaScript
        {
            get => _JavaScript;
            set => SetProperty(ref _JavaScript, value);
        }

    }

}
