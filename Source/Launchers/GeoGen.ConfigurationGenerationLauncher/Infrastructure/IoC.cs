﻿using GeoGen.ConfigurationGenerator;
using GeoGen.Constructor;
using GeoGen.Infrastructure;
using GeoGen.MainLauncher;
using GeoGen.ProblemGenerator;
using GeoGen.ProblemGenerator.InputProvider;
using GeoGen.TheoremFinder;
using Ninject;
using System.Threading.Tasks;

namespace GeoGen.ConfigurationGenerationLauncher
{
    /// <summary>
    /// Represents a static initializer of the dependency injection system.
    /// </summary>
    public static class IoC
    {
        #region Kernel

        /// <summary>
        /// Gets the dependency injection container.
        /// </summary>
        public static IKernel Kernel { get; private set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the <see cref="Kernel"/> and all the dependencies.
        /// </summary>
        /// <param name="settings">The settings of the application.</param>
        public static Task InitializeAsync(Settings settings)
        {
            // Initialize the container
            Kernel = Infrastructure.IoC.CreateKernel();

            // Add the logging system
            Kernel.AddLogging(settings.LoggingSettings);

            #region Local dependencies

            // Add local dependencies
            Kernel.Bind<IBatchRunner>().To<BatchRunner>();
            Kernel.Bind<IProblemGenerationRunner>().To<GenerationOnlyProblemGenerationRunner>().WithConstructorArgument(settings.GenerationOnlyProblemGenerationRunnerSettings);

            #endregion           

            #region Problem generator

            // Add the configuration generator with its settings
            Kernel.AddConfigurationGenerator(settings.GenerationSettings)
                // Add the constructor
                .AddConstructor()
                // Add the problem generator with its settings
                .AddProblemGenerator(settings.ProblemGeneratorSettings)
                // Add problem generator input provider
                .AddProblemGeneratorInputProvider(settings.ProblemGeneratorInputProviderSettings);

            // Add an empty theorem finder
            Kernel.Bind<ITheoremFinder>().To<EmptyTheoremFinder>();

            #endregion

            #region Tracers

            // Rebind Constructor Failure Tracer only if we're supposed be tracing
            if (settings.TraceConstructorFailures)
                Kernel.Rebind<IConstructorFailureTracer>().To<ConstructorFailureTracer>().WithConstructorArgument(settings.ConstructorFailureTracerSettings);

            // Rebind Geometry Failure Tracer only if we're supposed be tracing
            if (settings.TraceGeometryFailures)
                Kernel.Rebind<IGeometryFailureTracer>().To<GeometryFailureTracer>().WithConstructorArgument(settings.GeometryFailureTracerSettings);

            #endregion

            // Return a finished task
            return Task.CompletedTask;
        }

        #endregion
    }
}