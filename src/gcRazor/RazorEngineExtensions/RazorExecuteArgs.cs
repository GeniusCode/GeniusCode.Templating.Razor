using System;
using System.Collections.Generic;
using RazorEngine;
using RazorEngine.Templating;
using System.IO;
using System.Reflection;

namespace RazorEngineExtensions
{
    public class RazorExecuteArgs
    {
        public List<string> namespaces = new List<string>();
        public TemplateService service = Razor.DefaultTemplateService;
        public string BaseOutputFolder;
        public RazorFileResolver resolver;
        public List<FileInfo> AssemblyFilesToLoad = new List<FileInfo>();

        public ExistingFileBehavior RootExistingFileBehavior;
        public bool SuspendRootOutput = false;

        public RazorExecuteArgs(string baseTemplateFolder, string baseOutputFolder)
        {
            resolver = new RazorFileResolver(baseTemplateFolder);
            BaseOutputFolder = baseOutputFolder;
        }
    }
}
