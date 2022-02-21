using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Toolbox
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class ToolboxAttribute : Attribute
    {

        public string Title { get; init; }


        public string Icon { get; init; }


        public string? Description { get; init; }


        public ToolboxAttribute(string title, string icon, string? description = null)
        {
            Title = title;
            Icon = icon;
            Description = description;
        }


    }
}
