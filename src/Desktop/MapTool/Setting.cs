namespace Xunkong.Desktop.MapTool
{
    public class Setting
    {

        public bool DontShowTitleBarWhenActived { get; set; }

        public double OpacityWhenActived { get; set; } = 1;

        public bool DisableClickTransparentWhenDeactived { get; set; }

        public double OpacityWhenDeactived { get; set; } = 0.6;

        public bool UseYuanShenSite { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

    }
}
