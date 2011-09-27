namespace RazorEngine.Templating
{
	using System.Dynamic;

	using Compilation;
    using System.Collections.Generic;
    using System.IO;

	/// <summary>
	/// Provides a base implementation of a template with a model.
	/// </summary>
    public abstract class DictionaryTemplateBase : DictionaryTemplateBase<dynamic>
    {
    }

    /// <summary>
    /// Provides a base implementation of a template with a model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class DictionaryTemplateBase<TModel> : TemplateBase<TModel>, IDictionaryTemplate
    {
        string currentKeyName = string.Empty;
        public const string HeaderKeyName = "HeaderResult";

        protected Dictionary<string, string> _ResultDictionary;

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateBase{TModel}"/>.
        /// </summary>
        protected DictionaryTemplateBase()
            : base()
        {
            _ResultDictionary = new Dictionary<string, string>();
        }
        #endregion

        #region Private Helpers

        private void SetScope(string keyName)
        {
            // Load Result thus far into Dictionary
            LoadCurrentResultIntoDictionary();

            // Clear Last Result
            Builder.Clear();

            // Setup new Key
            currentKeyName = keyName;
        }

        /// <summary>
        /// Change to a Child scope. Child scopes are typically outputed to a separate file.
        /// </summary>
        /// <param name="childKeyName"></param>
        private void SetScopeToChild(string childKeyName)
        {
            SetScope(childKeyName);
        }

        private void LoadCurrentResultIntoDictionary()
        {
            if (_ResultDictionary.ContainsKey(currentKeyName) == false)
                _ResultDictionary.Add(currentKeyName, string.Empty);

            // Append Result thus far into Dictionary
            _ResultDictionary[currentKeyName] += Result;
        }
        #endregion

        #region Public members

        /// <summary>
        /// Writes out an Included template to the current scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeTemplateName">Name of compiled / cached template to include</param>
        /// <param name="model"></param>
        public virtual void PartialTemplate<T>(string templateName, T model)
        {
            var output = Include(templateName, model);
            Write(output);
        }

        /// <summary>
        /// Writes out an Included template to the specified child scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeTemplateName">Name of compiled / cached template to include</param>
        /// <param name="model"></param>
        public virtual void ChildTemplate<T>(string templateName, T model, string outputName)
        {
            string previousScope = currentKeyName;
            SetScopeToChild(outputName);

            PartialTemplate(templateName, model);

            SetScope(previousScope);
        }

        #endregion



        #region IDictionaryTemplate implementation

        Dictionary<string, string> IDictionaryTemplate.ResultDictionary { get { return _ResultDictionary; } }

        void IDictionaryTemplate.OnAfterExecute()
        {
            // Load last Result into Dictionary
            LoadCurrentResultIntoDictionary();
        }

        #endregion


        #region Sealed virtual methods
        private new string Include(string name)
        {
            return base.Include(name);
        }

        private new string Include<T>(string name, T model)
        {
            return base.Include<T>(name, model);
        }
        #endregion

    }


}