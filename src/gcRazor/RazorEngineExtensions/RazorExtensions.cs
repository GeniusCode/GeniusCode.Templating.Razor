using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RazorEngine.Templating;

namespace RazorEngineExtensions
{
    public static class RazorExtensions
    {

        public static void ExecuteRazor<T>(string templatesFolder, string razorFilename, T model, string outputFilename, string outputFolder, IEnumerable<string> assembliesToLoad, IEnumerable<string> assemblyFolders, IEnumerable<Regex> assemblyFolderIgnorePatterns)
        {
            var args = new RazorExecuteArgs(templatesFolder, outputFolder);

            ApplyAssembliesToRazorArgs(args, assembliesToLoad, assemblyFolders, assemblyFolderIgnorePatterns);

            ExecuteRazor<T>(args, razorFilename, model, outputFilename);
        }

        public static void ExecuteRazor<T>(string templatesFolder, string razorFilename, T model, string outputFilename, string outputFolder, IEnumerable<string> assembliesToLoad)
        {
            var args = new RazorExecuteArgs(templatesFolder, outputFolder);

            ApplyAssembliesToRazorArgs(args, assembliesToLoad, null, null);

            ExecuteRazor<T>(args, razorFilename, model, outputFilename);
        }

        public static void ExecuteRazor<T>(string templatesFolder, string razorFilename, T model, string outputFilename, string outputFolder)
        {
            var args = new RazorExecuteArgs(templatesFolder, outputFolder);

            ExecuteRazor<T>(args, razorFilename, model, outputFilename);
        }

        /// <summary>
        /// Primary Execute Razor Template method.
        /// </summary>
        public static void ExecuteRazor<T>(RazorExecuteArgs razorArgs, string razorFilename, T model, string outputFilename)
        {
            ApplyRazorExecuteArgs(razorArgs);

            var service = razorArgs.service;
            var templateFolder = razorArgs.resolver.BaseTemplateFolder;

            //  **** BUG FIX for runtime binder ****
            // - Forces RuntimeBinder to kick in.
            // - Otherwise, we get an exception during razor's compiling
            // Without this, only way to get tests to pass is to breakpoint & use propertygrid to inspect the model, which does the same thing - calls stuff so the RuntimeBinder kicks in
            if (model is dynamic)
            {
                string dynamicString = (model as dynamic).ToString();
            }
            // ***** END BUG FIX *******

            // Read Razor Template
            string razorFilePath = Path.Combine(templateFolder, razorFilename);
            string razor = File.ReadAllText(razorFilePath);

            IDictionaryTemplate templateResult;

            // Execute Razor into a Dictionary
            service.ParseDictionary(razor, model, out templateResult);

            // Write to Files
            WriteOutputToFiles(razorArgs, templateResult, razorFilename, outputFilename);
        }



        #region Dynamic Model

        public static void ExecuteRazorDynamicModel(string templatesFolder, string razorFilename, IDictionary<string, object> modelKeyValuePairs, string outputFilename, string outputFolder, IEnumerable<string> assembliesToLoad, IEnumerable<string> assemblyFolders, IEnumerable<Regex> assemblyFolderIgnorePatterns)
        {
            var args = new RazorExecuteArgs(templatesFolder, outputFolder);

            ApplyAssembliesToRazorArgs(args, assembliesToLoad, assemblyFolders, assemblyFolderIgnorePatterns);

            ExecuteRazorDynamicModel(args, razorFilename, modelKeyValuePairs, outputFilename);
        }

        public static void ExecuteRazorDynamicModel(string templatesFolder, string razorFilename, IDictionary<string, object> modelKeyValuePairs, string outputFilename, string outputFolder, IEnumerable<string> assembliesToLoad)
        {
            var args = new RazorExecuteArgs(templatesFolder, outputFolder);

            ApplyAssembliesToRazorArgs(args, assembliesToLoad, null, null);

            ExecuteRazorDynamicModel(args, razorFilename, modelKeyValuePairs, outputFilename);
        }

        public static void ExecuteRazorDynamicModel(string templatesFolder, string razorFilename, IDictionary<string, object> modelKeyValuePairs, string outputFilename, string outputFolder)
        {
            var args = new RazorExecuteArgs(templatesFolder, outputFolder);

            ExecuteRazorDynamicModel(args, razorFilename, modelKeyValuePairs, outputFilename);
        }

        public static void ExecuteRazorDynamicModel(RazorExecuteArgs args, string razorFilename, IDictionary<string, object> modelKeyValuePairs, string outputFilename)
        {
            // Create Model
            var model = new ExpandoObject();
            if (modelKeyValuePairs != null)
            {
                foreach (var keyPair in modelKeyValuePairs)
                    (model as IDictionary<string, Object>).Add(keyPair);
            }

            ExecuteRazor<dynamic>(args, razorFilename, model, outputFilename);
        }
        #endregion


        private static void ApplyRazorExecuteArgs(RazorExecuteArgs args)
        {
            var service = args.service;
            var namespaces = args.namespaces;
            var resolver = args.resolver;
            var assemblies = args.AssemblyFilesToLoad;

            // Set Default Razor Template Base
            // Strong-typing support is located in CompilationServiceBase.BuildTypeName() method
            // This WILL be overrided if the template specifies its own base class type
            service.SetTemplateBase(typeof(RazorEngine.Templating.DictionaryTemplateBase));

            // Namespaces
            if (namespaces != null)
            {
                foreach (var nspace in namespaces)
                    service.Namespaces.Add(nspace);
            }

            // Resolve Templates by Filename
            if (resolver != null)
                service.AddResolver(resolver);

            // Load Assemblies
            assemblies.ForEach(assemblyInfo => Assembly.LoadFrom(assemblyInfo.FullName));

            //// Compile Named Templates (enables @Include)
            //if (namedTemplates != null)
            //{
            //    foreach (var keyPair in namedTemplates)
            //    {
            //        string includeTemplate = File.ReadAllText(keyPair.Value);
            //        // will use dynamic (callsite binding), as model type is unknown
            //        service.CompileWithAnonymous(includeTemplate, keyPair.Key);
            //    }
            //}
        }

        private static void WriteOutputToFiles(RazorExecuteArgs razorArgs, IDictionaryTemplate templateResult, string razorFilename, string outputFilename)
        {
            string outputBaseFolder = razorArgs.BaseOutputFolder;

            // Output Folder
            // If no output path, use same folder as razor template
            if (String.IsNullOrEmpty(outputBaseFolder))
                outputBaseFolder = Path.GetDirectoryName(razorFilename);

            // Write out Files
            int resultCount = 0;
            foreach (var keyPair in templateResult.ResultDictionary)
            {
                string outputFilepath;
                string outputName = keyPair.Key;
                string rawOutput = keyPair.Value;

                bool isRootTemplate = false;

                if (resultCount == 0)
                {
                    // Root Template
                    outputFilepath = Path.Combine(outputBaseFolder, outputFilename);
                    isRootTemplate = true;
                }
                else
                    // Child Template
                    outputFilepath = Path.Combine(outputBaseFolder, outputName);

                // Create Directory, if it doesn't exist
                var folder = Path.GetDirectoryName(outputFilepath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                // Existing File Behavior
                var existingFileBehavior = GetExistingFileBehavior(razorArgs, templateResult, outputName, isRootTemplate);

                // If Should, Write File
                if (ShouldWriteFile(razorArgs, outputName, outputFilepath, isRootTemplate, existingFileBehavior))
                    File.WriteAllText(outputFilepath, rawOutput);

                resultCount++;
            }
        }

        private static ExistingFileBehavior GetExistingFileBehavior(RazorExecuteArgs razorArgs, IDictionaryTemplate template, string outputName, bool isRootTemplate)
        {
            // Default Behavior
            ExistingFileBehavior overwriteBehavior = ExistingFileBehavior.ThrowException;

            // Root Template
            if (isRootTemplate)
                overwriteBehavior = razorArgs.RootExistingFileBehavior;
            else
            {
                var fileSystemTemplate = template as IFileSystemTemplate;
                if (fileSystemTemplate != null)
                    fileSystemTemplate.ExistingFileBehaviors.TryGetValue(outputName, out overwriteBehavior);

            }

            return overwriteBehavior;
        }

        private static bool ShouldWriteFile(RazorExecuteArgs razorArgs, string outputName, string outputFilePath, bool isRootTemplate, ExistingFileBehavior overwriteBehavior)
        {
            // Ignore this File?
            if (isRootTemplate && razorArgs.SuspendRootOutput)
                return false;

            // File doesn't exist - Go for it
            bool fileExists = File.Exists(outputFilePath);
            if (!fileExists) return true;

            // File exists, Decide what to do
            switch (overwriteBehavior)
            {
                case ExistingFileBehavior.AlwaysOverwrite:
                    return true;

                case ExistingFileBehavior.NeverOverwrite:
                    return false;

                default:
                case ExistingFileBehavior.ThrowException:
                    throw new Exception(string.Format("File exists, please specify ExistingFileBehavior to overwite or ignore existing file: {0}", outputFilePath));
            }
        }

        public static void ApplyAssembliesToRazorArgs(RazorExecuteArgs razorArgs, IEnumerable<string> assembliesToLoad, IEnumerable<string> assemblyFolders, IEnumerable<Regex> assemblyFolderIgnorePatterns)
        {
            assemblyFolders = assemblyFolders ?? new List<string> { };
            assembliesToLoad = assembliesToLoad ?? new List<string> { };
            assemblyFolderIgnorePatterns = assemblyFolderIgnorePatterns ?? new List<Regex> { };

            // Assembly Folders
            var folderAssemblies = from folder in assemblyFolders
                                   from filePath in Directory.EnumerateFiles(folder, "*.dll", SearchOption.TopDirectoryOnly)
                                   let filename = Path.GetFileName(filePath)
                                   let ignore = assemblyFolderIgnorePatterns.Any(regex => regex.IsMatch(filename))
                                   where ignore == false
                                   select new FileInfo(filePath);

            // Assembly Files
            var fileAssemblies = from filePath in assembliesToLoad
                                 select new FileInfo(filePath);

            razorArgs.AssemblyFilesToLoad.AddRange(folderAssemblies);
            razorArgs.AssemblyFilesToLoad.AddRange(fileAssemblies);

        }
    }
}
