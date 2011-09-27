# GeniusCode - Razor Templating #

### Bye, Bye T4 -- Hello Razor.###

## Powerful toolset for generating files using Razor Templates (.cshtml) - outside ASP.NET. ##

* Supports Razor as C# 'Script Language'
* Supports Multiple File Outputs
* Supports ExistingFileBehavior (AlwaysOverwrite | NeverOverwrite)
* Supports Template Partials
* Supports Loading Assemblies at runtime
* Console Version for Build Events
* API (for Strongly-typed models)

**Purposes:**

We use this extensively to generate entities, partial classes, class libraries, calculation dependencies, and even create & compile new assemblies.  Most of our usage runs via the Console as Pre-Build Events.

You can generate Parent/Children files n-levels deep.  You have full control over naming of files and directories that are generated.

API provides you strongly-typed models. Console version provides Expando object for key/value pairs on a dynamic model.

**Console Usage Examples:**

Generates 'Start.g.cs' :

    RazorConsole --tbase "$(ProjectDir)\Templates\Entities" --obase "$(ProjectDir)\Output\Entities"
        -f Start.cshtml -o Start.g.cs

Generates only Child Files, no Parent output (notice suspendRootOutput). Supports loading 3rd party assemblies :

    RazorConsole --tbase "$(ProjectDir)\Templates\Entities" --obase "$(ProjectDir)\Output\Entities"
        --assemblyFolder "$(ProjectDir)\bin" --suspendRootOutput true -f Start.cshtml -o noFile
    
**Simple C# Template Script:**

This includes a partial template and generates some text

    @inherits RazorEngine.Templating.DictionaryTemplateBase<dynamic>

    Hello World. I'm a Razor Template.

    @{
        var partialModel = new { something = value };
    
        // Able to append other .cshtml templates to myself
        PartialTemplate("Partial.cshtml", partialModel);
    }
  
**Sample Parent/Child Template:**

This generates 3 child files: 'Child.1.out', 'Child.2.out', 'Child.3.out' into directory 'ChildFolder'

    @inherits RazorEngine.Templating.DictionaryTemplateBase<dynamic>
                  
    Hello World. I'm a Razor Template. Now let's generate 3 child files...
    
    @{
        var childTemplate = "Child.cshtml";
        var childFolder = "ChildFolder";

        var collection = new List<int>() { 1, 2, 3 };

        foreach (var i in collection)
    	{
            var childOutputName = String.Format(@"{0}\Child.{1}.out", childFolder, i);

            dynamic childModel = new System.Dynamic.ExpandoObject();
            childModel.someValue = "Hello Child, this is your Parent speaking";

            // Able to output to separate files and folders
            ChildTemplate(childTemplate, childModel, childOutputName);
    	}
    	
    }

**Running NUnit Tests:**

Make sure you disable 'Shadow-copy' in NUnit and/or Resharper.  Otherwise our test directories under bin\Debug will not be copied to the test folder, and thus - the tests will fail.

**Dependencies:**

* RazorEngine on CodePlex.
* gcConsole + NDesk for Console

We hope you enjoy!

Ryan & Jeremiah