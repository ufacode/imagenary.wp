using System.Windows;
using Imagenary.Assets.localization;
using Imagenary.ViewModels;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Imagenary.Views
{
    public partial class AboutView : PhoneApplicationPage
    {
        private ITransition _pageTransition;

        public AboutView()
        {
            InitializeComponent();

            BuildPageTransition();
            DataContext = new AboutViewViewModel();

            LocationSwitch.Tap += LocationSwitch_Tap;
        }

        private void BuildPageTransition()
        {
            var transition = new SlideTransition
                {
                    Mode = SlideTransitionMode.SlideLeftFadeIn
                };

            _pageTransition = transition.GetTransition(LayoutRoot);

            _pageTransition.Completed += (sender, args) => _pageTransition.Stop();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _pageTransition.Begin();
            base.OnNavigatedTo(e);
        }

        async void LocationSwitch_Tap(object sender, GestureEventArgs gestureEventArgs)
        {
            if (LocationSwitch.IsChecked ?? false)
            {
                var result = MessageBox.Show(AppResources.locationusetext,
                                             AppResources.gpsusetitle,
                                             MessageBoxButton.OKCancel);

                App.Settings.LocationServicesGranted = result == MessageBoxResult.OK;
                LocationSwitch.Content = (bool) LocationSwitch.IsChecked ? "On " : "Off";
            }
            else
            {
                App.Settings.LocationServicesGranted = false;
                LocationSwitch.Content = "Off";
            }
        }
    }
}