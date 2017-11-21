﻿using GeoGen.Core.Constructions;

namespace GeoGen.Analyzer.Constructing
{
    internal interface IConstructorsResolver
    {
        IObjectsConstructor Resolve(Construction construction);
    }
}