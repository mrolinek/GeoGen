﻿using GeoGen.Core;

namespace GeoGen.Generator
{
    /// <summary>
    /// A factory for creating an <see cref="IGenerator"/> from a generator input.
    /// </summary>
    public interface IGeneratorFactory
    {
        /// <summary>
        /// Creates a generator for a given generator input.
        /// </summary>
        /// <param name="generatorInput">The generator input.</param>
        /// <returns>The generator.</returns>
        IGenerator CreateGenerator(GeneratorInput generatorInput);
    }
}