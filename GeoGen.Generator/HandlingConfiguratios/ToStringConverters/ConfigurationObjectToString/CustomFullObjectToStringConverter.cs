﻿using GeoGen.Core.Configurations;

namespace GeoGen.Generator
{
    /// <summary>
    /// An implementation of <see cref="FullObjectToStringConverterBase"/> that
    /// uses a custom <see cref="IObjectIdResolver"/>. This sealed class is meant to be 
    /// used during the symmetric configurations detection, together with 
    /// <see cref="DictionaryObjectIdResolver"/> (<see cref="LeastConfigurationFinder"/>. 
    /// It expects that all objects have their ids already set. It automatically 
    /// caches the evaluated results (unlike <see cref="DefaultFullObjectToStringConverter"/>), 
    /// since it already has the ids available.
    /// </summary>
    internal sealed class CustomFullObjectToStringConverter : FullObjectToStringConverterBase
    {
        #region Constructor

        /// <summary>
        /// Constructs a new custom full object to string provider with a given
        /// arguments list to string provider and a given object id resolver.
        /// </summary>
        /// <param name="converter>The arguments list to string provider.</param>
        /// <param name="resolver">The configuration object id resolver.</param>
        public CustomFullObjectToStringConverter(IArgumentsListToStringConverter converter, IObjectIdResolver resolver)
                : base(converter, resolver)
        {
        }

        #endregion

        #region Abstract methods from base implementation

        /// <summary>
        /// Resolves if a given object has it's string value already cached.
        /// </summary>
        /// <param name="configurationObject">The configuration object.</param>
        /// <returns>The cached value, if exists, otherwise an empty string.</returns>
        protected override string ResolveCachedValue(ConfigurationObject configurationObject)
        {
            // We must have an id
            var id = configurationObject.Id ?? throw new GeneratorException("Id must be set");

            // Then we might or might have cached this object.
            return Cache.ContainsKey(id) ? Cache[id] : string.Empty;
        }

        /// <summary>
        /// Handles the resulting string value after constructing it, before returning it.
        /// </summary>
        /// <param name="configurationObject">The configuration object.</param>
        /// <param name="result">The object's string value.</param>
        protected override void HandleResult(ConfigurationObject configurationObject, string result)
        {
            // We must have an id
            var id = configurationObject.Id ?? throw new GeneratorException("Id must be set");

            // Then we can cache the object
            Cache.Add(id, result);
        }

        #endregion
    }
}