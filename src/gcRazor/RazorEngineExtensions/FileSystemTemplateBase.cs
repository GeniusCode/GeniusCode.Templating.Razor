using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorEngine.Templating;

namespace RazorEngineExtensions
{
    public enum ExistingFileBehavior : int
    {
        ThrowException = 0,
        NeverOverwrite,
        AlwaysOverwrite
    }

    public interface IFileSystemTemplate : IDictionaryTemplate
    {
        Dictionary<string, ExistingFileBehavior> ExistingFileBehaviors { get; }
    }

    public class FileSystemTemplateBase<T> : DictionaryTemplateBase<T>, IFileSystemTemplate
    {
        private Dictionary<string, ExistingFileBehavior> _ExistingFileBehaviors = new Dictionary<string,ExistingFileBehavior>();

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateBase{TModel}"></see>.
        /// </summary>
        protected FileSystemTemplateBase()
        {
        }

            
        public override void ChildTemplate<T>(string templateName, T model, string outputName)
        {
            this.ChildTemplate(templateName, model, outputName, ExistingFileBehavior.ThrowException);
        }

        public void ChildTemplate<T>(string templateName, T model, string outputName, ExistingFileBehavior overwriteBehavior)
        {
            _ExistingFileBehaviors.Add(outputName, overwriteBehavior);

            base.ChildTemplate(templateName, model, outputName);
        }

        Dictionary<string, ExistingFileBehavior> IFileSystemTemplate.ExistingFileBehaviors
        {
            get { return _ExistingFileBehaviors; }
        }

    }

}
