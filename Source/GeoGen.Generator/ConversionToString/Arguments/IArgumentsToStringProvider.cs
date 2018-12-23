﻿using GeoGen.Core;
using GeoGen.Utilities;

namespace GeoGen.Generator
{
    /// <summary>
    /// Represents a generic arguments to string converter that uses a custom 
    /// <see cref="IObjectToStringConverter"/> converter. 
    /// </summary>
    public interface IArgumentsToStringProvider
    {
        /// <summary>
        /// Converts given arguments to string, using a given configuration object to string provider.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="objectToString">The configuration object to string converter.</param>
        /// <returns>The string representation the arguments.</returns>
        string ConvertToString(Arguments arguments, IToStringConverter<ConfigurationObject> objectToString);
    }
}