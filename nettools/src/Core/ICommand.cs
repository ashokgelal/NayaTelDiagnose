using System.Collections.Generic;

namespace nettools.Core
{

    public interface ICommand
    {

        bool Execute(Dictionary<string, string> arguments);

        void Stop();

        string[] GetAvailableCommands();

        string[] GetAvailableArguments();
        
        string GetHelp();

        string GetArgumentHelp(string arg);

    }

}
