namespace nettools.Plugins
{

    internal enum PluginLanguage
    {

        /// <summary>
        /// Plugin which is written in C# (.cs)
        /// </summary>
        CSharp = 0,

        /// <summary>
        /// Plugin which is written in VB.NET (.vb)
        /// </summary>
        VBNet = 1,

        /// <summary>
        /// A already compiled plugin (.dll)
        /// </summary>
        Compiled = 2,

        /// <summary>
        /// Plugin is written in an unknown language (maybe wrong file-extension)
        /// </summary>
        NotSupported = 3

    }

}
