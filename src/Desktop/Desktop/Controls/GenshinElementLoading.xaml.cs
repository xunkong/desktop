using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls
{
    public sealed partial class GenshinElementLoading : UserControl
    {

        private Compositor? _compositor;

        private AmbientLight? _ambientLight;

        private PointLight? _pointLight;



        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(GenshinElementLoading), new PropertyMetadata(true, IsActivePropertyChanged));





        public bool SyncActiveAndVisibility
        {
            get { return (bool)GetValue(SyncActiveAndVisibilityProperty); }
            set { SetValue(SyncActiveAndVisibilityProperty, value); }
        }

        public static readonly DependencyProperty SyncActiveAndVisibilityProperty =
            DependencyProperty.Register("SyncActiveAndVisibility", typeof(bool), typeof(GenshinElementLoading), new PropertyMetadata(false));





        public GenshinElementLoading()
        {
            this.InitializeComponent();
            _Image_Element.SizeChanged += _Image_Element_SizeChanged;
        }



        private static void IsActivePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var loading = (GenshinElementLoading)sender;
            if ((bool)e.NewValue)
            {
                loading._pointLight?.Targets.RemoveAll();
                loading._pointLight?.Targets.Add(ElementCompositionPreview.GetElementVisual(loading._Image_Element));
                if (loading.SyncActiveAndVisibility)
                {
                    loading.Visibility = Visibility.Visible;
                }
            }
            else
            {
                loading._pointLight?.Targets.RemoveAll();
                if (loading.SyncActiveAndVisibility)
                {
                    loading.Visibility = Visibility.Collapsed;
                }
            }
        }




        private void _Image_Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitializeLighting();
        }



        private void InitializeLighting()
        {
            //get interop compositor
            _compositor = ElementCompositionPreview.GetElementVisual(_Image_Element).Compositor;

            //get interop visual for XAML TextBlock
            var image = ElementCompositionPreview.GetElementVisual(_Image_Element);

            _ambientLight = _compositor.CreateAmbientLight();
            _ambientLight.Color = Colors.White;
            _ambientLight.Targets.Add(image); //target XAML TextBlock
            _ambientLight.Intensity = 0.3f;

            _pointLight = _compositor.CreatePointLight();
            _pointLight.Color = Colors.White;
            _pointLight.Intensity = 1f;
            _pointLight.CoordinateSpace = image; //set up co-ordinate space for offset
            _pointLight.Targets.Add(image); //target XAML TextBlock

            //starts out to the left; vertically centered; light's z-offset is related to fontsize
            _pointLight.Offset = new Vector3(-(float)_Image_Element.ActualHeight * 4, (float)_Image_Element.ActualHeight / 2, (float)_Image_Element.ActualHeight * 2);

            //simple offset.X animation that runs forever
            var animation = _compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(0.5f, (float)(_Image_Element.ActualWidth + _Image_Element.ActualHeight * 4));
            animation.InsertKeyFrame(1, -(float)_Image_Element.ActualHeight * 4);
            animation.Duration = TimeSpan.FromSeconds(1.6f);
            animation.IterationBehavior = AnimationIterationBehavior.Forever;

            _pointLight.StartAnimation("Offset.X", animation);
        }





    }
}
