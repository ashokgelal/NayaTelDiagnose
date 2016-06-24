using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using nettools.Core;
using nettools.Configuration;
using nettools.Cryptography.Hashing;

namespace nettools.Plugins
{

    internal class PluginLoader
    {

        public static List<IPlugin> Plugins = new List<IPlugin>();

        public static void LoadPlugins(List<Assembly> plugins)
        {
            foreach (Assembly pluginAssembly in plugins)
                LoadPlugin(pluginAssembly);
        }
        
        public static void LoadPlugin(Assembly plugin)
        {
            if (plugin == null)
                return;

            Logger.Debug("Loading plugin-assembly \"" + Path.GetFileNameWithoutExtension(plugin.Location) + "\"...", "PluginLoader", LogMethod.FileOnly);

            Type pluginType = plugin.GetTypes().Where(t => t.GetInterfaces().Any(i => i.Name == "IPlugin")).FirstOrDefault();

            if (pluginType == null)
            {
                Logger.Error("Failed to load " + Path.GetFileNameWithoutExtension(plugin.Location) + ": Couldn't find class which implements \"IPlugin\"", LogMethod.FileOnly);
                return;
            }

            IPlugin instance = (IPlugin)Activator.CreateInstance(pluginType);
            IConfiguration config = new JsonConfiguration(Program.__applictionDirectory + "\\plugins\\config\\" + instance.Name.ComputeSHA1() + ".json");

            config.Load();

            Program.__pluginsTypes.Add(pluginType, instance);
            Program.__pluginsConfiguration.Add(instance, config);

            Plugins.Add(instance);
            foreach(IPluginCommand command in instance.PluginCommands)
            {
                CommandExecutor.LoadPluginCommand(instance, command);
            }
        }
    }

}
