using nettools.Configuration;

using System;
using System.Linq;
using System.Reflection;

namespace nettools.Plugins
{

    public abstract class Configurable
    {

        static protected IConfiguration Configuration
        {
            get
            {
                Assembly calling = Assembly.GetCallingAssembly();
                Type callingType = calling.GetTypes().Where(t => t.GetInterfaces().Any(i => i.Name == "IPlugin")).FirstOrDefault();
                IPlugin plugin = Program.__pluginsTypes[callingType];

                return Program.__pluginsConfiguration[plugin];
            }
        }

    }

}
