using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CSharp;
using Microsoft.VisualBasic;

using nettools.Core;
using nettools.Cryptography.Hashing;

namespace nettools.Plugins
{
    
    internal class PluginCompiler
    {
        
        public static List<Assembly> CompilePlugins(FileInfo[] codeFiles)
        {
            List<Assembly> list = new List<Assembly>();

            foreach (FileInfo codeFile in codeFiles)
                list.Add(CompilePlugin(codeFile));

            return list;
        }

        public static Assembly CompilePlugin(FileInfo codeFile)
        {
            PluginLanguage lang = GetPluginLanguage(codeFile);

            if (lang == PluginLanguage.CSharp)
                return CompileCSharpPlugin(codeFile);
            else if (lang == PluginLanguage.VBNet)
                return CompileVBNetPlugin(codeFile);
            else if (lang == PluginLanguage.Compiled)
                return Assembly.LoadFile(codeFile.FullName);
            else
                return null;
        }

        private static Assembly CompileVBNetPlugin(FileInfo codeFile)
        {
            return CompileDotNetPlugin(codeFile, new VBCodeProvider());
        }

        private static Assembly CompileCSharpPlugin(FileInfo codeFile)
        {
            return CompileDotNetPlugin(codeFile, new CSharpCodeProvider());
        }

        private static Assembly CompileDotNetPlugin(FileInfo codeFile, CodeDomProvider provider)
        {
            FileInfo compiledFile = new FileInfo(Program.__applictionDirectory + "\\plugins\\compiled\\" + Path.GetFileNameWithoutExtension(codeFile.Name).ComputeSHA1() + ".ntext");

            CodeDomProvider codeProvider = provider;
            CompilerParameters parameters = new CompilerParameters();

            parameters.GenerateExecutable = true;
            parameters.OutputAssembly = compiledFile.FullName;
            parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            parameters.GenerateExecutable = false;

            string code = "";

            using (StreamReader codeFileReader = codeFile.OpenText())
                code = codeFileReader.ReadToEnd();

            if(PluginDescription.HasDescription(code))
            {
                Dictionary<string, string> preprocess = PluginDescription.GetDescription(code);

                string[] codeLines = code.Split('\n');

                for(int i = 0; i < PluginDescription.GetLineOfDescriptionEnding(code); i++)
                {
                    codeLines[i] = "// PlgnDescr\t-->\t" + codeLines[i];
                }

                code = string.Join("\n", codeLines);

                if (preprocess.ContainsKey("libraries"))
                {
                    foreach (string lib in preprocess["libraries"].Split(','))
                    {
                        string libName = lib.Trim(' ', ';', ',');
                        string libPath = Program.__applictionDirectory + "\\plugins\\libraries\\" + (libName.EndsWith(".dll") ? libName : libName + ".dll");

                        if (File.Exists(libPath))
                            parameters.ReferencedAssemblies.Add(libPath);
                        else
                            Logger.Error("Failed to find referenced assembly for \"" + Path.GetFileNameWithoutExtension(codeFile.Name) + "\": " +
                                (libName.EndsWith(".dll") ? libName : libName + ".dll"), "PluginCompiler", LogMethod.FileOnly);
                    }
                }
            }

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, code);
            code = "";

            if (results.Errors.Count != 0)
            {
                string errors = "";
                foreach (CompilerError error in results.Errors)
                {
                    errors += "\t\t" + error.ErrorNumber + ": " + error.ErrorText + " (in " + error.FileName + " on Line " + error.Line + ", Column " + error.Column + ")\n";
                }

                Logger.Error("Failed to compile " + codeFile.Name + ": \n" + errors, LogMethod.FileOnly);
                return null;
            }
            else
            {
                return results.CompiledAssembly;
            }
        }

        private static PluginLanguage GetPluginLanguage(FileInfo path)
        {
            if (path.Extension == ".cs")
                return PluginLanguage.CSharp;
            else if (path.Extension == ".vb")
                return PluginLanguage.VBNet;
            else if (path.Extension == ".dll")
                return PluginLanguage.Compiled;
            else
                return PluginLanguage.NotSupported;
        }

    }

}
