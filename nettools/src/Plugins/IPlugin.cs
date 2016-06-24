using System;

namespace nettools.Plugins
{

    /// <summary>
    /// Base interface for a plugin with multiple commands
    /// </summary>
    public interface IPlugin
    {

        /// <summary>
        /// The commands of the plugin which should be loaded into the application
        /// </summary>
        IPluginCommand[] PluginCommands
        {
            get;
        }

        /// <summary>
        /// The name of this plugin
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// The author of this plugin, e.g. your (nick/real)name
        /// </summary>
        string Author
        {
            get;
        }

        /// <summary>
        /// A short/long description of your plugin
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// The current Version of this plugin
        /// </summary>
        Version Version
        {
            get;
        }

    }

}
