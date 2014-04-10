#if WINDOWS_PHONE
using System.Windows.Threading;
#endif
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
#if NETFX_CORE

#endif
#if ANDROID
#endif
#if PORTABLE
#endif


namespace Brain.Utils
{

    public static class Execute
    {
#if NETFX_CORE
        public static async Task OnUIThread(Action action)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () => action());
        }

        public static void DelayAction(TimeSpan delay, Action action)
        {
            new Task(
                async () =>
                    {
                        await Task.Delay(delay);
                        await OnUIThread(action);
                    }).Start();

        }
#endif

#if WINDOWS_PHONE
        public static void OnUIThread(Action action)
        {
            Deployment.Current.Dispatcher.BeginInvoke(action);
        }

        public static void DelayAction(TimeSpan delay, Action action)
        {
            new Thread(() =>
                           {
                               Thread.Sleep(delay);
                               OnUIThread(action);
                           }).Start();
        }
#endif

    }


}
