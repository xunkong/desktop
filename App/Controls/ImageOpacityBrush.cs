using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Numerics;

namespace Xunkong.Desktop.Controls;

internal class ImageOpacityBrush : XamlCompositionBrushBase
{

    private CompositionEffectFactory _alphaMaskEffectFactory;
    private CompositionEffectBrush _alphaMaskEffectBrush;
    private AlphaMaskEffect _alphaMaskEffect;
    private CompositionSurfaceBrush _surfaceBrush;
    private LoadedImageSurface _surface;
    private CompositionLinearGradientBrush _maskGradient;

    private Compositor _compositor = new Compositor();





    public string Source
    {
        get { return (string)GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register("Source", typeof(string), typeof(ImageOpacityBrush), new PropertyMetadata(null, OnImageSourceUriChanged));



    private static void OnImageSourceUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var oldValue = e.OldValue as string;
        var newValue = e.NewValue as string;
        if (oldValue == newValue)
        {
            return;
        }
        (d as ImageOpacityBrush)?.OnImageSourceUriChanged(oldValue, newValue);
    }

    protected virtual void OnImageSourceUriChanged(string? oldValue, string? newValue)
    {
        UpdateSurface();
    }


    private void UpdateSurface()
    {
        //if (Source != null && _surfaceBrush != null)
        //{
        //var uri = new Uri(Source);
        _surface = LoadedImageSurface.StartLoadFromUri(new(@"D:\Downloads\Download\#蛍(原神) 空蛍ログ③ - 八依莉的插画 - pixiv\95075873_p0.png"));
        _surfaceBrush.Surface = _surface;
        //}
    }



    protected override void OnConnected()
    {
        if (CompositionBrush is null)
        {
            _surfaceBrush = _compositor.CreateSurfaceBrush();
            _surfaceBrush.Stretch = CompositionStretch.None;

            UpdateSurface();

            _maskGradient = _compositor.CreateLinearGradientBrush();
            _maskGradient.StartPoint = Vector2.Zero;
            _maskGradient.EndPoint = Vector2.UnitY;
            _maskGradient.ColorStops.Add(_compositor.CreateColorGradientStop(0, Colors.Black));
            _maskGradient.ColorStops.Add(_compositor.CreateColorGradientStop(1, Colors.Transparent));

            _alphaMaskEffect = new AlphaMaskEffect()
            {
                Source = new CompositionEffectSourceParameter("Source"),
                AlphaMask = new CompositionEffectSourceParameter("AlphaMask"),
            };

            _alphaMaskEffectFactory = _compositor.CreateEffectFactory(_alphaMaskEffect);
            _alphaMaskEffectBrush = _alphaMaskEffectFactory.CreateBrush();
            _alphaMaskEffectBrush.SetSourceParameter("Source", _surfaceBrush);
            _alphaMaskEffectBrush.SetSourceParameter("AlphaMask", _maskGradient);
            CompositionBrush = _alphaMaskEffectBrush;
        }
    }


    protected override void OnDisconnected()
    {
        if (CompositionBrush is not null)
        {
            CompositionBrush.Dispose();
            CompositionBrush = null;
        }
        if (_alphaMaskEffectFactory is not null)
        {
            _alphaMaskEffectFactory.Dispose();
            _alphaMaskEffectFactory = null;
        }
        if (_alphaMaskEffectBrush is not null)
        {
            _alphaMaskEffectBrush.Dispose();
            _alphaMaskEffectBrush = null;
        }
        if (_alphaMaskEffect is not null)
        {
            _alphaMaskEffect.Dispose();
            _alphaMaskEffect = null;
        }
        if (_surfaceBrush is not null)
        {
            _surfaceBrush.Dispose();
            _surfaceBrush = null;
        }
        if (_surface is not null)
        {
            _surface.Dispose();
            _surface = null;
        }
        if (_maskGradient is not null)
        {
            _maskGradient.Dispose();
            _maskGradient = null;
        }
    }



}
