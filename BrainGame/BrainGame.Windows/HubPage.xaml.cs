using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Brain.Animate;
using BrainGame.Controls;
using BrainGame.DataModel;
using PropertyChanged;

namespace BrainGame
{
    public sealed partial class HubPage
    {

        public HubPage()
        {
            InitializeComponent();
            Init();

            Boxtana.Hide();
            Loaded += async (sender, args) =>
            {
                await Task.Delay(1800);
                await Boxtana.Do(BoxtanaAction.Entrance1);
            };
        }

        public ObservableCollection<GameDefinition> Games { get; set; }

        public void Init()
        {
            Games = new ObservableCollection<GameDefinition>();
            Loaded += async (sender, args) =>
            {
                if (Games.Count > 0) return;

                foreach (var game in await GameDefinitionSource.LoadDataAsync())
                    Games.Add(game);
            };
        }

        private async void GamesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var gameDefinition = ((GameDefinition)e.ClickedItem);

            await AnimationTrigger.AnimateClose();

            Frame.Navigate(typeof(GamePage), gameDefinition);
        }

        private async void Boxtana_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await Boxtana.Do(BoxtanaAction.Exit1);
            Frame.Navigate(typeof(TestBoxPage));
        }

    }
}
