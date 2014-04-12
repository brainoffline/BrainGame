using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using BrainGame.DataModel;
using PropertyChanged;

namespace BrainGame
{
    [ImplementPropertyChanged]
    public partial class HubPage
    {
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

        private void GamesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var gameDefinition = ((GameDefinition)e.ClickedItem);
            Frame.Navigate(typeof(GamePage), gameDefinition);
        }
    }
}
