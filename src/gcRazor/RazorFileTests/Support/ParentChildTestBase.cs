using System;
using System.Collections.Generic;
using NUnit.Framework;
using RazorFileTestModels;

namespace RazorFileTests.Support
{
    [TestFixture]
    public abstract class ParentChildTestBase
    {
        protected string parentTemplate;
        protected string outputFilename;
        protected List<string> outputFileNames;

        public void InitParentChildTest(string testName)
        {
            // Exepcted Output Files
            outputFilename = testName + ".out";
            outputFileNames = new List<string>()
            {
                outputFilename,
                String.Format(@"{0}\Child1.out", testName),
                String.Format(@"{0}\Child2.out", testName),
                String.Format(@"{0}\Child3.out", testName)
            };

            // Delete before Test
            outputFileNames.ForEach(file => TestHelper.DeleteOutputFile(file));

            parentTemplate = "Parent.cshtml";

            // Execute Test

        }

        public void ExecuteParentChildTest(ParentModel model)
        {
            // Execute
            TestHelper.GenerateFiles(parentTemplate, outputFilename, model);
        }

        public void AssertParentChildrenExist()
        {
            // Assert
            this.outputFileNames.ForEach(file => TestHelper.AssertOutputFileExists(file));
        }

    }

}
