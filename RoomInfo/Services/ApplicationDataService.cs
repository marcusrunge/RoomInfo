﻿using Windows.Storage;

namespace RoomInfo.Services
{
    public interface IApplicationDataService
    {
        T GetSetting<T>(string key);
        void SaveSetting(string key, object value);
    }
    public class ApplicationDataService : IApplicationDataService
    {
        ApplicationDataContainer _localSettings;
        public ApplicationDataService()
        {
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        /// <summary>    
        /// Retrieves application settings
        /// </summary>   
        /// <param name="key">Settings Key</param>  
        /// <returns>Application Settings of type T</returns>
        public T GetSetting<T>(string key)
        {
            if (_localSettings.Values.ContainsKey(key))
            {
                var value = _localSettings.Values[key];
                if (value.GetType() == typeof(T)) return (T)value;
            }
            return default(T);
        }

        /// <summary>    
        /// Saves application settings 
        /// </summary>   
        /// <param name="key">Settings Key</param>  
        /// <param name="value">Settings Value</param>
        public void SaveSetting(string key, object value) => _localSettings.Values[key] = value;
    }
}
