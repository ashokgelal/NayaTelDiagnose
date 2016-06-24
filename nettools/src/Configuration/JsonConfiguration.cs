using System;
using System.Collections.Generic;
using System.IO;

using nettools.ThirdParty.Newtonsoft.Json;
using nettools.ThirdParty.Newtonsoft.Json.Linq;

namespace nettools.Configuration
{

    public sealed class JsonConfiguration : IConfiguration
    {

        public event EventHandler SettingsCreated;

        private string _path;
        private JsonSerializerSettings _jsonSerializerSettings;

        private string _currentJson;
        private Dictionary<string, object> _currentSettings;

        public JsonConfiguration(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            this._path = path;
            this._jsonSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };        
        }

        public JsonConfiguration(string path, JsonSerializerSettings serializerSettings)
        {
            this._path = path;
            this._jsonSerializerSettings = serializerSettings;
        }

        public void Load()
        {
            if (!File.Exists(this._path))
            {
                File.WriteAllText(this._path, "{}");
                InitalizeJson();

                EventHandler tmpHandler = SettingsCreated;
                if (tmpHandler != null)
                    tmpHandler(this, EventArgs.Empty);
            }
            else
                InitalizeJson();
        }

        private void InitalizeJson()
        {
            this._currentJson = File.ReadAllText(this._path);
            this._currentSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(this._currentJson, this._jsonSerializerSettings);
        }

        public T GetValue<T>(string path, T defaultValue = default(T))
        {
            if (!_currentSettings.ContainsKey(path))
            {
                this.SetValue(path, defaultValue);
                return defaultValue;
            }

            object data = _currentSettings[path];

            if (data is T)
            {
                return (T)data;
            }
            else if (data is JArray)
            {
                return JArray.Parse(JsonConvert.SerializeObject(data)).ToObject<T>();
            }
            else
            {
                try
                {
                    return (T)Convert.ChangeType(data, typeof(T));
                }
                catch (InvalidCastException)
                {
                    return defaultValue;
                }
            }
        }
        
        public void SetValue(string path, object value, bool autosave = true)
        {
            if (_currentSettings.ContainsKey(path))
                _currentSettings.Remove(path);

            _currentSettings.Add(path, value);
            
            if (autosave)
                this.Save();
        }

        public void Remove(string path, bool autosave = true)
        {
            if (_currentSettings.ContainsKey(path))
                _currentSettings.Remove(path);
            
            if (autosave)
                this.Save();
        }

        public bool ContainsPath(string path)
        {
            return _currentSettings.ContainsKey(path);
        }

        public void Save()
        {
            this._currentJson = JsonConvert.SerializeObject(_currentSettings, this._jsonSerializerSettings);
            File.WriteAllText(this._path, this._currentJson);
        }

    }

}
