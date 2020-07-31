﻿using GeoGen.Core;
using GeoGen.Infrastructure;
using GeoGen.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GeoGen.Infrastructure.Log;

namespace GeoGen.ProblemGenerator.InputProvider
{
    /// <summary>
    /// The default implementation of <see cref="IProblemGeneratorInputProvider"/> loading data from the file system.
    /// </summary>
    public class ProblemGeneratorInputProvider : IProblemGeneratorInputProvider
    {
        #region Private fields

        /// <summary>
        /// The settings for the folder with inputs.
        /// </summary>
        private readonly ProblemGeneratorInputProviderSettings _settings;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemGeneratorInputProvider"/> class.
        /// </summary>
        /// <param name="settings">The settings for the folder with inputs.</param>
        public ProblemGeneratorInputProvider(ProblemGeneratorInputProviderSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        #endregion

        #region IProblemGeneratorInputProvider implementation

        /// <inheritdoc/>
        public async Task<IReadOnlyList<LoadedProblemGeneratorInput>> GetProblemGeneratorInputsAsync()
        {
            // Log that we're starting
            LoggingManager.LogInfo($"Starting to search for input files in {_settings.InputFolderPath}");

            // Prepare the available input files
            var inputFiles = Directory.EnumerateFiles(_settings.InputFolderPath, $"*.{_settings.FileExtension}", SearchOption.AllDirectories)
                    // Find their file names
                    .Select(path => (path, fileName: Path.GetFileNameWithoutExtension(path)))
                    // Take only those that start with the requested pattern
                    .Where(tuple => tuple.fileName.StartsWith(_settings.InputFilePrefix))
                    // Find their ids 
                    .Select(tuple => (tuple.path, id: tuple.fileName.Substring(_settings.InputFilePrefix.Length)))
                    // Enumerate them
                    .ToList();

            // Make sure there is some...
            if (inputFiles.Count == 0)
            {
                // Log it
                LoggingManager.LogWarning("No file found on which we could run problem generation.");

                // Finish the method
                return new List<LoadedProblemGeneratorInput>();
            }

            // Inform about the found ones
            LoggingManager.LogInfo($"Found {inputFiles.Count} input file{(inputFiles.Count == 1 ? "" : "s")}:\n\n" +
                // With the description of their paths and indices
                $"{inputFiles.Select((file, index) => $"   {index + 1}. {file.path}").ToJoinedString("\n")}\n");

            // Prepare the result
            var result = new List<LoadedProblemGeneratorInput>();

            // Go through all the input files
            foreach (var (inputFilePath, id) in inputFiles)
            {
                #region Loading the file

                // Prepare the file content
                var fileContent = default(string);

                try
                {
                    // Get the content of the file
                    fileContent = await File.ReadAllTextAsync(inputFilePath);
                }
                catch (Exception e)
                {
                    // If it cannot be done, make aware
                    throw new ProblemGeneratorInputProviderException($"Couldn't load the input file '{inputFilePath}'", e);
                }

                #endregion

                #region Parsing it

                // Get the lines 
                var lines = fileContent.Split('\n')
                    // Trimmed
                    .Select(line => line.Trim())
                    // That are not comments or empty ones
                    .Where(line => !line.StartsWith('#') && !string.IsNullOrEmpty(line))
                    // As a list
                    .ToList();

                // If there is no content
                if (lines.IsEmpty())
                {
                    // Warn
                    LoggingManager.LogWarning($"Empty input file {inputFilePath}");

                    // Move on
                    continue;
                }

                try
                {
                    // Try to parse it
                    var generatorInput = ParseInput(lines);

                    // Add the loaded input to the result list
                    result.Add(new LoadedProblemGeneratorInput
                    (
                        filePath: inputFilePath,
                        id: id,
                        constructions: generatorInput.Constructions,
                        initialConfiguration: generatorInput.InitialConfiguration,
                        numberOfIterations: generatorInput.NumberOfIterations,
                        maximalNumbersOfObjectsToAdd: generatorInput.MaximalNumbersOfObjectsToAdd,
                        excludeAsymmetricConfigurations: generatorInput.ExcludeAsymmetricConfigurations
                    ));
                }
                catch (ParsingException e)
                {
                    // Throw further
                    throw new ProblemGeneratorInputProviderException($"Couldn't parse the input file {inputFilePath}.", e);
                }

                #endregion
            }

            // Return the found results
            return result;
        }

        /// <summary>
        /// Parses given lines to a problem generator input.
        /// </summary>
        /// <param name="lines">The trimmed non-empty lines without comments to be parsed.</param>
        /// <returns>The parsed problem generator input.</returns>
        private static ProblemGeneratorInput ParseInput(IReadOnlyList<string> lines)
        {
            #region Finding construction and configuration sections

            // Make sure the first line beings constructions
            if (lines[0] != "Constructions:")
                throw new ParsingException($"The first line should be 'Constructions:' to start the constructions section");

            // Find the configuration section starting line
            var configurationLineIndex = lines.IndexOf("Initial configuration:");

            // Make sure there is some
            if (configurationLineIndex == -1)
                throw new ParsingException("There should be a line ''Initial configuration:'' starting the initial configuration");

            #endregion

            #region Parsing constructions

            // Parse the constructions...Get the lines between the first 
            // index and the one starting the configuration section
            var constructions = lines.ItemsBetween(1, configurationLineIndex)
                // Each line defines a construction
                .Select(Parser.ParseConstruction)
                // Enumerate 
                .ToReadOnlyHashSet();

            #endregion

            #region Parsing configuration

            // Get the configuration lines from the remaining lines
            var configurationLines = lines.ItemsBetween(configurationLineIndex + 1, lines.Count)
                // Unless we run into a line specifying iterations
                .TakeWhile(line => !line.StartsWith("Iterations:"))
                // Enumerate
                .ToList();

            // Parse the configuration from them
            var configuration = Parser.ParseConfiguration(configurationLines).configuration;

            #endregion

            #region Parsing iterations

            // Find the index of the iteration line, which should be after the configuration declaration
            var iterationLineIndex = configurationLineIndex + configurationLines.Count + 1;

            // Make sure we have enough lines
            if (iterationLineIndex >= lines.Count)
                throw new ParsingException("The line after the specification of the configuration should be in the form 'Iterations: {number}'");

            // Now we can parse the line
            var iterationsNumberMatch = Regex.Match(lines[iterationLineIndex], "^Iterations:(.+)$");

            // If there is no match, make aware
            if (!iterationsNumberMatch.Success)
                throw new ParsingException("The line after the specification of the configuration should be in the form 'Iterations: {number}'");

            // Prepare the number of iterations
            var numberOfIterations = default(int);

            try
            {
                // Try to parse 
                numberOfIterations = int.Parse(iterationsNumberMatch.Groups[1].Value.Trim());

                // Make sure it's a correct value
                if (numberOfIterations < 0)
                    throw new ParsingException($"The number of iterations cannot be negative, the found value is {numberOfIterations}.");
            }
            catch (Exception e) when (e is FormatException || e is OverflowException)
            {
                // Make sure the user is aware
                throw new ParsingException($"Cannot parse the number of iterations: '{iterationsNumberMatch.Groups[1].Value.Trim()}'");
            }

            #endregion

            #region Parsing maximal number of objects to be added

            // Find the configuration object types
            var objectTypes = Enum.GetValues(typeof(ConfigurationObjectType)).Cast<ConfigurationObjectType>().ToList();

            // Each type needs to have a line. Make sure we have enough lines
            if (iterationLineIndex + objectTypes.Count >= lines.Count)
                throw new ParsingException($"There should be {objectTypes.Count} lines after the iterations, " +
                    $"each specifying the maximal number of objects of the given type to be added to the initial configuration.");

            // Look for them
            var maximalNumbersOfObjectsToAdd = objectTypes.ToDictionary(objectType => objectType, objectType =>
            {
                // Go through the lines where the maximal counts should be
                var maximalNumberMatch = lines.ItemsBetween(iterationLineIndex + 1, iterationLineIndex + 1 + objectTypes.Count)
                    // The line should be for example 'MaximalPoints: 4'
                    .Select(line => Regex.Match(line, $"^Maximal{objectType}s:(.+)$"))
                    // Take the first match, if there is any
                    .FirstOrDefault(match => match.Success)
                    // If not, make aware
                    ?? throw new ParsingException($"No line in the form Maximal{objectType}s: {{number}}");

                // Prepare the maximal number
                var maximalNumber = default(int);

                try
                {
                    // Try to parse 
                    maximalNumber = int.Parse(maximalNumberMatch.Groups[1].Value.Trim());
                }
                catch (Exception)
                {
                    // Make sure the user is aware
                    throw new ParsingException($"Cannot parse the maximal number of {objectTypes}s: '{maximalNumberMatch.Groups[1].Value.Trim()}'");
                }

                // Make sure it's a correct value
                if (maximalNumber < 0)
                    throw new ParsingException($"The maximal number of {objectType}s cannot be negative, the found value is {maximalNumber}.");

                // Now we have it parse and we can finally return it
                return maximalNumber;
            });

            #endregion

            #region Parsing symmetry generation flag

            // Prepare the index of the line. After iterations there is a line for each object
            // and the following line should be the line with the flag
            var symmetryGenerationFlagLineIndex = iterationLineIndex + objectTypes.Count + 1;

            // Ensure this is the last line
            if (symmetryGenerationFlagLineIndex != lines.Count - 1)
                throw new ParsingException("The last line of the input file should indicate whether we want to generate only symmetric configurations.");

            // Parse it
            var symmetryFlagMatch = Regex.Match(lines[symmetryGenerationFlagLineIndex], "^GenerateOnlySymmetricConfigurations:(.*)$");

            // Ensure there is a match
            if (!symmetryFlagMatch.Success)
                throw new ParsingException("The last line of the input file should be of form GenerateOnlySymmetricConfigurations: {flag}, where {flag} is either true or false.");

            // Get the flag string
            var symmetryFlagString = symmetryFlagMatch.Groups[1].Value.Trim();

            // Try to parse the value
            if (!bool.TryParse(symmetryFlagString, out var excludeAsymmetricConfigurations))
                throw new ParsingException($"Cannot parse the symmetry generation flag: {symmetryFlagString}");

            #endregion

            // Return the final input
            return new ProblemGeneratorInput
            (
                initialConfiguration: configuration,
                constructions: constructions,
                numberOfIterations: numberOfIterations,
                maximalNumbersOfObjectsToAdd: maximalNumbersOfObjectsToAdd,
                excludeAsymmetricConfigurations: excludeAsymmetricConfigurations
            );
        }

        #endregion
    }
}
