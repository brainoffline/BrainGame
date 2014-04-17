using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using Windows.UI.Xaml.Shapes;
using Brain.Animate;
using Brain.Utils;

namespace BrainGame.Controls
{
    public enum BoxtanaAction
    {
        Entrance1,

        RandomWait,

        Swing1,
        Rotation1,
        Rotation2,
        Pulse1,
        Color1,

        Exit1,
    }

    public sealed partial class Boxtana
    {
        public Boxtana()
        {
            InitializeComponent();
        }

        public void Hide()
        {
            OuterGrid.Opacity = 0;
            InnerGrid.Opacity = 0;
        }

        public async Task Do(BoxtanaAction boxtanaAction)
        {
            if (AnimationManager.IsAnimating(OuterGrid) ||
                AnimationManager.IsAnimating(InnerGrid))
            {
                AnimationManager.PauseAnimations(InnerGrid);
                AnimationManager.PauseAnimations(OuterGrid);
                var tasks = new Task[]
                {
                    OuterGrid.AnimateAsync(new SlowResetAnimation() ),
                    InnerGrid.AnimateAsync(new SlowResetAnimation() ),
                };
                await Task.WhenAll(tasks);
                AnimationManager.StopAnimations(InnerGrid);
                AnimationManager.StopAnimations(OuterGrid);
            }

            switch (boxtanaAction)
            {
                case BoxtanaAction.Entrance1: 
                    await Entrance1();
                    break;
                case BoxtanaAction.Exit1: 
                    await Exit1();
                    break;
                case BoxtanaAction.RandomWait:
                    await RandomWait(3);
                    break;
                case BoxtanaAction.Swing1:
                    await Swing1();
                    break;
                case BoxtanaAction.Rotation1:
                    await Rotation(true);
                    break;
                case BoxtanaAction.Rotation2:
                    await Rotation(false);
                    break;
                case BoxtanaAction.Pulse1:
                    await Pulse();
                    break;
                case BoxtanaAction.Color1:
                    await Color1();
                    break;
            }
        }

        private async Task Entrance1()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new BounceInAnimation { FromDirection = ZDirection.Closer, Duration = 0.5}),
                InnerGrid.AnimateAsync(new BounceInAnimation { FromDirection = ZDirection.Away, Duration = 0.5}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task RandomWait(int repeatCount)
        {
            int i = RandomManager.Next(3);
            switch (i)
            {
                case 0:
                    await Rotation(true, repeatCount);
                    break;
                case 1:
                    await Rotation(false, repeatCount);
                    break;
                case 2:
                    await Pulse(repeatCount);
                    break;
            }
        }

        private async Task Exit1()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new BounceOutAnimation { ToDirection = ZDirection.Closer, Duration = 0.5}),
                InnerGrid.AnimateAsync(new BounceOutAnimation { ToDirection = ZDirection.Away, Duration = 0.5}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Swing1()
        {
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new FlipEnjoyAnimation() ),
                InnerGrid.AnimateAsync(new FlipEnjoyAnimation { Reverse = true} ),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Rotation(bool forward, int repeatCount = -1)
        {
            bool forever = repeatCount < 0;
            var tasks = new Task[]
            {
                OuterGrid.AnimateAsync(new SlowRotateAnimation { Reverse = forward, Forever = forever, RepeatCount = repeatCount} ),
                InnerGrid.AnimateAsync(new SlowRotateAnimation { Reverse = !forward, Forever = forever, RepeatCount = repeatCount } ),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Pulse(int repeatCount = -1)
        {
            bool forever = repeatCount < 0;
            var tasks = new Task[]
            {
                Outer.AnimateAsync(new LinePulseAnimation { Duration = 7, Forever = forever, RepeatCount = repeatCount}),
                InnerGrid.AnimateAsync(new SizePulseAnimation { Duration = 7, Reverse = true, Forever = forever, RepeatCount = repeatCount}),
            };
            await Task.WhenAll(tasks);
        }

        private async Task Color1()
        {
            var tasks = new Task[]
            {
                boxtana.AnimateAsync(new RainbowAnimation() ),
            };
            await Task.WhenAll(tasks);
        }
    }


    public class RainbowAnimation : AnimationDefinition
    {
        public RainbowAnimation()
        {
            Duration = 5;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            var color = Colors.Aquamarine;

            var brush = (SolidColorBrush) element.GetValue(Control.ForegroundProperty);
            if (brush != null)
                color = brush.Color;

            return new Timeline[]
            {
                element.AnimateColorProperty("(Control.Foreground).(SolidColorBrush.Color)")
                    .AddEasingKeyFrame(Duration * 0.25, Colors.Red)
                    .AddEasingKeyFrame(Duration * 0.5, Colors.Yellow)
                    .AddEasingKeyFrame(Duration * 0.75, Colors.Blue)
                    .AddEasingKeyFrame(Duration, color)
            };
        }
    }


    public class FlipEnjoyAnimation : AnimationDefinition
    {
        public bool Reverse;

        public FlipEnjoyAnimation()
        {
            Duration = 2;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            return new Timeline[]
            {
                element.AnimateProperty(Reverse ? AnimationProperty.RotationX : AnimationProperty.RotationY)
                    .AddEasingKeyFrame(0.0, 180)
                    .AddEasingKeyFrame(Duration * 0.75, 0, new QuadraticEase { EasingMode = EasingMode.EaseInOut}),
                element.AnimateProperty(Reverse ? AnimationProperty.RotationY : AnimationProperty.RotationX)
                    .AddEasingKeyFrame(Duration * 0.25, 180)
                    .AddEasingKeyFrame(Duration, 0, new QuadraticEase { EasingMode = EasingMode.EaseInOut}),
            };
        }
    }

    public class SlowRotateAnimation : AnimationDefinition
    {
        public bool Reverse { get; set; }

        public SlowRotateAnimation()
        {
            Duration = 10;
            Forever = true;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            return new Timeline[]
            {
                element.AnimateProperty(AnimationProperty.Rotation)
                    .AddEasingKeyFrame(0, Reverse ? 360 : -360)
                    .AddEasingKeyFrame(Duration, 0),
            };

        }
    }

    public class LinePulseAnimation : AnimationDefinition
    {
        public bool Reverse { get; set; }
        public double MinSize { get; set; }
        public double MaxSize { get; set; }

        public LinePulseAnimation()
        {
            Duration = 7;
            Forever = true;
            MinSize = 10;
            MaxSize = 20;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            var shape = element as Shape;
            if (shape == null) 
                return new Timeline[]{};

            var timeline = element.AnimateProperty("(Shape.StrokeThickness)");
            timeline.EnableDependentAnimation = true;
            var defaultThickness = shape.StrokeThickness;

            return new Timeline[]
            {
                timeline
                    .AddEasingKeyFrame(0.0, defaultThickness)
                    .AddEasingKeyFrame(Duration*0.25, Reverse ? MaxSize : MinSize, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration*0.75, Reverse ? MinSize : MaxSize, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration, defaultThickness),
            };
        }
    }

    public class SizePulseAnimation : AnimationDefinition
    {
        public bool Reverse { get; set; }
        public double MinScale { get; set; }
        public double MaxScale { get; set; }

        public SizePulseAnimation()
        {
            Forever = true;
            MinScale = 0.9;
            MaxScale = 1.1;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            return new Timeline[]
            {
                element.AnimateProperty(AnimationProperty.ScaleX)
                    .AddEasingKeyFrame(0.0, 1)
                    .AddEasingKeyFrame(Duration*0.25, Reverse ? MaxScale : MinScale, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration*0.75, Reverse ? MinScale : MaxScale, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration, 1),
                element.AnimateProperty(AnimationProperty.ScaleY)
                    .AddEasingKeyFrame(0.0, 1)
                    .AddEasingKeyFrame(Duration*0.25, Reverse ? MaxScale : MinScale, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration*0.75, Reverse ? MinScale : MaxScale, new QuadraticEase {EasingMode = EasingMode.EaseInOut})
                    .AddEasingKeyFrame(Duration, 1)
            };
        }
    }


    public class SlowResetAnimation : AnimationDefinition
    {
        public bool Reverse { get; set; }

        public SlowResetAnimation()
        {
            Duration = 0.3;
        }

        public override IEnumerable<Timeline> CreateAnimation(FrameworkElement element)
        {
            return new Timeline[]
            {
                element.AnimateProperty(AnimationProperty.Rotation).AddEasingKeyFrame(Duration, 0, new QuadraticEase()),
                element.AnimateProperty(AnimationProperty.RotationX).AddEasingKeyFrame(Duration, 0, new QuadraticEase()),
                element.AnimateProperty(AnimationProperty.RotationY).AddEasingKeyFrame(Duration, 0, new QuadraticEase()),
            };
            
        }
    }



}
