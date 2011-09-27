using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GeniusCode.Components.Console;
using GeniusCode.Components.Console.Support;
using NLog;
using RazorEngineExtensions;

namespace RazorConsole
{
    public class Program
    {
        // NLog - Helpful to diagnose if a DLL cannot be found, etc.
        private static Logger logger;

        public static void Main(string[] args)
        {
            SetupNLog();

            var options = new RequiredValuesOptionSet();

            // Required
            var templateBaseFolderArg = options.AddRequiredVariable<string>("tbase", "{TemplateBaseFolder} which contains .cshtml Razor templates");
            var razorFilenameArg = options.AddRequiredVariable<string>("f", "{RazorFile} to parse & execute");
            var outputFilenameArg = options.AddRequiredVariable<string>("o", "{OutputFilename}");

            // Optional
            var modelKeyValuePairArgs = options.AddVariableMatrix<object>("m", "{Model} key/value pairs");
            var namespaceArgs = options.AddVariableList<string>("n", "{Namespaces} to automatically include");
            var outputBaseFolderArg = options.AddVariable<string>("obase", "{OutputBaseFolder}");
            var assemblyFileArgs = options.AddVariableList<string>("assemblyFile", "{Assembly} path to load");
            var assemblyFolderArgs = options.AddVariableList<string>("assemblyFolder", "{AssemblyFolder} to load assemblies from");
            var assemblyFolderIgnorePatternArgs = options.AddVariableList<string>("ignoreAssemblyPattern", "{RegexIgnorePattern} to ignore certain assemblies within Assembly Folders. Helpful to prevent an assembly from attempting to load multiple times.");
            var suspendRootOutput = options.AddVariable<bool>("suspendRootOutput", "Silence primary root template's output. Child templates are still outputed.");
            var rootExistingFileBehavior = options.AddVariable<int>("rootOverwriteBehavior", "ExistingFileBehavior of primary root template. 0 = ThrowException, 1 = NeverOverwrite, 3 = AlwaysOverwrite");

            var consoleManager = new ConsoleManager(options, "gcRazorConsole", "?", new string[] {
                "",
                "gcRazorConsole: 'Console-Based Razor Template Execution'",
                "",
                "Specify the Razor file to parse & execute, as well as key/value pairs to be used for @Model calls. For instance: Console parameter m:Name=Greg would transform razor @Model.Name into Greg",
                "Please find out more information at http://www.geniuscode.net", 
                ""});

            // VisualStudio-Aware Exceptions (for cast exceptions)
            consoleManager.MakeOptionExceptionsVisualStudioAware();

            var argErrors = new StringWriter();

            // Apply Arguments

            bool canProceed = consoleManager.PerformCanProceed(argErrors, args);

            if (canProceed)
            {
                logger.Info(string.Format("templateBaseFolder = {0}", templateBaseFolderArg.Value));
                logger.Info(string.Format("razorFilename = {0}", razorFilenameArg.Value));
                logger.Info(string.Format("outputFilename = {0}", outputFilenameArg.Value));
                logger.Info(string.Format("outputBaseFolder = {0}", outputBaseFolderArg.Value));
                logger.Info(string.Format("assemblyFiles = {0}", assemblyFileArgs.Values.Select(file => file + "; ")));
                logger.Info(string.Format("assemblyFolders = {0}", assemblyFolderArgs.Values.Select(folder => folder + "; ")));
                logger.Info(string.Format("assemblyFolderIgnorePatterns = {0}", assemblyFolderIgnorePatternArgs.Values.Select(pattern => pattern + "; ")));

                // Remove quotes from folder paths
                var templateBaseFolder = RemoveSurroundingQuotes(templateBaseFolderArg.Value);
                var razorFilename = RemoveSurroundingQuotes(razorFilenameArg.Value);
                var outputFilename = RemoveSurroundingQuotes(outputFilenameArg.Value);
                var outputBaseFolder = RemoveSurroundingQuotes(outputBaseFolderArg.Value);

                var assemblyFiles = assemblyFileArgs.Values.Select(file => RemoveSurroundingQuotes(file));
                var assemblyFolders = assemblyFolderArgs.Values.Select(folder => RemoveSurroundingQuotes(folder));
                var assemblyFolderIgnorePatterns = assemblyFolderIgnorePatternArgs.Values.Select(pattern => new Regex(pattern));

                // Template & Output Base Folders
                var razorArgs = new RazorExecuteArgs(templateBaseFolder, outputBaseFolder);

                // Additional Namespaces
                razorArgs.namespaces.AddRange(namespaceArgs.Values);

                // Additional Assemblies
                RazorExtensions.ApplyAssembliesToRazorArgs(razorArgs, assemblyFiles, assemblyFolders, assemblyFolderIgnorePatterns);

                // Additional Options
                razorArgs.SuspendRootOutput = suspendRootOutput.Value;
                razorArgs.RootExistingFileBehavior = (ExistingFileBehavior)rootExistingFileBehavior.Value;

                // Execute Razor
                RazorExtensions.ExecuteRazorDynamicModel(razorArgs, razorFilename, modelKeyValuePairArgs.Matrix, outputFilename);
            }
            else
            {
                // Arguments Failed
                // Build VisualStudio-Aware Exception
                var errorString = ConsoleHelper.CreateVisualStudioErrorString("gcRazorConsole", "-100", string.Format("Invalid command-line arguments. {0}",argErrors.ToString()));

                logger.Error(errorString);

                Console.Write(errorString);
            }

        }

        private static string RemoveSurroundingQuotes(string path)
        {
            return path.Replace("\"", "");
        }

        private static void SetupNLog()
        {
            // Create a Logger
            logger = LogManager.GetCurrentClassLogger();

            logger.Info("Starting razor console...");

            // Add the event handler for handling non-UI thread exceptions 
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                const string message = "Razor Console encountered unhandled exception";

                var exception = (Exception)e.ExceptionObject;
                logger.FatalException(message, exception);

                // Build VisualStudio-Aware Exception
                var errorString = ConsoleHelper.CreateVisualStudioErrorString("gcRazorConsole", "-200", string.Format("{0} : {1}",message, exception.ToString()));

                Console.WriteLine(errorString);
            };
        }

    }
}
