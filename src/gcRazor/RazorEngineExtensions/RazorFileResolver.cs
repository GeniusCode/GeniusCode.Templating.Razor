using System.IO;

namespace RazorEngineExtensions
{
    public class RazorFileResolver : RazorEngine.Templating.ITemplateResolver
    {
        private string _BaseTemplateFolder { get; set; }

        public RazorFileResolver(string baseTemplateFolder)
        {
            BaseTemplateFolder = baseTemplateFolder;
        }

        public string BaseTemplateFolder
        {
            get { return _BaseTemplateFolder; }
            set
            {
                // Folders must always end with a slash
                if (!value.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    value += Path.DirectorySeparatorChar;

                _BaseTemplateFolder = value;
            }
        }

        #region ITemplateResolver

        public string GetTemplate(string templateName)
        {
            var filePath = Path.Combine(BaseTemplateFolder, templateName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Cannot find Template file", filePath);

            return File.ReadAllText(filePath);
        }
        #endregion

    }
}
