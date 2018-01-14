﻿using GeoGen.Core;

namespace GeoGen.Generator
{
    /// <summary>
    /// A NInject module that binds things from the Core module.
    /// </summary>
    public class GeneratorModule : BaseModule
    {
        /// <summary>
        /// Loads all bindings.
        /// </summary>
        public override void Load()
        {
            // General code
            BindDefiningGeneratorScope<IGenerator, Generator>("maximalNumberOfIterations", input => input.MaximalNumberOfIterations);
            BindFactoryInSingletonScope<IGeneratorFactory>();

            // Constructing objects
            BindInGeneratorScope<IArgumentsGenerator, ArgumentsGenerator>();
            BindFactoryInGeneratorScope<IArgumentsListContainerFactory>();
            BindInTransietScope<IArgumentsListContainer, ArgumentsListContainer>();
            BindInGeneratorScope<IDefaultArgumentsListToStringConverter, DefaultArgumentsListToStringConverter>();
            BindInGeneratorScope<IDefaultObjectToStringConverter, DefaultObjectToStringConverter>();
            BindInGeneratorScope<IDefaultObjectIdResolver, DefaultObjectIdResolver>();
            BindInGeneratorScope<IConstructionSignatureMatcher, ConstructionSignatureMatcher>();
            BindInGeneratorScope<IConstructionsContainer, ConstructionsContainer>("constructions", input => input.Constructions);
            BindInGeneratorScope<IObjectsConstructor, ObjectsConstructor>();

            // Handling configurations
            BindInGeneratorScope<IConfigurationsManager, ConfigurationsManager>("initialConfiguration", input => input.InitialConfiguration);
            BindInGeneratorScope<IConfigurationObjectsContainer, ConfigurationObjectsContainer>("initialConfiguration", input => input.InitialConfiguration);
            BindInGeneratorScope<IConfigurationConstructor, ConfigurationConstructor>();
            BindInGeneratorScope<IMinimalFormResolver, MinimalFormResolver>();
            BindInGeneratorScope<IObjectIdResolversContainer, ObjectIdResolversContainer>();
            BindInGeneratorScope<IConfigurationToStringProvider, ConfigurationToStringProvider>();
            BindInGeneratorScope<IArgumentsListToStringProvider, ArgumentsListToStringProvider>();
            BindInGeneratorScope<IDefaultFullObjectToStringConverter, DefaultFullObjectToStringConverter>();
            BindInGeneratorScope<IConfigurationsContainer, ConfigurationsContainer>();
            BindInGeneratorScope<IConfigurationsResolver, ConfigurationsResolver>();
            BindInGeneratorScope<IFullConfigurationToStringConverter, FullConfigurationToStringConverter>();
            BindInGeneratorScope<IFullObjectToStringConvertersFactory, FullObjectToStringConvertersFactory>();
            BindFactoryInGeneratorScope<IAutocacheFullObjectToStringConverterFactory>();
            BindInTransietScope<IAutocacheFullObjectToStringConverter, AutocacheFullObjectToStringConverter>();
            BindInGeneratorScope<ILooseObjectsHolder, ConfigurationObjectsContainer>("initialConfiguration", input => input.InitialConfiguration);
        }
    }
}