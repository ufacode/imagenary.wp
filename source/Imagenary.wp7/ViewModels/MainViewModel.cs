using System.Collections.ObjectModel;
using System.Linq;
using Imagenary.Annotations;
using Imagenary.Api;
using Imagenary.Assets.localization;
using Imagenary.Core;
using Imagenary.Views;
using Microsoft.Phone.Tasks;
using RestSharp;
using System.ComponentModel;
using System.Device.Location;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Imagenary.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainView View;
        private readonly ImagenarySettings _settings;
        private byte[] _photoData = new byte[0];
        private string _comment;
        private Visibility _photoTagVisibility;
        private Visibility _photoVisibility;
        private long _lastLoadedId = -1;
        private long _firstLoadedId = -1;

        public bool IsDataLoaded;

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                OnPropertyChanged("Comment");
            }
        }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Visibility PhotoTagVisibility
        {
            get { return _photoTagVisibility; }
            set
            {
                if (value == _photoTagVisibility) return;
                _photoTagVisibility = value;
                OnPropertyChanged("PhotoTagVisibility");
            }
        }
        public Visibility PhotoVisibility
        {
            get { return _photoVisibility; }
            set
            {
                if (value == _photoVisibility) return;
                _photoVisibility = value;
                OnPropertyChanged("PhotoVisibility");
            }
        }

        public PhotoViewModel SelectedPhoto { get; set; }
        public BitmapImage image;

        public ObservableCollection<PhotoViewModel> Photos {get; private set; }

        private readonly ImagenaryApi _api;

        public MainViewModel()
        {
            IsDataLoaded = false;

            PhotoTagVisibility = Visibility.Visible;
            PhotoVisibility = Visibility.Collapsed;
            Photos = new ObservableCollection<PhotoViewModel>();

            _settings = new ImagenarySettings();
            _api = new ImagenaryApi(_settings.Domain);

            var locator = new GeoCoordinateWatcher(GeoPositionAccuracy.High)
            {
                MovementThreshold = 20
            };

            locator.PositionChanged += locator_PositionChanged;
            locator.Start();
        }

        private void Clean()
        {
            Comment = string.Empty;
            Latitude = string.Empty;
            Longitude = string.Empty;
            
            _photoData = new byte[0];
            PhotoTagVisibility = Visibility.Visible;
            PhotoVisibility = Visibility.Collapsed;
        }

        public void ChoosePhotoTask()
        {
            var t = new PhotoChooserTask {ShowCamera = true};
            t.Completed += (sender, result) =>
                {
                    if (result.TaskResult != TaskResult.OK)
                        return;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (result.Error != null)
                            {
                                View.ShowToast(AppResources.error, result.Error.Message);
                                return;
                            }
                            using (var s = new MemoryStream())
                            {
                                result.ChosenPhoto.CopyTo(s);
                                s.Position = 0;

                                _photoData = s.ToArray();

                                s.Position = 0;
                                var image = new BitmapImage();
                                image.SetSource(s);

                                if (image.PixelHeight > image.PixelWidth)
                                {
                                    View.Photo.Height = 300;
                                }
                                else
                                {
                                    View.Photo.Width = Application.Current.Host.Content.ActualWidth;
                                }
                                
                                View.Photo.Source = image;

                                PhotoTagVisibility = Visibility.Collapsed;
                                PhotoVisibility = Visibility.Visible;
                            }
                        });
                };
            t.Show();
        }

        public void ShareTask()
        {
            View.Dispatcher.BeginInvoke(() =>
                {
                    View.ShareButton.IsEnabled = false;
                    View.ShowProgress("Sending photo...");
                });

            var c = new RestClient();
            var req = new RestRequest(string.Format("http://{0}//photos.json", _settings.Domain), Method.POST)
                {
                    AlwaysMultipartFormData = true
                };

            req.AddParameter("token", _settings.Token);
            req.AddParameter("comment", Comment);
            req.AddParameter("latitude", Latitude);
            req.AddParameter("longitude", Longitude);
            req.AddFile("photo", _photoData, "photo.jpg", "image/jpeg");

            c.ExecuteAsync<PhotoUploadResponse>(req, response =>
                {
                    if (response.Data.Status.Code == 200)
                    {
                        Clean();
                    }

                    View.Dispatcher.BeginInvoke(() =>
                        {
                            if (response.Data.Status.Code == 200)
                            {
                                View.ShareButton.IsEnabled = false;
                            }

                            View.HideProgress();
                            View.ShowToast("", response.Data.Status.Msg);
                        });

                    Debug.WriteLine(response.Content);
                });
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Comment) && _photoData.Length != 0;
        }

        public void LoadPhotosUp()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => View.ShowProgress("Loading new photos"));

            _api.Photos(@from: _lastLoadedId, direction: "up")
                .ContinueWith(response =>
                    {
                        if (response.Result.Status.Code == 200)
                        {
                            if (!response.Result.Photos.Any()) return;

                            Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                foreach (var photo in response.Result.Photos)
                                {
                                    Debug.WriteLine(photo.Id);
                                    
                                    Photos.Insert(0, new PhotoViewModel
                                        {
                                            Id = photo.Id,
                                            Author = photo.Author.Name,
                                            Comment = photo.Comment,
                                            CreatedAt = photo.created_at,
                                            Image = photo.Image.Box,
                                            FullImage = photo.Image.Full
                                        });
                                }

                                _firstLoadedId = Photos.Last().Id;
                                _lastLoadedId = Photos.First().Id;

                                Debug.WriteLine("First: {0} Last: {1}", _firstLoadedId, _lastLoadedId);
                            });
                        }
                        else
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => AuthView.ShowToast(AppResources.error, response.Result.Status.Error));
                        }
                    })
                    .ContinueWith(t =>
                        {
                            IsDataLoaded = true;
                            Deployment.Current.Dispatcher.BeginInvoke(() => View.HideProgress());
                        });

        }

        public void LoadPhotosDown()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() => View.ShowProgress("Loading new photos"));

            _api.Photos(@from: _firstLoadedId, direction: "down")
                .ContinueWith(response =>
                {
                    if (response.Result.Status.Code == 200)
                    {
                        if (!response.Result.Photos.Any()) return;

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            foreach (var photo in response.Result.Photos)
                            {
                                Debug.WriteLine(photo.Id);

                                Photos.Add(new PhotoViewModel
                                    {
                                        Id = photo.Id,
                                        Author = photo.Author.Name,
                                        Comment = photo.Comment,
                                        CreatedAt = photo.created_at,
                                        Image = photo.Image.Box,
                                        FullImage = photo.Image.Full
                                    });
                            }

                            _firstLoadedId = Photos.Last().Id;
                            _lastLoadedId = Photos.First().Id;

                            Debug.WriteLine("First: {0} Last: {1}", _firstLoadedId, _lastLoadedId);
                        });
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() => AuthView.ShowToast(AppResources.error, response.Result.Status.Error));
                    }
                })
                    .ContinueWith(t =>
                    {
                        IsDataLoaded = true;
                        Deployment.Current.Dispatcher.BeginInvoke(() => View.HideProgress());
                    });
        }

        public void Logout()
        {
            _settings.Token = "";
        }

        void locator_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (!_settings.LocationServicesGranted)
            {
                Latitude = e.Position.Location.Latitude.ToString(CultureInfo.InvariantCulture);
                Longitude = e.Position.Location.Longitude.ToString(CultureInfo.InvariantCulture);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}