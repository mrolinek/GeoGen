﻿using System;

namespace GeoGen.TheoremProver.InferenceRuleProvider
{
    /// <summary>
    /// The settings for <see cref="InferenceRuleProvider"/> containing information about the rule folder.
    /// </summary>
    public class InferenceRuleProviderSettings
    {
        #region Public properties

        /// <summary>
        /// The path to the folder containing inference rules.
        /// </summary>
        public string RuleFolderPath { get; }

        /// <summary>
        /// The file extension of all rule files.
        /// </summary>
        public string FileExtension { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="InferenceRuleProviderSettings"/> class.
        /// </summary>
        /// <param name="ruleFolderPath">The path to the folder containing inference rules.</param>
        /// <param name="fileExtension">The file extension of all rule files.</param>
        public InferenceRuleProviderSettings(string ruleFolderPath, string fileExtension)
        {
            RuleFolderPath = ruleFolderPath ?? throw new ArgumentNullException(nameof(ruleFolderPath));
            FileExtension = fileExtension ?? throw new ArgumentNullException(nameof(fileExtension));
        }

        #endregion
    }
}