using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Playnite.Controls
{
    /// <summary>
    /// Interaction logic for FadeImage.xaml
    /// </summary>
    public partial class FadeImage : UserControl, IDisposable
    {
        private enum CurrentImage
        {
            Image1,
            Image2,
            None
        }

        private CurrentImage currentImage = CurrentImage.None;
        private object currentSource = null;
        private System.Timers.Timer sourceChangeTimer = new System.Timers.Timer() { AutoReset = false };
        private object newSource;
        private readonly SynchronizationContext context;
        private CancellationTokenSource loadCancellation;

        internal Storyboard Image1FadeIn;
        internal Storyboard Image2FadeIn;
        internal Storyboard Image1FadeOut;
        internal Storyboard Image2FadeOut;
        internal Storyboard stateAnim;
        internal Storyboard BorderDarkenFadeOut;

        public double SourceUpdateDelay { get; set; } = 0;

        #region AnimationEnabled

        public static readonly DependencyProperty AnimationEnabledProperty = DependencyProperty.Register(
            nameof(AnimationEnabled),
            typeof(bool),
            typeof(FadeImage),
            new PropertyMetadata(true));

        public bool AnimationEnabled
        {
            get { return (bool)GetValue(AnimationEnabledProperty); }
            set { SetValue(AnimationEnabledProperty, value); }
        }

        #endregion AnimationEnabled

        #region Source

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(object),
            typeof(FadeImage),
            new PropertyMetadata(null, SourceChanged));

        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion Source

        #region ImageOpacityMask

        public static readonly DependencyProperty ImageOpacityMaskProperty = DependencyProperty.Register(
            nameof(ImageOpacityMask),
            typeof(Brush),
            typeof(FadeImage),
            new PropertyMetadata());

        public Brush ImageOpacityMask
        {
            get { return (Brush)GetValue(ImageOpacityMaskProperty); }
            set { SetValue(ImageOpacityMaskProperty, value); }
        }

        #endregion ImageOpacityMask

        #region ImageDarkeningBrush

        public static readonly DependencyProperty ImageDarkeningBrushProperty = DependencyProperty.Register(
            nameof(ImageDarkeningBrush),
            typeof(Brush),
            typeof(FadeImage),
            new PropertyMetadata());

        public Brush ImageDarkeningBrush
        {
            get { return (Brush)GetValue(ImageDarkeningBrushProperty); }
            set { SetValue(ImageDarkeningBrushProperty, value); }
        }

        #endregion ImageDarkeningBrush

        #region Stretch

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            nameof(Stretch),
            typeof(Stretch),
            typeof(FadeImage),
            new PropertyMetadata(Stretch.UniformToFill));

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        #endregion Strech

        #region IsBlurEnabled

        public static readonly DependencyProperty IsBlurEnabledProperty = DependencyProperty.Register(
            nameof(IsBlurEnabled),
            typeof(bool),
            typeof(FadeImage),
            new PropertyMetadata(false, BlurSettingChanged));

        public bool IsBlurEnabled
        {
            get { return (bool)GetValue(IsBlurEnabledProperty); }
            set { SetValue(IsBlurEnabledProperty, value); }
        }

        #endregion IsBlurEnabled

        #region BlurAmount

        public static readonly DependencyProperty BlurAmountProperty = DependencyProperty.Register(
            nameof(BlurAmount),
            typeof(int),
            typeof(FadeImage),
            new PropertyMetadata(10, BlurSettingChanged));

        public int BlurAmount
        {
            get { return (int)GetValue(BlurAmountProperty); }
            set { SetValue(BlurAmountProperty, value); }
        }

        #endregion IsBlurEnabled

        #region HighQualityBlur

        public static readonly DependencyProperty HighQualityBlurProperty = DependencyProperty.Register(
            nameof(HighQualityBlurProperty),
            typeof(bool),
            typeof(FadeImage),
            new PropertyMetadata(false, BlurSettingChanged));

        public bool HighQualityBlur
        {
            get { return (bool)GetValue(HighQualityBlurProperty); }
            set { SetValue(HighQualityBlurProperty, value); }
        }

        #endregion HighQualityBlur

        public FadeImage()
        {
            InitializeComponent();
            context = SynchronizationContext.Current;
            Image1FadeIn = (Storyboard)TryFindResource("Image1FadeIn");
            Image2FadeIn = (Storyboard)TryFindResource("Image2FadeIn");
            Image1FadeOut = (Storyboard)TryFindResource("Image1FadeOut");
            Image2FadeOut = (Storyboard)TryFindResource("Image2FadeOut");
            BorderDarkenFadeOut = (Storyboard)TryFindResource("BorderDarkenFadeOut");
            Image1FadeOut.Completed += Image1FadeOut_Completed;
            Image2FadeOut.Completed += Image2FadeOut_Completed;
            BorderDarkenFadeOut.Completed += BorderDarkenOut_Completed;
            sourceChangeTimer.Elapsed += (_, __) => context.Send((___) => LoadNewSource(), null);
        }

        ~FadeImage()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Cancel any pending image loading
                loadCancellation?.Cancel();
                loadCancellation?.Dispose();

                // Stop timers
                sourceChangeTimer.Stop();
                sourceChangeTimer.Dispose();

                // Clean up event handlers
                Image1FadeOut.Completed -= Image1FadeOut_Completed;
                Image2FadeOut.Completed -= Image2FadeOut_Completed;
                BorderDarkenFadeOut.Completed -= BorderDarkenOut_Completed;

                // Dispose bitmap sources
                DisposeBitmapSource(Image1.Source);
                DisposeBitmapSource(Image2.Source);

                // Clear sources
                Image1.Source = null;
                Image2.Source = null;

                // Dispose blur effect
                if (ImageHolder.Effect is BlurEffect blurEffect)
                {
                    ImageHolder.Effect = null;
                    // BlurEffect doesn't need explicit disposal, but we remove the reference
                }
            }
        }

        private void DisposeBitmapSource(object imageSource)
        {
            if (imageSource is BitmapSource bitmapSource)
            {
                // Try to freeze if not already frozen to optimize memory
                if (!bitmapSource.IsFrozen && bitmapSource.CanFreeze)
                {
                    try
                    {
                        bitmapSource.Freeze();
                    }
                    catch
                    {
                        // Ignore freeze errors
                    }
                }
            }
        }

        private void Image1FadeOut_Completed(object sender, EventArgs e)
        {
            // Properly dispose the bitmap source
            DisposeBitmapSource(Image1.Source);
            Image1.Source = null;
            Image1.UpdateLayout();
            // Remove manual GC.Collect() - let framework handle garbage collection
        }

        private void Image2FadeOut_Completed(object sender, EventArgs e)
        {
            // Properly dispose the bitmap source
            DisposeBitmapSource(Image2.Source);
            Image2.Source = null;
            Image2.UpdateLayout();
        }

        private void BorderDarkenOut_Completed(object sender, EventArgs e)
        {
            BorderDarken.Opacity = 0;
        }

        private static void BlurSettingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FadeImage)obj;
            if (control.Source == null)
            {
                return;
            }

            var blurAmount = control.BlurAmount;
            var blurEnabled = control.IsBlurEnabled;
            var highQuality = control.HighQualityBlur;
            if (blurEnabled)
            {
                control.ImageHolder.Effect = new BlurEffect()
                {
                    KernelType = KernelType.Gaussian,
                    Radius = blurAmount,
                    RenderingBias = highQuality ? RenderingBias.Quality : RenderingBias.Performance
                };
            }
            else
            {
                control.ImageHolder.Effect = null;
            }
        }

        private static void SourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FadeImage)obj;
            control.newSource = args.NewValue;

            if (control.SourceUpdateDelay > 0)
            {
                control.sourceChangeTimer.Enabled = false;
                control.sourceChangeTimer.Interval = control.SourceUpdateDelay;
                control.sourceChangeTimer.Enabled = true;
            }
            else
            {
                control.LoadNewSource();
            }
        }

        private async void LoadNewSource()
        {
            sourceChangeTimer.Enabled = false;
            if (newSource?.Equals(currentSource) == true)
            {
                return;
            }

            var blurAmount = BlurAmount;
            var blurEnabled = IsBlurEnabled;
            var highQuality = HighQualityBlur;
            BitmapSource image = null;
            currentSource = newSource;

            loadCancellation?.Cancel();
            loadCancellation?.Dispose();
            loadCancellation = new CancellationTokenSource();
            var localCancellation = loadCancellation;
            var cancelToken = localCancellation.Token;
            if (newSource != null)
            {
                try
                {
                    if (newSource is string str)
                    {
                        image = await ImageSourceManager.GetImageAsync(str, false, null, cancelToken);
                    }
                    else if (newSource is BitmapLoadProperties props)
                    {
                        image = await ImageSourceManager.GetImageAsync(props.Source, false, props, cancelToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                if (cancelToken.IsCancellationRequested || localCancellation != loadCancellation)
                {
                    // Dispose the unused image to prevent memory leaks
                    DisposeBitmapSource(image);
                    return;
                }
            }

            if (blurEnabled)
            {
                if (ImageHolder.Effect == null)
                {
                    ImageHolder.Effect = new BlurEffect()
                    {
                        KernelType = KernelType.Gaussian,
                        Radius = blurAmount,
                        RenderingBias = highQuality ? RenderingBias.Quality : RenderingBias.Performance
                    };
                }
            }
            else
            {
                if (ImageHolder.Effect != null)
                {
                    ImageHolder.Effect = null;
                }
            }

            if (AnimationEnabled)
            {
                if (image == null)
                {
                    if (currentImage == CurrentImage.None)
                    {
                        return;
                    }

                    if (currentImage == CurrentImage.Image1)
                    {
                        Image1FadeOut.Begin();
                        BorderDarkenFadeOut.Begin();
                    }
                    else if (currentImage == CurrentImage.Image2)
                    {
                        Image2FadeOut.Begin();
                        BorderDarkenFadeOut.Begin();
                    }

                    currentImage = CurrentImage.None;
                }
                else
                {
                    if (currentImage == CurrentImage.None)
                    {
                        Image1FadeOut.Stop();
                        Image1.Source = image;
                        Image1FadeIn.Begin();
                        BorderDarken.Opacity = 1;
                        BorderDarkenFadeOut.Stop();
                        currentImage = CurrentImage.Image1;
                    }
                    else if (currentImage == CurrentImage.Image1)
                    {
                        Image2FadeOut.Stop();
                        Image2.Source = image;
                        Image2FadeIn.Begin();
                        Image1FadeOut.Begin();
                        BorderDarken.Opacity = 1;
                        BorderDarkenFadeOut.Stop();
                        currentImage = CurrentImage.Image2;
                    }
                    else if (currentImage == CurrentImage.Image2)
                    {
                        Image1FadeOut.Stop();
                        Image1.Source = image;
                        Image1FadeIn.Begin();
                        Image2FadeOut.Begin();
                        BorderDarken.Opacity = 1;
                        BorderDarkenFadeOut.Stop();
                        currentImage = CurrentImage.Image1;
                    }
                }
            }
            else
            {
                if (currentImage == CurrentImage.Image1)
                {
                    Image1.Source = image;
                }
                else if (currentImage == CurrentImage.Image2)
                {
                    Image2.Source = image;
                }
                else
                {
                    Image1.Source = image;
                    currentImage = CurrentImage.Image1;
                }
            }
        }
    }
}
