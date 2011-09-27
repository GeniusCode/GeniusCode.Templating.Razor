using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RazorEngine;

namespace RazorFileTests
{
    [TestFixture]
    public class RazorExtensionsTest
    {
        [Test]
        public void Type_Templates()
        {
            var razorFilename = "TypeTemplate.cshtml";
            var templateCachedName = "TypeTemplate";

            var outputFilename = "TypeTemplate.out";
            var outputFileNames = new List<string>()
            {
                outputFilename,
                "Type1.out",
                "Type2.out",
                "Type3.out"
            };
            outputFileNames.ForEach(file => TestHelper.DeleteOutputFile(file));

            var types = new List<Type>();
            types.Add(typeof(string));
            types.Add(typeof(List<int>));
            types.Add(typeof(Guid));

            // Set Razor Template Base
            //Razor.SetTemplateBase(typeof(RazorEngine.Templating.DictionaryTemplateBase));

            // Read template from File
            string razorTemplate = File.ReadAllText(Path.Combine(TestHelper.TemplateFolder, razorFilename));

            // Very important - Forces RuntimeBinder to kick in.
            // Otherwise, we get an exception during razor's compiling
            // Without this, only way to get tests to pass is to breakpoint & use propertygrid to inspect the model, which does the same thing - calls stuff so the RuntimeBinder kicks in
            //var modelPrototype = new { };
            //string dynamicString = (modelPrototype as dynamic).ToString();

            // Compile & Cache
            //Razor.Compile(razorTemplate, modelPrototype.GetType(), templateCachedName);
            Razor.Compile(razorTemplate, typeof(Type), templateCachedName);

            // Write out Files
            int resultCount = 0;
            foreach (var type in types)
            {
                // Create Model
                //dynamic model = new ExpandoObject();
                //model.Type = type;

                var model = type;

                // Execute Razor
                var output = Razor.Run(model, templateCachedName);

                // Prep for Output
                string childFilename = String.Format("Type{0}.out", resultCount);
                string outputFilePath = Path.Combine(TestHelper.OutputFolder, childFilename);

                File.WriteAllText(outputFilePath, output);

                resultCount++;
            }


        }


        [Test]
        public void Simple_Without_Model()
        {
            var outputFilename = "SimpleWithoutModel.out";
            TestHelper.DeleteOutputFile(outputFilename);

            var razorFilename = "SimpleWithoutModel.cshtml";

            TestHelper.GenerateFilesDynamicModel(razorFilename, null);

            TestHelper.AssertOutputFileExists(outputFilename);
        }

        [Test]
        public void Simple_With_Model()
        {
            var outputFilename = "SimpleWithModel.out";
            TestHelper.DeleteOutputFile(outputFilename);

            var razorFilename = "SimpleWithModel.cshtml";

            var modelPairs = new Dictionary<string, object>();
            modelPairs.Add("Name", "On Walden's Pond");
            modelPairs.Add("Price", "13.50");

            TestHelper.GenerateFilesDynamicModel(razorFilename, modelPairs);

            TestHelper.AssertOutputFileExists(outputFilename);
        }

    }
}
