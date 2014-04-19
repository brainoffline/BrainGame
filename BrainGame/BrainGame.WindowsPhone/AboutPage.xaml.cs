using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556
using Brain.Animate;
using Brain.Storage;
using Brain.Utils;
using BrainGame.Controls;
using BrainGame.DataModel;
using BrainGame.Game;
using PropertyChanged;
using RateMyApp.Helpers;

namespace BrainGame
{

    [ImplementPropertyChanged]
    public sealed partial class AboutPage
    {
        private IStorage storage = new DataStorage();
        private const string DonationProductId = "Donation";

        public AboutPage()
        {
            InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            Games = new ObservableCollection<GameData>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadData();

            Task.Run(async () =>
            {
                try
                {
#if DEBUG
                    var productLicense = CurrentAppSimulator.LicenseInformation.ProductLicenses[DonationProductId];
#else
                    var productLicense = CurrentApp.LicenseInformation.ProductLicenses[DonationProductId];
#endif
                    if (productLicense != null)
                    {
                        ApplicationData.Current.RoamingSettings.Values["HasDonated"] = productLicense.IsActive;
                        HasDonated = productLicense.IsActive;
                    }

#if DEBUG
                    var listingInformation = await CurrentAppSimulator.LoadListingInformationAsync();
#else
                    var listingInformation = await CurrentApp.LoadListingInformationAsync();
#endif
                    if (listingInformation != null)
                    {
                        var productListing = listingInformation.ProductListings[DonationProductId];
                        if (productListing != null)
                        {
                            await Execute.OnUIThread(() =>
                            {
                                DonationAmount = productListing.FormattedPrice;
                                DonationText = "Donate " + DonationAmount;
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    // Do Nothing
                }
            });
            
            AnimateHowToRepeat();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            _repeating = false;
        }

        private async void LoadData()
        {
            Games.Clear();
            var games = await GameDefinitionSource.LoadDataAsync();
            foreach (var gameDefinition in games)
            {
                var gameData = storage.Get<GameData>("GameData." + gameDefinition.UniqueId) ?? new GameData();
                gameData.Description = gameDefinition.Title;
                gameData.Rank = BinaryGame.GetRank(gameData.BestPiece);
                Games.Add(gameData);
            }
        }


        private async void ResetAllScores_Click(object sender, RoutedEventArgs e)
        {
            var answer = await MessageBox.ShowAsync(
                "Are you sure you want to clear all scores and achievements?", "Reset Scores", MessageBoxButton.OKCancel);
            if (answer != MessageBoxResult.OK) return;

            var games = await GameDefinitionSource.LoadDataAsync();
            foreach (var gameDefinition in games)
                storage.Delete("GameData." + gameDefinition.UniqueId);

            LoadData();
        }


        private async void PleaseRateButton_OnClick(object sender, RoutedEventArgs e)
        {
            StorageHelper.StoreSetting(FeedbackHelper.ReviewedKey, true, true);
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=" + App.FamilyPackageName));
        }

        private void DontLikeUsButton_OnClick(object sender, RoutedEventArgs e)
        {
            //EasyTracker.GetTracker().SendEvent("Like", "No", null, 0);

            LikeUsButton.AnimateAsync(new BounceOutDownAnimation());
            DontLikeUsButton.AnimateAsync(new BounceOutAnimation());
        }

        private async void DonateButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //EasyTracker.GetTracker().SendEvent("Like", "Donate", null, 0);

                var xml = await CurrentApp.RequestProductPurchaseAsync(DonationProductId, false);
                ApplicationData.Current.RoamingSettings.Values["HasDonated"] = true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Donation failed to process: ");
            }
        }

        private async void LikeUsButton_OnClick(object sender, RoutedEventArgs e)
        {
            //EasyTracker.GetTracker().SendEvent("Like", "Yes", null, 0);

            await Task.WhenAll(new Task[]
            {
                LikeUsButton.AnimateAsync(new TadaAnimation()),
                DontLikeUsButton.AnimateAsync(new BounceOutDownAnimation())
            });
            await LikeUsButton.AnimateAsync(new BounceOutDownAnimation());

            await Task.WhenAll(new Task[]
            {
                DonateButton.AnimateAsync(new BounceInUpAnimation()),
                PleaseRateButton.AnimateAsync(new BounceInUpAnimation {Delay = 0.1})
            });
        }

        private bool _repeating;
        private GameDefinition demoGameDefinition = new GameDefinition {Width = 3, Height = 3};
        private async void AnimateHowToRepeat()
        {
            _repeating = true;
            while (_repeating)
            {
                await AnimateHowTo();
                HowToGameCanvas.Children.Clear();
            }
        }

        private async Task AnimateHowTo()
        {
            var tileDataTopLeft = new TileData(new XY(2, 0), 2, demoGameDefinition);
            var tileTopLeft = new TileControl { TileData = tileDataTopLeft, Width = 80, Height = 80 };
            HowToGameCanvas.Children.Add(tileTopLeft);
            await tileTopLeft.MoveToAsync(0.0, new Point(0, 0));

            var tileDataMiddle = new TileData(new XY(1, 1), 2, demoGameDefinition);
            var tileMiddle = new TileControl { TileData = tileDataMiddle, Width = 80, Height = 80 };
            HowToGameCanvas.Children.Add(tileMiddle);
            await tileMiddle.MoveToAsync(0.0, new Point(0, 80));

            var tileDataMiddleRight = new TileData(new XY(2, 1), 4, demoGameDefinition);
            var tileMiddleRight = new TileControl { TileData = tileDataMiddleRight, Width = 80, Height = 80 };
            HowToGameCanvas.Children.Add(tileMiddleRight);
            await tileMiddleRight.MoveToAsync(0.0, new Point(160, 80));

            var tileDataBottom = new TileData(new XY(2, 2), 2, demoGameDefinition);
            var tileBottom = new TileControl { TileData = tileDataBottom, Width = 80, Height = 80 };
            HowToGameCanvas.Children.Add(tileBottom);
            await tileBottom.MoveToAsync(0.0, new Point(0, 160));

            var tileDataBottomRight = new TileData(new XY(2, 2), 2, demoGameDefinition);
            var tileBottomRight = new TileControl { TileData = tileDataBottomRight, Width = 80, Height = 80 };
            tileDataBottomRight.MergedFrom = tileDataBottom;
            HowToGameCanvas.Children.Add(tileBottomRight);
            await tileBottomRight.MoveToAsync(0.0, new Point(160, 160));


            await Task.Delay(2000);
            tileDataBottom.Value *= 2;
            await Task.WhenAll(new Task[]
            {
                TileMove(tileTopLeft, tileDataTopLeft.Pos.X, tileDataTopLeft.Pos.Y, false),
                TileMove(tileMiddle, tileDataMiddle.Pos.X, tileDataMiddle.Pos.Y, false),
                TileMove(tileBottom, tileDataBottom.Pos.X, tileDataBottom.Pos.Y, true),
                tileBottomRight.AnimateAsync(new BounceOutAnimation { Amplitude = 0.4, Duration = 0.2 })
            });
            await Task.Delay(1000);
            HowToGameCanvas.Children.Remove(tileBottomRight);

            tileDataTopLeft.Pos = new XY(2, 1);
            tileDataMiddle.Pos = new XY(1, 2);
            tileDataMiddleRight.Value *= 2;
            tileDataMiddleRight.Pos = new XY(2, 2);
            await Task.WhenAll(new Task[]
            {
                TileMove(tileTopLeft, tileDataTopLeft.Pos.X, tileDataTopLeft.Pos.Y, false),
                TileMove(tileMiddle, tileDataMiddle.Pos.X, tileDataMiddle.Pos.Y, false),
                TileMove(tileMiddleRight, tileDataMiddleRight.Pos.X, tileDataMiddleRight.Pos.Y, true),
                tileBottom.AnimateAsync(new BounceOutAnimation { Amplitude = 0.4, Duration = 0.2 })
            });

            await Task.Delay(1000);
        }

        private async Task TileMove(TileControl tile, int x, int y, bool hasMerged)
        {
            var width = (tile.ActualWidth <= 0) ? 80 : tile.ActualWidth;
            var height = (tile.ActualHeight <= 0) ? 80 : tile.ActualHeight;

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



        public string DonationAmount { get; set; }
        public string DonationText { get; set; }
        public bool HasDonated { get; set; }

        public ObservableCollection<GameData> Games { get; set; }

    }
}
