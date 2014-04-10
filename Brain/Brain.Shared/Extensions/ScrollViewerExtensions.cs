using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#if NETFX_CORE

#endif

#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Controls;
#endif

namespace Brain.Extensions
{
    public static class ScrollViewerExtensions
    {
        public static void ScrollTo(this ScrollViewer scrollViewer, FrameworkElement element)
        {
            var transform = element.TransformToVisual(scrollViewer);

#if NETFX_CORE
            var position = transform.TransformPoint(new Point(0, 0));
#else
            var position = transform.Transform(new Point(0, 0));
#endif
            scrollViewer.ScrollToVerticalOffset(position.Y);
        }
    }
}
