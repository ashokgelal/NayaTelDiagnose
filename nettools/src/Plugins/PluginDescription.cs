using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace nettools.Plugins
{

    public class PluginDescription
    {
        
        public static Dictionary<string, string> GetDescription(string source)
        {
            string descr = ParseDescription(source);
            Dictionary<string, string> values = InterpretDescription(descr);

            descr = "";

            return values;
        }

        public static int GetLineOfDescriptionEnding(string pluginSource)
        {
            var lines = pluginSource.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                foreach (Match m in Regex.Matches(lines[i], "\\|>>"))
                    return i + 1;
            }
            return -1;
        }

        public static bool HasDescription(string pluginSource)
        {
            Match match = Regex.Match(pluginSource, "<<\\|(.*)\\|>>", RegexOptions.Singleline);
            return match.Success;
        }

        public static string ParseDescription(string pluginSource)
        {
            Match match = Regex.Match(pluginSource, "<<\\|(.*)\\|>>", RegexOptions.Singleline);

            if (match.Success)
                return match.Groups[1].Value;
            else
                return null;
        }

        public static Dictionary<string, string> InterpretDescription(string description)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            MatchCollection matches = Regex.Matches(description, "([a-zA-Z].*):(.*);", RegexOptions.Singleline);

            foreach(Match match in matches)
                values.Add(match.Groups[1].Value, match.Groups[2].Value.Trim('\t', '\r', ' ', '\0'));

            matches = null;
            return values;
        }

    }

}
