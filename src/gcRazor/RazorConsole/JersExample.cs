using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GeniusCode.Framework.Console;
using GeniusCode.Framework.Extensions;
using GeniusCode.Framework.Support.Console;
using GeniusCode.Framework.Support.Files;
using GeniusCode.Tools.RemoteFileSync.Contracts;
using C = System.Console;

namespace FTPFindAndReplaceConsole
{

    public class myConsoleManager : ConsoleManager
    {

        public myConsoleManager(RequiredValuesOptionSet os, string consoleName, string helpFlag, string[] text, TextWriter writer)
            : base(os, consoleName, helpFlag, text)
        {
            Writer = writer;
        }


        private TextWriter Writer;

        public void WriteHeader(string contents)
        {
            Writer.WriteLine();
            Writer.WriteLine();
            Writer.WriteLine("*************************************");
            Writer.WriteLine(contents.ToUpper());
            Writer.WriteLine("*************************************");
            Writer.WriteLine();
        }

        public void WriteStatus(string message)
        {
            Writer.WriteLine(message);
        }

        public bool PerformCanProceed(string[] args)
        {
            return base.PerformCanProceed(Writer, args);
        }


        public void RenderProgressBar(int maxValue, double intProgress, int intLeftPos, int intTopPos)
        {
            string strResult = null;
            double percent = 0;

            if ((maxValue > 0))
            {
                percent = Math.Round((intProgress / maxValue) * 100, 0);
                if (percent >= 10)
                {
                    strResult = String.Format("| {0}% |{1}{2}|", percent,
                        new string('=', (int)percent / 10),
                        new string(' ', 10 - ((int)percent / 10)));
                }
                else
                {
                    strResult = String.Format("| {0}% |{1}|", percent, new string(' ', 10));
                }
            }
            else
            {
                percent = intProgress;
                strResult = "| 0% |";
            }
            C.CursorVisible = false;
            C.SetCursorPosition(intLeftPos, intTopPos);
            C.Write(strResult);
            C.CursorVisible = true;
        }

    }


    public class Program
    {

        public static void Main(string[] args)
        {

            var os = new RequiredValuesOptionSet();

            var user = os.AddRequiredVariable<string>("u", "{Username} required for login");
            var password = os.AddRequiredVariable<string>("p", "{Password} required for login");
            var server = os.AddRequiredVariable<string>("s", "{Server} name (no paths)");
            var path = os.AddRequiredVariable<string>("f", "Filename to server {file} to search");

            var trial = os.AddFlag("test", "Performs a trial operation withing modifying any files on the server");
            var patterns = os.AddVariableList<string>("frp", "Find and Replace {Pattern}.Seperate with a double pipe ||.");

            var m = new myConsoleManager(os, "gcRFR", "?", new string[] {
                "",
                "gcRFR: 'Remote Find and Replace'",
                "",
                "Specify the file on the server you would like to perform the find & replace on.",
                "The change can be previewed as well.",
                "Please find out more information at http://www.geniuscode.net", 
                ""}
                , Console.Out);

            m.Requirements.Add(new Requirement() { RequirementSatisfied = () => patterns.Values.Any(), Text = new string[] { "Please specify at least one pattern" } });

            if (!m.PerformCanProceed(args)) return;


            // get an adapter using MEF
            var cat = new DirectoryCatalog(Paths.AssemblyDirectory);
            var con = new CompositionContainer(cat);

            m.WriteStatus("Initializing remote connection");
            IRemoteFileAdapter adapter = con.GetExportedValue<IRemoteFileAdapter>();

            string fileContents;
            string tempFileName;

            m.WriteStatus("Getting information about remote");
            tempFileName = GetRemoteFileContents(user, password, server, path, adapter, out fileContents);
            m.WriteStatus("Applying Find & Replace");
            fileContents = ApplyRegexReplace(patterns, fileContents);
            m.WriteStatus("Saving changes");
            CommitValueToServer(path, server, password, user, adapter, fileContents, tempFileName);
            m.WriteStatus("Complete");
        }

        private static string GetRemoteFileContents(Variable<string> user, Variable<string> password, Variable<string> server, Variable<string> path, IRemoteFileAdapter adapter, out string value)
        {
            string filename;
            filename = Path.GetTempFileName();

            adapter.Connect(server, user, password);
            adapter.Get(filename, path);
            adapter.Disconnect();

            using (var r = new StreamReader(filename))
            {
                value = r.ReadToEnd();
                r.Close();
            }
            return filename;
        }

        private static void CommitValueToServer(Variable<string> path, string server, string password, string user, IRemoteFileAdapter adapter, string fileContents, string tempFileName)
        {
            using (var w = new StreamWriter(tempFileName, false))
            {
                w.Write(fileContents);
                w.Flush();
                w.Close();
            }
            adapter.Connect(server, user, password);
            adapter.Push(tempFileName, path);
            adapter.Disconnect();
        }

        private static string ApplyRegexReplace(VariableList<string> patterns, string fileContents)
        {
            patterns.Values.ToList().ForEach(l =>
            {
                var items = l.Split("||".WrapObjInNewArray(), StringSplitOptions.RemoveEmptyEntries);
                if (items.Count() != 2)
                {
                    throw new Exception("Improper values detected");
                }
                fileContents = Regex.Replace(fileContents, items[0], items[1]);
            });
            return fileContents;
        }
    }
}
