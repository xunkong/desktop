﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;


/// <summary>
/// 图片查看器
/// </summary>
/// <remarks>
/// <see cref="Image"/> 在解码后属性 <see cref="FrameworkElement.ActualWidth"/>和<see cref="FrameworkElement.ActualHeight"/> 实际上是图片像素的宽高；
/// 再经过系统的缩放后，在显示器上实际占用的像素值会比图片像素要多，一定程度上造成了图片模糊；
/// 查看器中通过引入系统缩放率 <see cref="MainWindowHelper.UIScale"/> 解决这个问题。
/// </remarks>
[INotifyPropertyChanged]
public sealed partial class ImageViewer : UserControl
{



    public ImageViewer()
    {
        this.InitializeComponent();
        _ScrollViewer_Image.MaxZoomFactor = (float)(2 / MainWindowHelper.UIScale);
    }



    [ObservableProperty]
    private string source;


    [ObservableProperty]
    private List<string>? sourceCollection;



    partial void OnSourceCollectionChanged(List<string>? value)
    {
        _GridView_ImageCollection.Visibility = Visibility.Visible;
    }



    /// <summary>
    /// 缩放
    /// </summary>
    /// <param name="factor">缩放因子</param>
    private void Zoom(double factor)
    {
        var newFactor = Math.Clamp(factor, 0.1, 2 / MainWindowHelper.UIScale);
        var oldFactor = _ScrollViewer_Image.ZoomFactor;
        var offset_Width = _ScrollViewer_Image.HorizontalOffset;
        var offset_Height = _ScrollViewer_Image.VerticalOffset;
        var viewport_Width = _ScrollViewer_Image.ViewportWidth;
        var viewport_Height = _ScrollViewer_Image.ViewportHeight;
        var extent_Width = _ScrollViewer_Image.ExtentWidth;
        var extent_Height = _ScrollViewer_Image.ExtentHeight;
        //var extent_Width_New = extent_Width * newFactor / oldFactor;
        //var extent_Height_New = extent_Height * newFactor / oldFactor;

        // 保证图片以画面中心为基点进行缩放
        double offset_Width_New = 0, offfset_Height_New = 0;
        if (extent_Width < viewport_Width)
        {
            // 如果缩放前图片宽度 < 视图宽度，则以图片半宽为基准缩放
            offset_Width_New = extent_Width / 2 * newFactor / oldFactor - viewport_Width / 2;
        }
        else
        {
            // 如果缩放前图片宽度 > 视图宽度，则以视图中心点到图片边缘为基准缩放
            offset_Width_New = (viewport_Width / 2 + offset_Width) * newFactor / oldFactor - viewport_Width / 2;
        }
        if (extent_Height < viewport_Height)
        {
            offfset_Height_New = extent_Height / 2 * newFactor / oldFactor - viewport_Height / 2;
        }
        else
        {
            offfset_Height_New = (viewport_Height / 2 + offset_Height) * newFactor / oldFactor - viewport_Height / 2;
        }

        // 缩放后若填不满视图，则图片居中（这部分没用）
        //if (extent_Width_New < viewport_Width)
        //{
        //    offset_Width_New = (extent_Width_New - viewport_Width) / 2;
        //}
        //if (extent_Height_New < viewport_Height)
        //{
        //    offfset_Height_New = (extent_Height_New - viewport_Height) / 2;
        //}

        _ScrollViewer_Image.ChangeView(offset_Width_New, offfset_Height_New, (float)newFactor);
    }



    /// <summary>
    /// 缩小
    /// </summary>
    [RelayCommand]
    private void ZoomOut()
    {
        // 缩放率调整为 0.1 的倍数
        var factor = Math.Ceiling(_ScrollViewer_Image.ZoomFactor * 10 * MainWindowHelper.UIScale - 1.1f) / 10 / MainWindowHelper.UIScale;
        Zoom(factor);
    }


    /// <summary>
    /// 放大
    /// </summary>
    [RelayCommand]
    private void ZoomIn()
    {
        var factor = Math.Floor(_ScrollViewer_Image.ZoomFactor * 10 * MainWindowHelper.UIScale + 1.1f) / 10 / MainWindowHelper.UIScale;
        Zoom(factor);
    }




    /// <summary>
    /// 顺时针旋转90°
    /// </summary>
    [RelayCommand]
    private void Rotate()
    {
        _Image.Rotation = _Image.Rotation + 90;
    }


    /// <summary>
    /// 复制图片
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task CopyImageAsync()
    {
        try
        {

            RandomAccessStreamReference reference;
            if (Source.StartsWith("ms-appx"))
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(Source));
                reference = RandomAccessStreamReference.CreateFromFile(file);
            }
            else
            {
                if (!File.Exists(Source))
                {
                    NotificationProvider.Warning("找不到缓存的文件", 3000);
                    return;
                }
                var file = await StorageFile.GetFileFromPathAsync(Source);
                reference = RandomAccessStreamReference.CreateFromFile(file);
            }
            var data = new DataPackage();
            data.RequestedOperation = DataPackageOperation.Copy;

            data.SetBitmap(reference);
            Clipboard.SetContent(data);
            _Button_Copy.Content = "\xE8FB";
            await Task.Delay(3000);
            _Button_Copy.Content = "\xE8C8";
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
        }
    }



    /// <summary>
    /// 关闭页面
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        MainWindowHelper.CloseFullScreen();
    }



    /// <summary>
    /// 缩放率文本变更
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        _TextBlock_Factor.Text = (_ScrollViewer_Image.ZoomFactor * MainWindowHelper.UIScale).ToString("P0");
    }



    /// <summary>
    /// 图片加载后计算合适的缩放率
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Image_ImageExOpened(object sender, CommunityToolkit.WinUI.UI.Controls.ImageExOpenedEventArgs e)
    {
        var width = _Image.ActualWidth;
        var height = _Image.ActualHeight;
        if (width * height == 0)
        {
            return;
        }
        _Image.CenterPoint = new System.Numerics.Vector3((float)(width / 2), (float)(height / 2), 0);
        var factor = GetFitZoomFactor();
        _TextBlock_Factor.Text = (factor * MainWindowHelper.UIScale).ToString("P0");
        _ScrollViewer_Image.ZoomToFactor((float)factor);
    }


    /// <summary>
    /// 图片加载后计算合适的缩放率
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Image_ImageOpened(object sender, RoutedEventArgs e)
    {
        var width = _Image.ActualWidth;
        var height = _Image.ActualHeight;
        if (width * height == 0)
        {
            return;
        }
        _Image.CenterPoint = new System.Numerics.Vector3((float)(width / 2), (float)(height / 2), 0);
        var factor = GetFitZoomFactor();
        _TextBlock_Factor.Text = (factor * MainWindowHelper.UIScale).ToString("P0");
        _ScrollViewer_Image.ZoomToFactor((float)factor);
    }

    /// <summary>
    /// 计算适合窗口的图片缩放率
    /// </summary>
    /// <returns></returns>
    private double GetFitZoomFactor()
    {
        double factor;
        if (_Image.Rotation % 180 == 0)
        {
            var widthFactor = _ScrollViewer_Image.ViewportWidth / _Image.ActualWidth;
            var heightFactor = _ScrollViewer_Image.ViewportHeight / _Image.ActualHeight;
            factor = Math.Min(widthFactor, heightFactor);
        }
        else
        {
            var widthFactor = _ScrollViewer_Image.ViewportHeight / _Image.ActualWidth;
            var heightFactor = _ScrollViewer_Image.ViewportWidth / _Image.ActualHeight;
            factor = Math.Min(widthFactor, heightFactor);
        }
        return Math.Min(factor, 1 / MainWindowHelper.UIScale);
    }


    /// <summary>
    /// 鼠标单击，显示或隐藏图片缩率图栏
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ChangeToolBarVisibility();
    }


    /// <summary>
    /// 更改工具栏的可见性
    /// </summary>
    private void ChangeToolBarVisibility()
    {
        if (_Border_ToolBar.IsHitTestVisible)
        {
            _Border_ToolBar.Opacity = 0;
            _Border_ToolBar.IsHitTestVisible = false;
            if (SourceCollection?.Any() ?? false)
            {
                _GridView_ImageCollection.Opacity = 0;
                _GridView_ImageCollection.IsHitTestVisible = false;
            }
        }
        else
        {
            _Border_ToolBar.Opacity = 1;
            _Border_ToolBar.IsHitTestVisible = true;
            if (SourceCollection?.Any() ?? false)
            {
                _GridView_ImageCollection.Opacity = 1;
                _GridView_ImageCollection.IsHitTestVisible = true;
            }
        }
    }


    /// <summary>
    /// 鼠标双击，放大或缩小
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ChangeToolBarVisibility();
        var oldFactor = _ScrollViewer_Image.ZoomFactor;
        double newFactor;
        if (oldFactor < 0.4 / MainWindowHelper.UIScale)
        {
            newFactor = oldFactor * 2;
        }
        else if (oldFactor < 0.999 / MainWindowHelper.UIScale)
        {
            newFactor = 1 / MainWindowHelper.UIScale;
        }
        else
        {
            newFactor = GetFitZoomFactor();
        }
        Zoom(newFactor);
    }


    /// <summary>
    /// 鼠标滚动，切换图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Image_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var stats = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
        if (stats == CoreVirtualKeyStates.None)
        {
            if (SourceCollection?.Any() ?? false)
            {
                var index = _GridView_ImageCollection.SelectedIndex;
                var count = SourceCollection.Count;
                var pointer = e.GetCurrentPoint(_ScrollViewer_Image);
                if (pointer.Properties.MouseWheelDelta < 0)
                {
                    if (index < count - 1)
                    {
                        _GridView_ImageCollection.SelectedIndex = index + 1;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        _GridView_ImageCollection.SelectedIndex = index - 1;
                    }
                }
            }
            // 阻止事件传递到滚动条
            e.Handled = true;
        }
    }



    private bool canImageMoved;

    private Point oldPosition;


    private void _ScrollViewer_Image_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        canImageMoved = true;
        oldPosition = e.GetCurrentPoint(_ScrollViewer_Image).Position;
    }

    /// <summary>
    /// 鼠标拖动图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (canImageMoved)
        {
            var pointer = e.GetCurrentPoint(_ScrollViewer_Image);
            if (pointer.Properties.IsLeftButtonPressed)
            {
                var deltaX = pointer.Position.X - oldPosition.X;
                var deltaY = pointer.Position.Y - oldPosition.Y;
                oldPosition = pointer.Position;
                // offset 的方向应与鼠标移动的方向相反
                // 不要使用 ChangeView，会出现图片无法跟随鼠标的情况
                _ScrollViewer_Image.ScrollToHorizontalOffset(_ScrollViewer_Image.HorizontalOffset - deltaX);
                _ScrollViewer_Image.ScrollToVerticalOffset(_ScrollViewer_Image.VerticalOffset - deltaY);
            }
        }
    }

    private void _ScrollViewer_Image_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        canImageMoved = false;
    }



    /// <summary>
    /// 拖入更改图片，测试用
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_Drop(object sender, DragEventArgs e)
    {
        //if (e.DataView.Contains(StandardDataFormats.StorageItems))
        //{
        //    var items = await e.DataView.GetStorageItemsAsync();
        //    if (items.Any())
        //    {
        //        var file = items.FirstOrDefault()?.Path;
        //        ImageSource = file;
        //    }
        //}
    }

    private void _ScrollViewer_Image_DragOver(object sender, DragEventArgs e)
    {
        //e.AcceptedOperation = DataPackageOperation.Copy;
        //e.DragUIOverride.IsCaptionVisible = false;
        //e.DragUIOverride.IsGlyphVisible = false;
    }


}
