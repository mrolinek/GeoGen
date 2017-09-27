﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeoGen.Core.Configurations;
using GeoGen.Core.Constructions.Arguments;
using GeoGen.Core.Utilities;
using GeoGen.Generator.ConstructingConfigurations.ObjectsContainer;
using GeoGen.Generator.ConstructingConfigurations.ObjectToString.ObjectIdResolving;

namespace GeoGen.Generator.ConstructingConfigurations.IdsFixing
{
    /// <summary>
    /// A default implementation of <see cref="IIdsFixer"/>. It uses 
    /// <see cref="DictionaryObjectIdResolver"/> and caches resolved
    /// results. This class is not thread-safe.
    /// </summary>
    internal class IdsFixer : IIdsFixer
    {
        #region Private fields

        /// <summary>
        /// The configuration objects container.
        /// </summary>
        private readonly IConfigurationObjectsContainer _objectsContainer;

        /// <summary>
        /// The dictionary object id resolver.
        /// </summary>
        private readonly DictionaryObjectIdResolver _resolver;

        /// <summary>
        /// The cache dictionary mapping object ids to their fixed versions.
        /// </summary>
        private readonly Dictionary<int, ConstructedConfigurationObject> _cache;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new ids fixer that uses a given objects container
        /// (to register and cache new objects) and is associated with a
        /// given dictionary object id resolver.
        /// </summary>
        /// <param name="objectsContainer">The configuration objects container.</param>
        /// <param name="resolver">The dictionary object id resolver.</param>
        public IdsFixer(IConfigurationObjectsContainer objectsContainer, DictionaryObjectIdResolver resolver)
        {
            _objectsContainer = objectsContainer ?? throw new ArgumentNullException(nameof(objectsContainer));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _cache = new Dictionary<int, ConstructedConfigurationObject>();
        }

        #endregion

        #region IIdsFixer methods

        /// <summary>
        /// Replaces a given configuration object with a new one that 
        /// has correct ids of its interior objects.
        /// </summary>
        /// <param name="configurationObject">The configuration object.</param>
        /// <returns>The fixed configuration object.</returns>
        public ConfigurationObject FixObject(ConfigurationObject configurationObject)
        {
            if (configurationObject == null)
                throw new ArgumentNullException(nameof(configurationObject));

            // If the object is loose, we'll return the associated object from the container
            if (configurationObject is LooseConfigurationObject looseObject)
            {
                // Get the id of resolved object
                var resolvedId = _resolver.ResolveId(looseObject);

                // Return the object from the container
                return _objectsContainer[resolvedId];
            }

            // Pull the actual id of the object
            var id = configurationObject.Id ?? throw new GeneratorException("Id must be set.");

            // If we have the cached result, we'll return it
            if (_cache.ContainsKey(id))
                return _cache[id];

            // Since the object is not loose, it's constructed.
            var constructedObject = (ConstructedConfigurationObject) configurationObject;

            // First we fix arguments
            var newArguments = constructedObject.PassedArguments
                    .Select(FixArgument)
                    .ToList();

            // Pull index and construction from the original object
            var index = constructedObject.Index;
            var construction = constructedObject.Construction;

            // Create a fixed object with the new arguments and pulled information
            var fixedObject = new ConstructedConfigurationObject(construction, newArguments, index);

            // Let the container register the object.
            fixedObject = _objectsContainer.Add(fixedObject);

            // Cache it (it can't be cached yet)
            _cache.Add(id, fixedObject);

            // Return the fixed object
            return fixedObject;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Replaces a given construction argument with a new one that
        /// has correct ids of its interior objects. 
        /// </summary>
        /// <param name="argument">The construction argument.</param>
        /// <returns>The fixed configuration argument.</returns>
        private ConstructionArgument FixArgument(ConstructionArgument argument)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            // If the argument is an object argument
            if (argument is ObjectConstructionArgument objectArgument)
            {
                // We'll convert the passed object
                var fixedObject = FixObject(objectArgument.PassedObject);

                // Construct and return the result
                return new ObjectConstructionArgument(fixedObject);
            }

            // Otherwise we must have the set construction argument
            var setArgument = (SetConstructionArgument) argument;

            // We convert the interior arguments
            var interiorArguments = setArgument.PassedArguments
                    .Select(FixArgument)
                    .ToSet();

            // Construct and return the result
            return new SetConstructionArgument(interiorArguments);
        }

        #endregion
    }
}