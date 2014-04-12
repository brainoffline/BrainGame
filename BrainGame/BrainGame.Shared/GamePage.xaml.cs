#if WINDOWS_APP

// Windows phone not ready yet
#define GOOGLE_ANALYTICS
using GoogleAnalytics;
#endif

using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Brain.Animate;
using Brain.Utils;
using BrainGame.DataModel;
using BrainGame.Game;
using PropertyChanged;


namespace BrainGame
{
    [ImplementPropertyChanged]
    public partial class GamePage 
    {
        public GamePage()
        {
            InitializeComponent();

#if WINDOWS_APP
            SizeChanged += (sender, args) =>
            {
                if (pageRoot.ActualWidth < 500)
                    BackButton.Style = Application.Current.Resources["BackButtonSmallStyle"] as Style;
                else
                    BackButton.Style = Application.Current.Resources["BackButtonNormalStyle"] as Style;
            };
#endif
        }

        private void GameOnTileMoved(object sender, TileData tileData)
        {
            TotalScore = bc.Game.Score;
            BestScore = bc.Game.GameData.BestScore;
        }

        private void GameOnNewAchievement(object sender, int value)
        {
            if (value > 8)
            {
                Rank = BinaryGame.GetRank(bc.Game.GameData.BestPiece);
                ShowAchievement(Rank, "NEW ACHIEVEMENT", true);
            }
        }

        private void GameOnNewHighScore(object sender, int score)
        {
            BestScore = score;
        }


        private async void ShowAchievement(string content, string title = null, bool showTrophy = false)
        {
            ShowTrophy = showTrophy;
            AchievementTitle = title;
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
            EasyTracker.GetTracker().SendEvent("GameOver", "Moves", null, bc.Game.Moves);
            if (TotalScore == bc.Game.GameData.BestScore)
                EasyTracker.GetTracker().SendEvent("BestScore", "BestScore", null, bc.Game.GameData.BestScore);
#endif

            ShowShare = true;
            PlayGrid.IsHitTestVisible = true;
            PlayGrid.AnimateAsync(new FadeInAnimation());
            PlayButton.IsHitTestVisible = true;
            PlayButton.AnimateAsync(new BounceInUpAnimation());

            bc.SaveData();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (bc.Game == null)
            {
                var gameDefinition = (GameDefinition) e.Parameter;
                if (gameDefinition == null) return;

                bc.Build(gameDefinition);

                bc.Game.TileMoved += GameOnTileMoved;
                bc.NewHighScore += GameOnNewHighScore;
                bc.NewAchievement += GameOnNewAchievement;
                bc.Game.GameWon += GameOnGameWon;
                bc.Game.GameOver += GameOnGameOver;
            }

            if (TotalScore == 0)
            {
                bc.LoadData();
                TotalScore = bc.Game.Score;
                BestScore = bc.Game.GameData.BestScore;
                Rank = BinaryGame.GetRank(bc.Game.GameData.BestPiece);
            }
            Rank = BinaryGame.GetRank(bc.Game.GameData.BestPiece);

#if GOOGLE_ANALYTICS
            EasyTracker.GetTracker().SendView("Main");
#endif

            if (TotalScore == 0)
            {
                PlayGrid.AnimateAsync(new FadeInAnimation { Delay = 0.3 });
                PlayButton.AnimateAsync(new BounceInUpAnimation { Delay = 0.4 });

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
            bc.SaveData();

            base.OnNavigatedFrom(e);
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
            bc.Game.Move(moveType);
        }



        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            var message = string.Format("OMG! I just scored {0} in BrainGame. (Rank: {1}) #BrainOffline #BrainGame",
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

#if GOOGLE_ANALYTICS
            EasyTracker.GetTracker().SendEvent("GameStart", "Start", null, 0);
#endif

            await bc.Clear();
            bc.Game.Setup();

            TotalScore = 0;
            AllGamesOver = false;
        }

        private void RestartButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PlayButton_Click(sender, null);
        }


        public int TotalScore { get; set; }
        public int BestScore { get; set; }

        public bool AllGamesOver { get; set; }
        public bool ShowShare { get; set; }

        public bool ShowTrophy { get; set; }
        public string AchievementTitle { get; set; }
        public string AchievementText { get; set; }

        public string Rank { get; set; }

    }
}