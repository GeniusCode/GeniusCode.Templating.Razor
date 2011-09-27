GeniusCode.Templating.Razor
=============

Powerful toolset for generating files using Razor (.cshtml) templates - outside ASP.NET.
-------------

* Completely Replaces T4 Templates
* Supports Razor as C# 'Script Language'
* Supports Multiple File Outputs
* Supports ExistingFileBehavior (AlwaysOverwrite | NeverOverwrite)
* Supports Template Partials
* Supports Loading Assemblies at runtime
* Console Version for Build Events
* API (for Strongly-typed models)

**Purposes:**

We use this extensively to generate entities, partial classes, calculation dependencies, and even create & compile new assemblies.  Most of our usage runs via the Console as Pre-Build Events.

You can generate Parent/Children files n-levels deep.  You have full control over naming of files and directories that are generated.

API provides you strongly-typed models. Console version provides Expando object for key/value pairs on a dynamic model.

**Console Usage Examples:**

Generate 
    RazorConsole --tbase "$(ProjectDir)\Templates\Entities" --obase "$(ProjectDir)\Output\Entities"
        -f Start.cshtml -o Start.g.cs

Generate Child Files Only, no Parent output (notice suspendRootOutput)
    RazorConsole --tbase "$(ProjectDir)\Templates\Entities" --obase "$(ProjectDir)\Output\Entities"
        --assemblyFolder "$(ProjectDir)\bin" --suspendRootOutput true -f Start.cshtml -o noFile
    
**Running NUnit Tests:**

Make sure you disable 'Shadow-copy' in NUnit and/or Resharper.  Otherwise our test directories under bin\Debug will not be copied to the test folder, and thus - the tests will fail.

**Simple C# Template Script:**

This includes a partial template and generates a file 'Child.g.cs'

    @inherits RazorEngine.Templating.DictionaryTemplateBase<dynamic>

    var partialModel = new { something = value };
    var childModel = new { something = value };

    // Able to append other .cshtml files to myself
    PartialTemplate("Partial.cshtml", partialModel);

    // Able to output to separate files
    ChildTemplate("Child.cshtml", childModel, "Child.g.cs");
  
**Sample Parent/Child Template:**

    @inherits RazorEngine.Templating.DictionaryTemplateBase<dynamic>
                  
    This is Parent output
    
    @{
      var collection = new List<int>() { 1, 2, 3 };
    
      foreach (var i in collection)
    	{
    		    var childTemplateName = "Child.cshtml";
    
            dynamic childModel = new System.Dynamic.ExpandoObject();
    
            var childFolder = "newChildFolder";
            var childOutputName = String.Format(@"{0}\Child{1}.out", childFolder, i);
    
            // Able to output to separate files and folders
            ChildTemplate(childTemplateName, childModel, childOutputName);
    	}
    	
    }

**Dependencies:**

* RazorEngine on CodePlex.
* gcConsole + NDesk for Console

We hope you enjoy!

Ryan & Jeremiah