﻿using GeoGen.Core;
using System.Linq;

namespace GeoGen.TheoremRanker
{
    /// <summary>
    /// The <see cref="IAspectTheoremRanker"/> of <see cref="RankedAspect.NumberOfCyclicQuadrilaterals"/>.
    /// </summary>
    public class NumberOfCyclicQuadrilateralsRanker : AspectTheoremRankerBase
    {
        /// <inheritdoc/>
        public override double Rank(Theorem theorem, Configuration configuration, TheoremMap allTheorems)
            // Simply return the number of theorems of type ConcyclicPoints
            => allTheorems.GetObjectsForKeys(TheoremType.ConcyclicPoints).Count()
                // Potentially excluding the one we cannot prove, if it's of type ConcyclicPoints
                - (theorem.Type == TheoremType.ConcyclicPoints ? 1 : 0);
    }
}