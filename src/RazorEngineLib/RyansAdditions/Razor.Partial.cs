namespace RazorEngine
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Compilation;
    using Configuration;
    using Templating;

    /// <summary>
    /// Provides quick access to template services.
    /// </summary>
    public static partial class Razor
    {
        /// <summary>
        /// Parses the given template and returns the result.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="template">The template to parse.</param>
        /// <param name="model">The model.</param>
        /// <param name="name">[Optional] The name of the template. This is used to cache the template.</param>
        /// <returns>The string result of the parsed template.</returns>
        public static Dictionary<string, string> ParseDictionary<T>(string template, T model, out IDictionaryTemplate templateBase, string name = null)
        {
            return DefaultTemplateService.ParseDictionary<T>(template, model, out templateBase, name);
        }
    }
}
