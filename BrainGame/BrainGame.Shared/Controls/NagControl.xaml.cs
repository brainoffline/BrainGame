using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using Brain.Animate;
using Brain.Utils;

namespace BrainGame.Controls
{
    public sealed partial class NagControl : UserControl
    {
        private const string DonationProductId = "Donation";

        public NagControl()
        {
            this.InitializeComponent();

            DonateButton.Visibility = Visibility.Collapsed;

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

                                DonateButton.Visibility = Visibility.Collapsed;
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    // Do Nothing
                }
            });
        }

        private async void LikeUsButton_OnClick(object sender, RoutedEventArgs e)
        {
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

        private async void PleaseRateButton_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: Rate Us
            await MessageBox.ShowAsync("TODO: Rate us");

        }

        private void DontLikeUsButton_OnClick(object sender, RoutedEventArgs e)
        {
            LikeUsButton.AnimateAsync(new BounceOutDownAnimation());
            DontLikeUsButton.AnimateAsync(new BounceOutAnimation());
        }

        public string DonationAmount { get; set; }
        public string DonationText { get; set; }
        public bool HasDonated { get; set; }

    }
}
