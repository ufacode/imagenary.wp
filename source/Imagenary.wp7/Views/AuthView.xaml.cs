using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using Coding4Fun.Toolkit.Controls;
using Imagenary.Assets.localization;
using Imagenary.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Imagenary.Views
{
    public partial class AuthView : PhoneApplicationPage
    {
        private ITransition _transition;
        private ITransition _leftTransition;
        public ApplicationBarIconButton LoginButton;
        public AuthViewModel Model { get; set; }
        public AuthView()
        {
            InitializeComponent();

            BuildAppBar();
            BuildTransition();

            Model = new AuthViewModel(this);
            Model.PropertyChanged += (sender, args) =>
                {
                    LoginButton.IsEnabled = Model.IsValid();
                };

            DataContext = Model;
        }

        private void BuildTransition()
        {
            var tr = new SlideTransition
                {
                    Mode = SlideTransitionMode.SlideRightFadeIn
                };

            _transition = tr.GetTransition(LayoutRoot);

            _transition.Completed += (sender, args) => _transition.Stop();

            var trLeft = new SlideTransition
                {
                    Mode = SlideTransitionMode.SlideLeftFadeIn
                };
            _leftTransition = trLeft.GetTransition(LayoutRoot);

            _leftTransition.Completed += (sender, args) => _leftTransition.Stop();
        }

        private void BuildAppBar()
        {
            ApplicationBar = new ApplicationBar();

            LoginButton = new ApplicationBarIconButton(new Uri("/Assets/images/check.png", UriKind.Relative))
                {
                    Text = AppResources.login,
                    IsEnabled = false
                };

            LoginButton.Click += (sender, args) => Model.Login();

            ApplicationBar.Buttons.Add(LoginButton);

            var about = new ApplicationBarMenuItem(AppResources.about);
            about.Click +=
                (sender, args) => NavigationService.Navigate(new Uri("/Views/AboutView.xaml", UriKind.Relative));

            ApplicationBar.MenuItems.Add(about);
        }

        public static void ShowToast(string title, string message)
        {
            var toast = new ToastPrompt
            {
                Title = title,
                Message = message
            };
            toast.Show();
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            // Update the binding source
            BindingExpression bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            bindingExpr.UpdateSource();
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var textBox = sender as PasswordBox;
            // Update the binding source
            BindingExpression bindingExpr = textBox.GetBindingExpression(PasswordBox.PasswordProperty);
            bindingExpr.UpdateSource();
        }

        public void ShowProgress()
        {
            ProgressIndicator.IsIndeterminate = true;
            ProgressIndicator.IsVisible = true;
        }

        public void HideProgress()
        {
            ProgressIndicator.IsIndeterminate = false;
            ProgressIndicator.IsVisible = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("logout"))
            {
                NavigationService.RemoveBackEntry();
            }

            if (e.NavigationMode == NavigationMode.Back)
            {
                _transition.Begin();
            }
            else
            {
                _leftTransition.Begin();
            }
            

            if (App.Settings.FirstStart)
            {
                App.Settings.FirstStart = false;
                Model.DisplayLocataionPermissionPrompt();
            }

            base.OnNavigatedTo(e);
        }

        public void NavigateMain()
        {
            NavigationService.Navigate(new Uri("/Views/MainView.xaml?login=true", UriKind.Relative));
        }
    }
}