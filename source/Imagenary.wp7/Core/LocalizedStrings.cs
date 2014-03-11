using Imagenary.Assets.localization;

namespace Imagenary.Core
{
    public class LocalizedStrings
    {
        public LocalizedStrings() { }

        private static AppResources localizedresources = new AppResources();

        public AppResources AppResources { get { return localizedresources; } }
    }
}