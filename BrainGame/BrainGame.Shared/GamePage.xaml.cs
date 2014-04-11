#if WINDOWS_APP

// Windows phone not ready yet
#define GOOGLE_ANALYTICS

using GoogleAnalytics;
#endif

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Brain.Animate;
using Brain.Storage;
using Brain.Utils;
using BrainGame.Controls;
using BrainGame.Game;
using PropertyChanged;


namespace BrainGame
{
    [ImplementPropertyChanged]
    public partial class GamePage 
    {
        private const int TileWidth = 70;
        private const int TileHeight = 70;

        private const int GameWidth = 3;
        private const int GameHeight = 3;

        public GamePage()
        {
            InitializeComponent();

            ShowShare = false;

            Game1 = new BinaryGame(GameWidth, GameHeight);
            Game1.TileMoved += GameOnTileMoved;
            Game1.GameWon += GameOnGameWon;
            Game1.GameOver += GameOnGameOver;

            bc.Game = Game1;
            bc.Build();

            DonationText = "Donate";
        }

        public int TotalScore { get; set; }
        public bool AllGamesOver { get; set; }

        private void GameOnTileMoved(object sender, TileData tileData)
        {
            TotalScore = bc.Game.Score;
            if (TotalScore > GameData.BestScore)
                GameData.BestScore = TotalScore;

            if (tileData.Value > GameData.BestPiece)
                Achievement(tileData.Value);
        }

        public void Achievement(int value)
        {
            GameData.BestPiece = value;
            SaveData();

            if (GameData.BestPiece > 8)
            {
                Rank = GetRank(GameData.BestPiece);
                ShowAchievement(GetRank(value), "NEW ACHIEVEMENT", true);
            }
        }

        private string GetRank(int value)
        {
            switch (value)
            {
                default:
                    return "Unranked";
                case 16: return "Beginner";
                case 32: return "Rookie";
                case 64: return "Novice";
                case 128: return "Amateur";
                case 256: return "Skilled";
                case 512: return "Profesional";
                case 1024: return "Expert";
                case 2048: return "Ninja";
            }
        }

        private async void ShowAchievement(string content, string title = null, bool showTrophy = false)
        {
            ShowTrophy = showTrophy;
            AchievementTitle = title;// ?? "NEW ACHIEVEMENT";
            AchievementText = content;

            await AchievementBalloon.AnimateAsync(new BounceInUpAnimation());

            await Task.Delay(3000);

            await AchievementBalloon.AnimateAsync(new BounceOutDownAnimation());
        }

        private void GameOnGameWon(object sender, EventArgs eventArgs)
        {
            //GameMessage = "You ROCK!";
        }

        private void GameOnGameOver(object sender, EventArgs eventArgs)
        {
            AllGamesOver = (bc.Game.IsGameOver);
            if (!AllGamesOver) return;

            ShowAchievement("No more moves", "GAME OVER");

#if GOOGLE_ANALYTICS
            EasyTracker.GetTracker().SendEvent("GameOver", "Score", null, TotalScore);
            EasyTracker.GetTracker().SendEvent("GameOver", "Swipes", null, swipes);
            if (TotalScore == GameData.BestScore)
                EasyTracker.GetTracker().SendEvent("BestScore", "BestScore", null, GameData.BestScore);
#endif

            ShowShare = true;
            PlayGrid.IsHitTestVisible = true;
            PlayGrid.AnimateAsync(new FadeInAnimation());
            PlayButton.IsHitTestVisible = true;
            PlayButton.AnimateAsync(new BounceInUpAnimation());

            SaveData();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (TotalScore == 0)
                LoadData();
            Rank = GetRank(GameData.BestPiece);

#if GOOGLE_ANALYTICS
            EasyTracker.GetTracker().SendView("Main");
#endif

            if (TotalScore == 0)
            {
                PlayGrid.AnimateAsync(new FadeInAnimation { Delay = 1.8 });
                PlayButton.AnimateAsync(new BounceInUpAnimation { Delay = 1.8 });

                PlayGrid.IsHitTestVisible = true;
                PlayButton.IsHitTestVisible = true;
            }
            else
            {
                PlayGrid.Opacity = 0;
                PlayGrid.IsHitTestVisible = false;
                PlayButton.Opacity = 0;
                PlayButton.IsHitTestVisible = false;
            }

            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Window.Current.CoreWindow.KeyUp -= CoreWindow_KeyUp;
            SaveData();

            base.OnNavigatedFrom(e);
        }

        private void LoadData()
        {
            GameData = storage.GetSettings<GameData>("GameData") ?? new GameData();
            Game1.GameData = GameData;

            var savedGame = storage.GetSettings<SavedGame>("SavedGame");
            if (savedGame != null)
            {
                Game1.Score = savedGame.Score1;

                for (int x = 0; x < GameWidth; x++)
                {
                    for (int y = 0; y < GameHeight; y++)
                    {
                        Game1.RestoreTile(savedGame.BinaryGrid1.Tiles[x, y]);
                    }
                }

                TotalScore = bc.Game.Score;

                //SimpleSettings.DeleteSetting("SavedGame");
            }
        }

        private void SaveData()
        {
            storage.SaveSettings(GameData, "GameData");

            if (!AllGamesOver && TotalScore > 0)
            {
                var savedGame = new SavedGame
                {
                    Score1 = Game1.Score,
                    BinaryGrid1 = Game1.BinaryGrid,
                };
                storage.SaveSettings(savedGame, "SavedGame");
            }
            else
                storage.DeleteSetting("SavedGame");
        }

        private async Task TileMove(TileControl tile, int x, int y, bool hasMerged)
        {
            var width = (tile.ActualWidth <= 0) ? TileWidth : tile.ActualWidth;
            var height = (tile.ActualHeight <= 0) ? TileHeight : tile.ActualHeight;

            if (hasMerged)
            {
                await Task.WhenAll(new Task[]
                {
                    tile.MoveToAsync(0.3, new Point(x*width, y*height), new BackEase {Amplitude = 0.4}),
                    tile.AnimateAsync(new PulseAnimation {Duration = 0.4})
                });
            }
            else
                await tile.MoveToAsync(0.3, new Point(x * width, y * height), new BackEase { Amplitude = 0.4 });
        }



        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (AllGamesOver) return;

            var x = e.Cumulative.Translation.X;
            var y = e.Cumulative.Translation.Y;

            if (Math.Abs(x) > Math.Abs(y))
            {
                if (x < -20)
                    Move(MoveType.Left);
                else if (x > 20)
                    Move(MoveType.Right);
            }
            else
            {
                if (y < -20)
                    Move(MoveType.Up);
                else if (y > 20)
                    Move(MoveType.Down);
            }
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs e)
        {
            switch (e.VirtualKey)
            {
                case VirtualKey.Left: Move(MoveType.Left); break;
                case VirtualKey.Right: Move(MoveType.Right); break;
                case VirtualKey.Up: Move(MoveType.Up); break;
                case VirtualKey.Down: Move(MoveType.Down); break;
            }
        }

        private void Move(MoveType moveType)
        {
            swipes++;
            Game1.Move(moveType);
        }


        public BinaryGame Game1 { get; set; }
        public GameData GameData { get; set; }


        //public string GameMessage { get; set; }
        public bool ShowShare { get; set; }
        private int swipes;
        //public bool IsPaused { get; set; }

        public bool ShowTrophy { get; set; }
        public string AchievementTitle { get; set; }
        public string AchievementText { get; set; }

        public string DonationAmount { get; set; }
        public string DonationText { get; set; }

        public string Rank { get; set; }

        private ISimpleStorage storage = new SimpleStorage();

        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            var message = string.Format("OMG! I just scored {0} in Binary Squared. (Rank: {1}) #BrainOffline #BinarySquared",
                TotalScore, Rank);

            await MessageBox.ShowAsync("TODO: Share link");
            /*
            var task = new ShareLinkTask
            {
                Title = "Binary",
                Message = message,
                LinkUri = new Uri("http://www.windowsphone.com/s?appid=" + CurrentApp.AppId)
            };
            task.Show();
             */
        }


        private async void PlayButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            await PlayGrid.AnimateAsync(new FadeOutAnimation());
            PlayGrid.IsHitTestVisible = false;

            /*
            if (IsPaused)
            {
                IsPaused = false;
            }
            else
             */
            {
                //GameMessage = "Swipe any direction to move tiles";
                //ShowAchievement("Swipe in any direction to move tiles");
                swipes = 0;

#if GOOGLE_ANALYTICS
                EasyTracker.GetTracker().SendEvent("GameStart", "Start", null, 0);
#endif

                await bc.Clear();
                Game1.Setup();

                TotalScore = 0;
                AllGamesOver = false;
            }
        }

    }
}