using nettools.Core;

namespace nettools.Plugins
{

    /// <summary>
    /// Define a command which is created by plugins
    /// </summary>
    public interface IPluginCommand : ICommand
    {

        /// <summary>
        /// Defines the name of the command
        /// </summary>
        string Name
        {
            get;
        }

    }

}
