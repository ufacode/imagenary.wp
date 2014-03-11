using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Imagenary.Annotations;
using Imagenary.Core;
using Microsoft.Phone.Tasks;

namespace Imagenary.ViewModels
{
    public class AboutViewViewModel : INotifyPropertyChanged
    {
        public bool LocationServicesGranted
        {
            get { return App.Settings.LocationServicesGranted; }
            set
            {
                if (value.Equals(App.Settings.LocationServicesGranted)) return;

                App.Settings.LocationServicesGranted = value;
                OnPropertyChanged();
            }
        }

        public AboutViewViewModel()
        {
            RateAndReviewCommand = new DelegateCommand(_ =>
                {
                    var task = new MarketplaceReviewTask();
                    task.Show();
                });

            SendFeedbackCommand = new DelegateCommand(_ =>
                {
                    var task = new EmailComposeTask
                        {
                            To = "info@aomega.co",
                            Subject = "Imagenary feedback."
                        };
                    task.Show();
                });

            MoreAppsCommand = new DelegateCommand(_ =>
                {
                    var task = new MarketplaceSearchTask
                        {
                            SearchTerms = "iBrainGamer"
                        };
                    task.Show();
                });

            TwitterCommand = new DelegateCommand(_ =>
                {
                    var task = new WebBrowserTask
                        {
                            Uri = new Uri("http://twitter.com/AlexandrYZ")
                        };
                    task.Show();
                });

            OpenSiteCommand = new DelegateCommand(_ =>
                {
                    var task = new WebBrowserTask
                        {
                            Uri = new Uri("http://aomega.co")
                        };
                    task.Show();
                });
            
            Version = "v" + GetVersion();
        }

        public string Version { get; set; }

        public ICommand RateAndReviewCommand { get; set; }
        public ICommand SendFeedbackCommand { get; set; }
        public ICommand MoreAppsCommand { get; set; }
        public ICommand TwitterCommand { get; set; }
        public ICommand OpenSiteCommand { get; set; }

        private static string GetVersion()
        {
            var assemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            return assemblyName.Version.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}