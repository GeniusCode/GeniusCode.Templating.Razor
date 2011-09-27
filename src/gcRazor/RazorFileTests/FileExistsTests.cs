using System;
using System.IO;
using NUnit.Framework;
using RazorEngineExtensions;
using RazorFileTestModels;

namespace RazorFileTests
{
    [TestFixture]
    public class FileExistsTests
    {
        [Test]
        public void Respects_Existing_File_Behavior()
        {
            var parentTemplateName = "FileExistsParent.cshtml";
            var parentFilename = "FileExistsParent.out";
            var childFilename = "FileExistsChild.out";

            TestHelper.DeleteOutputFile(parentFilename);
            TestHelper.DeleteOutputFile(childFilename);

            Action<ExistingFileBehavior, int> WriteFile = (behavior, number) =>
                {
                    var model = new ExistingFileBehaviorModel()
                    {
                        Behavior = behavior,
                        ChildNumber = number
                    };

                    // Generate
                    TestHelper.GenerateFiles(parentTemplateName, parentFilename, model, (razorArgs) =>
                        {
                            razorArgs.RootExistingFileBehavior = ExistingFileBehavior.AlwaysOverwrite;
                        });
                };

            Action<int> AssertChildFileContains = (number) =>
            {
                var filePath = Path.Combine(TestHelper.OutputFolder, childFilename);
                var fileText = File.ReadAllText(filePath);
                Assert.IsTrue(fileText.Contains(number.ToString()));
            };

            // Create File
            WriteFile(ExistingFileBehavior.ThrowException, 123);
            AssertChildFileContains(123);

            // Never Overwrite
            WriteFile(ExistingFileBehavior.NeverOverwrite, 456);
            AssertChildFileContains(123);

            // Overwrite File
            WriteFile(ExistingFileBehavior.AlwaysOverwrite, 456);
            AssertChildFileContains(456);

            // Make sure we blow up
            bool threwException = false;
            try
            {
                WriteFile(ExistingFileBehavior.ThrowException, 789);
            }
            catch (Exception ex)
            {
                threwException = true;
            }
            Assert.IsTrue(threwException);

        }
    }
}
