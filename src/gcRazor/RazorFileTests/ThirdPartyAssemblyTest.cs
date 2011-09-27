using System.Collections.Generic;
using NUnit.Framework;

namespace RazorFileTests
{
    [TestFixture]
    public class ThirdPartyAssemblyTest
    {





        [Test]
        public void Load_Third_Party_Assemblies()
        {
            // Exepcted Output Files
            const string outputFilename = "ThirdPartyAssembly.out";

            // Delete before Test
            TestHelper.DeleteOutputFile(outputFilename);

            const string templateFilename = "ThirdPartyAssembly.cshtml";

            // Empty Model
            var modelPairs = new Dictionary<string, object>();

            // Execute
            TestHelper.GenerateFilesDynamicModel(templateFilename, modelPairs);

            // Assert
            TestHelper.AssertOutputFileExists(outputFilename);
        }
    }
}
