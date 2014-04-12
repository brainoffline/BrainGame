using Windows.Graphics.Display;
using Windows.UI.Xaml.Navigation;

namespace BrainGame
{
    public sealed partial class HubPage
    {
        public HubPage()
        {
            InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            NavigationCacheMode = NavigationCacheMode.Required;

            Init();
        }
    }
}