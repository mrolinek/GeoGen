﻿using System;
using GeoGen.Analyzer.Objects;
using GeoGen.Core.Theorems;

namespace GeoGen.Analyzer.Theorems
{
    internal sealed class VerifierOutput
    {
        public Func<IObjectsContainer, bool> VerifierFunction { get; set; }

        public Func<Theorem> Theorem { get; set; }
    }
}