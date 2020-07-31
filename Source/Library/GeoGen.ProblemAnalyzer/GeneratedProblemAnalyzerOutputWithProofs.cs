﻿using GeoGen.Core;
using GeoGen.TheoremProver;
using GeoGen.TheoremRanker;
using System;
using System.Collections.Generic;

namespace GeoGen.ProblemAnalyzer
{
    /// <summary>
    /// Represents an output of <see cref="IGeneratedProblemAnalyzer"/> that includes <see cref="TheoremProofs"/>.
    /// </summary>
    public class GeneratedProblemAnalyzerOutputWithProofs : GeneratedProblemAnalyzerOutputBase
    {
        #region Public properties

        /// <summary>
        /// The dictionary mapping proved theorems to their proofs.
        /// </summary>
        public IReadOnlyDictionary<Theorem, TheoremProof> TheoremProofs { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedProblemAnalyzerOutputWithProofs"/> class.
        /// </summary>
        /// <param name="interestingTheorems"><inheritdoc cref="GeneratedProblemAnalyzerOutputBase.InterestingTheorems" path="/summary"/></param>
        /// <param name="notInterestringAsymmetricTheorems"><inheritdoc cref="GeneratedProblemAnalyzerOutputBase.NotInterestringAsymmetricTheorems" path="/summary"/></param>
        /// <param name="theoremProofs"><inheritdoc cref="TheoremProofs" path="/summary"/></param>
        public GeneratedProblemAnalyzerOutputWithProofs(IReadOnlyList<RankedTheorem> interestingTheorems,
                                                        IReadOnlyCollection<Theorem> notInterestringAsymmetricTheorems,
                                                        IReadOnlyDictionary<Theorem, TheoremProof> theoremProofs)

            : base(interestingTheorems, notInterestringAsymmetricTheorems)
        {
            TheoremProofs = theoremProofs ?? throw new ArgumentNullException(nameof(theoremProofs));
        }

        #endregion
    }
}
