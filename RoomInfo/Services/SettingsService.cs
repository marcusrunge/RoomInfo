using System.Collections.Generic;
using Windows.Storage;

namespace RoomInfo.Services
{
    public enum SettingStrategy { Local, Roaming}
    public interface ISettingsService
    {
        T GetSetting<T>(string key, SettingStrategy settingStrategy);
        void SaveSetting(string key, object value, SettingStrategy settingStrategy);
        void RemoveSetting(string key, SettingStrategy settingStrategy);
    }

    public class SettingsService : ISettingsService
    {
        ApplicationDataContainer _localSettings;
        ApplicationDataContainer _roamingSettings;

        public SettingsService()
        {
            _localSettings =  ApplicationData.Current.LocalSettings;
            _roamingSettings = ApplicationData.Current.RoamingSettings;
        }
        /// <summary>    
        /// Retrieves application settings of types bool, float, int, long, ICollection<string> and string
        /// </summary>   
        /// <param name="key">Setting Key</param>
        /// <param name="settingStrategy">Setting Strategy</param>
        /// <returns>Application Settings of types bool, float, int, long, ICollection<string> or string</returns> 
        public T GetSetting<T>(string key, SettingStrategy settingStrategy)
        {
            switch (settingStrategy)
            {
                case SettingStrategy.Local:
                    if (typeof(T) == typeof(bool)) return (T)(object)_localSettings.Values[key];
                    else if (typeof(T) == typeof(float)) return (T)(object)_localSettings.Values[key];
                    else if (typeof(T) == typeof(int)) return (T)(object)_localSettings.Values[key];
                    else if (typeof(T) == typeof(long)) return (T)(object)_localSettings.Values[key];
                    else if (typeof(T) == typeof(ICollection<string>)) return (T)(object)_localSettings.Values[key];
                    else return (T)(object)_localSettings.Values[key];
                case SettingStrategy.Roaming:
                    if (typeof(T) == typeof(bool)) return (T)(object)_roamingSettings.Values[key];
                    else if (typeof(T) == typeof(float)) return (T)(object)_roamingSettings.Values[key];
                    else if (typeof(T) == typeof(int)) return (T)(object)_roamingSettings.Values[key];
                    else if (typeof(T) == typeof(long)) return (T)(object)_roamingSettings.Values[key];
                    else if (typeof(T) == typeof(ICollection<string>)) return (T)(object)_roamingSettings.Values[key];
                    else return (T)(object)_roamingSettings.Values[key];                
                default:
                    return default;
            }
        }

        /// <summary>    
        /// Saves application settings of types bool, float, int, long, ICollection<string> and string 
        /// </summary>   
        /// <param name="key">Settings Key</param>  
        /// <param name="value">Settings Value</param>
        /// <param name="settingStrategy">Setting Strategy</param>
        public void SaveSetting(string key, object value, SettingStrategy settingStrategy)
        {
            switch (settingStrategy)
            {
                case SettingStrategy.Local:
                    _localSettings.Values[key] = value;
                    break;
                case SettingStrategy.Roaming:
                    _roamingSettings.Values[key] = value;
                    break;                
                default:
                    break;
            }
        }

        /// <summary>    
        /// Removes application settings
        /// </summary>   
        /// <param name="key">Settings Key</param>  
        /// <param name="settingStrategy">Setting Strategy</param>
        public void RemoveSetting(string key, SettingStrategy settingStrategy)
        {
            switch (settingStrategy)
            {
                case SettingStrategy.Local:
                    _localSettings.Values.Remove(key);
                    break;
                case SettingStrategy.Roaming:
                    _roamingSettings.Values.Remove(key);
                    break;
                default:
                    break;
            }
        }
    }
}
