namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Compilation;

    /// <summary>
    /// Defines a templating service.
    /// </summary>
    public partial class TemplateService
    {
        public Dictionary<string, string> ParseDictionary<T>(string template, T model)
        {
            IDictionaryTemplate baseType;
            return ParseDictionary(template, model, out baseType);
        }

        /// <summary>
        /// Parses the given template and returns the result.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="template">The template to parse.</param>
        /// <param name="model">The model.</param>
        /// <param name="name">[Optional] The name of the template. This is used to cache the template.</param>
        /// <returns>The string result of the parsed template.</returns>
        public Dictionary<string, string> ParseDictionary<T>(string template, T model, out IDictionaryTemplate razorBaseType, string templateName = null)
        {
            var instance = GetTemplate(template, typeof(T), templateName);

            var dictionaryTemplate = instance as IDictionaryTemplate;

            if (dictionaryTemplate == null)
                throw new Exception("TemplateBase must be an IDictionaryTemplate<T>.  Did you forget to call SetTemplateBase() ?");

            SetService(instance, this);
            SetModel(instance, model);
            instance.Execute();

            // Because .Execute() doesn't call our base class
            dictionaryTemplate.OnAfterExecute();

            razorBaseType = dictionaryTemplate;

            return dictionaryTemplate.ResultDictionary;
        }


        //private Type GetRazorBaseTypeForFile(string template)
        //{
        //    // TODO: Scan file's 1st line with REGEX and resolve Type, if exists
        //    // Pretty sure we don't need this!!
        //    // Razor will read the @inherits line itself! Wayhoo!
        //    return templateType;
        //}

        /// <summary>
        /// Creates an instance of <see cref="ITemplate"/> from the specified template.
        /// </summary>
        /// <param name="template">The template to compile.</param>
        /// <param name="modelType">The model type.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        internal ITemplate CreateTemplate(string template, Type modelType)
        {
            // Modified by Ryan (but commented back out, not needed)
            var baseType = templateType; //GetRazorBaseTypeForFile(template);

            var context = new TypeContext
            {
                TemplateType = baseType,
                TemplateContent = template,
                ModelType = modelType
            };

            foreach (string @namespace in Namespaces)
                context.Namespaces.Add(@namespace);

            Type instanceType = compilerService.CompileType(context);
            var instance = activator.CreateInstance(instanceType);

            return instance;
        }

    }

}
