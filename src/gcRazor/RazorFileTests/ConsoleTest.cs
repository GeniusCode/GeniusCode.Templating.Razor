using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RazorFileTests.Support;

namespace RazorFileTests
{
    [TestFixture]
    public class ConsoleTest : ParentChildTestBase
    {
        [Test]
        public void Console_Parent_Child_works()
        {
            var testName = "Console_Parent_Child";

            // Init Parent-Child test
            InitParentChildTest(testName);

            var templateBaseFolder = TestHelper.TemplateFolder;
            var razorFile = base.parentTemplate;
            var outputFilename = base.outputFilename;
            var outputBaseFolder = TestHelper.OutputFolder;

            var model = new Dictionary<string, object>();
            model.Add("ChildFolderName", testName);
            var modelArg = ConvertModelToConsoleArg(model);

            var args = String.Format("-tbase|\"{0}\"|-assemblyFolder|\"{5}\"|-f|{1}|-o|{2}|-obase|\"{3}\"|{4}", templateBaseFolder, razorFile, outputFilename, outputBaseFolder, modelArg, TestHelper.GetBasePath());

            var argsArray = args.Split(new[] { "|" }, StringSplitOptions.None);

            // Execute Console App
            RazorConsole.Program.Main(argsArray);

            // Assert
            AssertParentChildrenExist();
        }

        private static string ConvertModelToConsoleArg(IEnumerable<KeyValuePair<string, object>> modelDictionary)
        {
            var modelArgs = from keyPair in modelDictionary
                            select String.Format("-m:{0}={1}", keyPair.Key, keyPair.Value.ToString());

            string model = string.Empty;

            // Put into one line
            modelArgs.ToList().ForEach(arg =>
                {
                    model += arg;
                });

            return model;
        }
    }
}
