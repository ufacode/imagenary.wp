using System.Windows.Media.Imaging;
using Coding4Fun.Toolkit.Storage;
using Imagenary.ViewModels;
using Microsoft.Phone.Controls;

namespace Imagenary.Views
{
    public partial class PhotoView : PhoneApplicationPage
    {
        private ITransition _pageTransition;
        public static PhotoViewModel Model { get; set; }

        public PhotoView()
        {
            InitializeComponent();
            BuildPageTransition();

            Model = Serializer.Open<PhotoViewModel>("photo.view");
            using (var s = PlatformFileAccess.GetOpenFileStream("photo.jpg"))
            {
                var image = new BitmapImage();
                image.SetSource(s);

                PostPhoto.Source = image;
            }

            DataContext = Model;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _pageTransition.Begin();

            base.OnNavigatedFrom(e);
        }

        private void BuildPageTransition()
        {
            var te = new SlideTransition
            {
                Mode = SlideTransitionMode.SlideLeftFadeIn
            };

            _pageTransition = te.GetTransition(LayoutRoot);

            _pageTransition.Completed += (sender, args) => _pageTransition.Stop();
        }
    }
}