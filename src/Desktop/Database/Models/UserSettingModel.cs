using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xunkong.Desktop.Models
{

    [Table("UserSettings")]
    public class UserSettingModel
    {

        [Key]
        public string Key { get; set; }


        public string? Value { get; set; }


    }
}
