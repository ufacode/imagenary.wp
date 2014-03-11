using System.ComponentModel;
using System.Windows;
using Imagenary.Annotations;
using Imagenary.Api;
using Imagenary.Assets.localization;
using Imagenary.Core;
using Imagenary.Views;

namespace Imagenary.ViewModels
{
    public class AuthViewModel : INotifyPropertyChanged
    {
        public AuthView View { get; set; }
        private string _email;
        private string _password;
        private string _domain;

        private readonly ImagenarySettings _settings;

        public AuthViewModel(AuthView authView)
        {
            View = authView;
            _settings = new ImagenarySettings();

            Email = _settings.Email;
            Domain = _settings.Domain;
        }

        public void Login()
        {
            View.Dispatcher.BeginInvoke(() =>
                {
                    View.LoginButton.IsEnabled = false;
                    View.ShowProgress();
                });

            var api = new ImagenaryApi(Domain);
            api.Login(Email, Password)
                .ContinueWith(response =>
                    {
                        if (!string.IsNullOrWhiteSpace(response.Result.User.Token))
                        {
                            _settings.Token = response.Result.User.Token;
                            _settings.Email = response.Result.User.Email;

                            View.Dispatcher.BeginInvoke(() => View.NavigateMain());
                        }
                        else
                        {
                            View.Dispatcher.BeginInvoke(() =>
                            {
                                View.LoginButton.IsEnabled = true;
                                View.HideProgress();
                                AuthView.ShowToast(AppResources.error, response.Result.Status.Error);
                            });
                        }
                    });
        }

        public void DisplayLocataionPermissionPrompt()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    var result = MessageBox.Show(AppResources.locationusetext,
                                                 AppResources.gpsusetitle,
                                                 MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        App.Settings.LocationServicesGranted = true;
                    }
                });
        }

        public string Email
        {
            get { return _email; }
            set
            {
                if (value == _email) return;
                _email = value;
                OnPropertyChanged("Email");
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                if (value == _password) return;
                _password = value;
                OnPropertyChanged("Password");
            }
        }
        public string Domain
        {
            get { return _domain; }
            set
            {
                if (value == _domain) return;
                _domain = value;

                OnPropertyChanged("Domain");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Domain)
                    && !string.IsNullOrWhiteSpace(Email)
                    && !string.IsNullOrWhiteSpace(Password);
        }
    }
}