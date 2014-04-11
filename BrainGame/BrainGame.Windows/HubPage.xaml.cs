using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BrainGame.Common;
using BrainGame.DataModel;
using PropertyChanged;

namespace BrainGame
{
    [ImplementPropertyChanged]
    public sealed partial class HubPage : Page
    {
        private NavigationHelper _navigationHelper;
        public NavigationHelper NavigationHelper
        {
            get { return _navigationHelper ?? (_navigationHelper = new NavigationHelper(this)); }
        }

        public ObservableCollection<GameDefinition> Games { get; set; }

        public HubPage()
        {
            InitializeComponent();

            Games = new ObservableCollection<GameDefinition>();
            NavigationHelper.LoadState += async (sender, args) =>
            {
                foreach(var game in await GameDefinitionSource.LoadDataAsync())
                    Games.Add(game);
            };
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            NavigationHelper.OnNavigatedFrom(e);
        }

        private void GamesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var gameDefinition = ((GameDefinition) e.ClickedItem);
            Frame.Navigate(typeof (GamePage), gameDefinition);
        }
    }
}
