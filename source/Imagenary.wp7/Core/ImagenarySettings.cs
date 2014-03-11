namespace Imagenary.Core
{
    public class ImagenarySettings :SettingsBase
    {
        public string Email
        {
            get { return GetValueOrDefault("email", ""); }
            set { if (AddOrUpdateValue("email", value)) Save();}
        }

        public string Domain
        {
            get { return GetValueOrDefault("domain", "imonno.ru"); }
            set { if (AddOrUpdateValue("domain", value)) Save(); }
        }

        public string Token
        {
            get { return GetValueOrDefault("token", ""); }
            set { if (AddOrUpdateValue("token", value)) Save();}
        }

        public bool FirstStart
        {
            get { return GetValueOrDefault("firststart", true); }
            set { if (AddOrUpdateValue("firststart", value)) Save(); }
        }

        public bool LocationServicesGranted
        {
            get { return GetValueOrDefault("locationservicesgranted", false); }
            set { if (AddOrUpdateValue("locationservicesgranted", value)) Save(); }
        }
    }
}