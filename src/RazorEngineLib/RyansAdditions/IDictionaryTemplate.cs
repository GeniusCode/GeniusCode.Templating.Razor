using System.Collections.Generic;
namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing a template with a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public interface IDictionaryTemplate : ITemplate
    {
        Dictionary<string, string> ResultDictionary { get; }
        
        void OnAfterExecute();
    }
}