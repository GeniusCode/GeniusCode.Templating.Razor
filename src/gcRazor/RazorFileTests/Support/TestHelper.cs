using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using RazorEngineExtensions;

namespace RazorFileTests
{
    public static class TestHelper
    {
        public static readonly string TemplateFolder = Path.Combine(GetBasePath(), @"Templates\");
        public static readonly string OutputFolder = Path.Combine(GetBasePath(), @"RazorOutput\");
        public static readonly string AssemblyFolder = Path.Combine(GetBasePath(), @"TemplateAssemblies\");
        public static readonly List<Regex> AssemblyFolderIgnorePatterns;

        static TestHelper()
        {
            AssemblyFolderIgnorePatterns = new List<Regex>()
            {
                new Regex("Razor.*dll"),
                new Regex("gcCore.dll")
            };
        }

        public static void GenerateFiles<T>(string razorFilename, T model)
        {
            var outputFilename = Path.ChangeExtension(razorFilename, "out");

            GenerateFiles<T>(razorFilename, outputFilename, model);
        }

        public static void GenerateFiles<T>(string razorFilename, string outputFilename, T model)
        {
            GenerateFiles<T>(razorFilename, outputFilename, model, null);
        }

        public static void GenerateFiles<T>(string razorFilename, string outputFilename, T model, Action<RazorExecuteArgs> argsAction)
        {
            var razorArgs = GetRazorExecuteArgs();

            if (argsAction != null)
                argsAction(razorArgs);

            RazorExtensions.ExecuteRazor<T>(razorArgs, razorFilename, model, outputFilename);
        }

        public static RazorExecuteArgs GetRazorExecuteArgs()
        {
            var razorArgs = new RazorExecuteArgs(TemplateFolder, OutputFolder);

            // Assemblies
            RazorExtensions.ApplyAssembliesToRazorArgs(razorArgs, null, new List<string> { AssemblyFolder }, AssemblyFolderIgnorePatterns);

            return razorArgs;
        }


        public static void GenerateFilesDynamicModel(string razorFilename, IDictionary<string, object> modelKeyValuePairs)
        {
            var outputFilename = Path.ChangeExtension(razorFilename, "out");

            GenerateFilesDynamicModel(razorFilename, outputFilename, modelKeyValuePairs);
        }

        public static void GenerateFilesDynamicModel(string razorFilename, string outputFilename, IDictionary<string, object> modelKeyValuePairs)
        {
            RazorExtensions.ExecuteRazorDynamicModel(TemplateFolder, razorFilename, modelKeyValuePairs, outputFilename, OutputFolder, null, new List<string> { AssemblyFolder }, AssemblyFolderIgnorePatterns);
        }

        public static void DeleteOutputFile(string filename)
        {
            string fullPath = GetFullPath(filename);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public static void AssertOutputFileExists(string filename)
        {
            string fullPath = GetFullPath(filename);
            Assert.IsTrue(File.Exists(fullPath));
        }

        private static string GetFullPath(string filename)
        {
            var basePath = GetBasePath();
            return Path.Combine(basePath, OutputFolder, filename);
        }

        public static string GetBasePath()
        {
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return basePath;
        }



    }
}
