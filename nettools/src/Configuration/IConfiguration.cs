using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nettools.Configuration
{

    public interface IConfiguration
    {

        event EventHandler SettingsCreated;
        
        T GetValue<T>(string path, T defaultValue = default(T));

        void SetValue(string path, object value, bool autosave = true);

        void Remove(string path, bool autosave = true);

        bool ContainsPath(string path);

        void Load();

        void Save();

    }

}
