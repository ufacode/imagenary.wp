using System;
using System.IO.IsolatedStorage;

namespace Imagenary.Core
{
    public class SettingsBase
    {
        readonly IsolatedStorageSettings _settings;

        public void Save()
        {
            _settings.Save();
        }

        public bool AddOrUpdateValue(string key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (_settings.Contains(key))
            {
                // If the value has changed
                if (_settings[key] != value)
                {
                    // Store the new value
                    _settings[key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                _settings.Add(key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        public T GetValueOrDefault<T>(string key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (_settings.Contains(key))
            {
                value = (T)_settings[key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        public SettingsBase()
        {
            _settings = IsolatedStorageSettings.ApplicationSettings;
        }
    }
}