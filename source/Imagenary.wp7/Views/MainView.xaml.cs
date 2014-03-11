using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Coding4Fun.Toolkit.Controls;
using Coding4Fun.Toolkit.Storage;
using Imagenary.Assets.localization;
using Imagenary.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Imagenary.Views
{
    public partial class MainView
    {
        public ApplicationBarIconButton ShareButton;
        private MainViewModel viewModel;
        
        private ApplicationBar _shareAppBar;
        private ApplicationBar _photosAppBar;

        private ScrollViewer _sv;

        private bool _scrolledToTop;
        private bool _scrolledToBottom;
        
        private bool _alreadyHookedScrollEvents;
        private ITransition _pageTransition;

        public MainView()
        {
            InitializeComponent();

            viewModel = (MainViewModel) Resources["viewModel"];
            viewModel.View = this;

            BuildAppBar();
            BuildPageTransition();

            viewModel.PropertyChanged += (sender, args) => ShareButton.IsEnabled = viewModel.IsValid();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!viewModel.IsDataLoaded)
            {
                viewModel.LoadPhotosDown();
            }

            if (_alreadyHookedScrollEvents)
                return;

            _alreadyHookedScrollEvents = true;
            Photos.AddHandler(
                ManipulationCompletedEvent,
                (EventHandler<ManipulationCompletedEventArgs>)ListBox_ManipulationCompleted, true);

            _sv = (ScrollViewer)FindElementRecursive(Photos, typeof(ScrollViewer));

            if (_sv != null)
            {
                // Visual States are always on the first child of the control template 
                var element = VisualTreeHelper.GetChild(_sv, 0) as FrameworkElement;
                if (element != null)
                {
                    VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression");
                    if (vgroup != null)
                    {
                        vgroup.CurrentStateChanging += vgroup_CurrentStateChanging;
                    }
                }
            }

        }

        private void BuildAppBar()
        {
            _shareAppBar = new ApplicationBar();
            ShareButton = new ApplicationBarIconButton(new Uri("/Assets/images/check.png", UriKind.Relative))
                {
                    Text = "share",
                    IsEnabled = false
                };

            ShareButton.Click += (sender, args) =>
                {
                    viewModel.ShareTask();
                    Focus();
                };

            _shareAppBar.Buttons.Add(ShareButton);

            var about = new ApplicationBarMenuItem(AppResources.about);
            about.Click +=
                (sender, args) => NavigationService.Navigate(new Uri("/Views/AboutView.xaml", UriKind.Relative));

            var logout = new ApplicationBarMenuItem("logout");
            logout.Click += (sender, args) => Logout();

            _shareAppBar.MenuItems.Add(logout);
            _shareAppBar.MenuItems.Add(about);

            _photosAppBar = new ApplicationBar();
            _photosAppBar.MenuItems.Add(logout);
            _photosAppBar.MenuItems.Add(about);
        }

        public void HideProgress()
        {
            ProgressIndicator.IsIndeterminate = false;
            ProgressIndicator.IsVisible = false;
        }

        public void ShowProgress(string message)
        {
            ProgressIndicator.IsVisible = true;
            ProgressIndicator.IsIndeterminate = true;
            ProgressIndicator.Text = message;
        }

        private void AddPhotoTap(object sender, GestureEventArgs e)
        {
            viewModel.ChoosePhotoTask();
        }

        public void ShowToast(string title, string message)
        {
            var toast = new ToastPrompt
            {
                Title = title,
                Message = message,
                MillisecondsUntilHidden = 5000
            };
            toast.Show();
        }

        private void CommentChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            // Update the binding source
            var bindingExpr = textBox.GetBindingExpression(TextBox.TextProperty);
            bindingExpr.UpdateSource();
        }

        private void Logout()
        {
            viewModel.Logout();
            NavigationService.Navigate(new Uri("/Views/AuthView.xaml?logout=true", UriKind.Relative));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (NavigationContext.QueryString.ContainsKey("login"))
            {
                // prevent to return back to Auth page
                NavigationService.RemoveBackEntry();
            }

            if (e.NavigationMode == NavigationMode.Back)
            {
                _pageTransition.Begin();
            }

            base.OnNavigatedTo(e);
        }

        private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivotItem = e.AddedItems[0] as PivotItem;
            
            if (pivotItem == null) return;

            if (pivotItem.Header.Equals("share"))
            {
                ApplicationBar = _shareAppBar;
                ApplicationBar.Mode = ApplicationBarMode.Default;
            }
            else
            {
                ApplicationBar = _photosAppBar;
                ApplicationBar.Mode = ApplicationBarMode.Minimized;
            }
            
        }

        private void Photos_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0
                || !viewModel.IsDataLoaded) return;

            var post = e.AddedItems[0] as PhotoViewModel;
            
            if (post == null) return;

            viewModel.SelectedPhoto = post;

            ShowProgress("Loading image...");
            
            viewModel.IsDataLoaded = false;
            viewModel.image = new BitmapImage(new Uri("http://" + App.Settings.Domain + post.FullImage))
                {
                    CreateOptions = BitmapCreateOptions.IgnoreImageCache
                };
            viewModel.image.DownloadProgress += ImageDownloadProgress;
        }

        private void ImageDownloadProgress(object sender, DownloadProgressEventArgs args)
        {
            if (args.Progress < 100) return;

            Photos.SelectedIndex = -1;

            viewModel.image.DownloadProgress -= ImageDownloadProgress;

            viewModel.IsDataLoaded = true;
            HideProgress();

            Serializer.Save("photo.view", viewModel.SelectedPhoto);

            using (var s = PlatformFileAccess.GetSaveFileStream("photo.jpg"))
            {
                var bmp = new WriteableBitmap(viewModel.image);
                bmp.SaveJpeg(s, viewModel.image.PixelWidth, viewModel.image.PixelHeight, 0, 100);
            }
            
            NavigationService.Navigate(new Uri("/Views/PhotoView.xaml", UriKind.Relative));
        }

        private void ListBox_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_scrolledToTop)
            {
                _scrolledToTop = false;
                viewModel.LoadPhotosUp();
            }

            if (_scrolledToBottom)
            {
                _scrolledToBottom = false;
                viewModel.LoadPhotosDown();
            }
        }

        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionTop")
            {
                _scrolledToBottom = false;
                _scrolledToTop = true;
            }

            if (e.NewState.Name == "CompressionBottom")
            {
                _scrolledToTop = false;
                _scrolledToBottom = true;
            }
        }

        private void BuildPageTransition()
        {
            var te = new SlideTransition
            {
                Mode = SlideTransitionMode.SlideRightFadeIn
            };

            _pageTransition = te.GetTransition(LayoutRoot);

            _pageTransition.Completed += (sender, args) => _pageTransition.Stop();
        }

        private UIElement FindElementRecursive(FrameworkElement parent, Type targetType)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            UIElement returnElement = null;
            if (childCount > 0)
            {
                for (var i = 0; i < childCount; i++)
                {
                    Object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == targetType)
                    {
                        return element as UIElement;
                    }

                    returnElement = FindElementRecursive(VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType);
                }
            }
            return returnElement;
        }
        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }
    }
}