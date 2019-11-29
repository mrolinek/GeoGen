﻿using GeoGen.Core;
using GeoGen.TheoremProver;
using System.Linq;

namespace GeoGen.TheoremRanker
{
    /// <summary>
    /// The <see cref="IAspectTheoremRanker"/> of <see cref="RankedAspect.ObjectsPerCircles"/>.
    /// </summary>
    public class ObjectsPerCirclesRanker : AspectTheoremRankerBase
    {
        /// <summary>
        /// Ranks a given theorem, potentially using all given provided context.
        /// </summary>
        /// <param name="theorem">The theorem to be ranked.</param>
        /// <param name="configuration">The configuration where the theorem holds.</param>
        /// <param name="allTheorems">The map of all theorems of the configuration.</param>
        /// <param name="proverOutput">The output from the theorem prover for all the theorems of the configuration.</param>
        /// <returns>A number representing the ranking of the theorem. The range of its values depends on the implementation.</returns>
        public override double Rank(Theorem theorem, Configuration configuration, TheoremMap allTheorems, TheoremProverOutput proverOutput)
            // Simply apply the formula described in the documentation of RankedAspect.ObjectsPerCircles
            => 1D / (1 + allTheorems.GetTheoremsOfTypes(TheoremType.ConcyclicPoints).Count());
    }
}
