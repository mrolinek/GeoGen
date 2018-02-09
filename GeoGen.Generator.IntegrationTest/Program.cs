﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using GeoGen.AnalyticalGeometry;
using GeoGen.Analyzer;
using GeoGen.Core;
using GeoGen.Utilities;
using Ninject;
using Ninject.Planning.Bindings.Resolvers;
using static GeoGen.Core.PredefinedConstructionType;

namespace GeoGen.Generator.IntegrationTest
{
    public class Program
    {
        private static ConstructionsContainer _constructionsContainer;

        private static ComposedConstructions _composedConstructions;

        private static ConstructorHelper _constructorHelper;

        private static void Main()
        {
            //for (var j = 0; j < 50; j++)
            //{
//            while (true)
                //          {
                _constructionsContainer = new ConstructionsContainer();
                _composedConstructions = new ComposedConstructions(_constructionsContainer);
                _constructorHelper = new ConstructorHelper(_constructionsContainer);

                var kernel = new StandardKernel
                (
                    new GeneratorModule(),
                    new UtilitiesModule(),
                    new AnalyerModule(),
                    new AnalyticalGeometryModule()
                );

                kernel.Components.RemoveAll<IMissingBindingResolver>();
                kernel.Settings.AllowNullInjection = true;
                var tracker = new ConsoleInconsistenciesTracker();
                kernel.Bind<IInconsistenciesTracker>().ToConstant(tracker);

                //kernel.Rebind<ITheoremsAnalyzer>().ToConstant(new DummyTheoremsAnalyzer());
                //kernel.Rebind<IGeometryRegistrar>().ToConstant(new DummyGeometryRegistrar());

                var factory = kernel.Get<IGeneratorFactory>();

                var points = Enumerable.Range(0, 3)
                        .Select(i => new LooseConfigurationObject(ConfigurationObjectType.Point))
                        .ToList();

                var constructedObjects = ConstructedObjects(points);

                var configuration = new Configuration(points, constructedObjects);
                var constructions = Constructions();

                var input = new GeneratorInput
                {
                    InitialConfiguration = configuration,
                    Constructions = constructions,
                    MaximalNumberOfIterations = 3
                };

                var generator = factory.CreateGenerator(input);
                var stopwatch = new Stopwatch();

                stopwatch.Start();
                var result = generator.Generate().ToList();
                stopwatch.Stop();

                //Console.WriteLine("Starting the first attempt..");
                //Console.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds}");
                //Console.WriteLine($"Generated: {result.Count}");
                //Console.WriteLine($"Generated with theorems: {result.Count(r => r.Theorems.Any())}");
                Console.WriteLine($"Total number of theorems: {result.Sum(output => output.Theorems.Count)}");
                //Console.WriteLine($"Inconsistencies: {tracker.Inconsistencies}");
                //Console.WriteLine($"Failed attempts to reconstruct: {tracker.AttemptsToReconstruct}");
           // }

            //Console.WriteLine("Starting the second attempt..");
                //var otherResult = factory.CreateGenerator(input).Generate().ToList();
                //Console.WriteLine("Done");

                //var formatter = new OutputFormatter(_constructionsContainer);

                //var counter = 0;

                //for (var i = 0; i < result.Count; i++)
                //{
                //    var first = result[i];
                //    var second = otherResult[i];

                //    string CastTheorem(Theorem theorem)
                //    {
                //        return formatter.ConvertToString(theorem);
                //    }

                //    var c1 = formatter.Format(first.Configuration);
                //    var c2 = formatter.Format(second.Configuration);

                //    if (c1 != c2)
                //    {
                //        Console.WriteLine();
                //    }

                //    var firstSet = first.Theorems.Select(CastTheorem).ToSet();

                //    var secondSet = second.Theorems.Select(CastTheorem).ToSet();

                //    var set = Differences(firstSet, secondSet);

                //    if (set.Empty())
                //        continue;

                //    if (set.Count == 13)
                //    {
                //        Console.WriteLine("Magical 13");
                //        continue;
                //    }

                //    Console.WriteLine(i);

                //    Console.WriteLine($"{++counter}. In configuration: ");
                //    Console.WriteLine("-------------------\n");
                //    Console.WriteLine(formatter.Format(first.Configuration));
                //    Console.WriteLine("-------------------\n");
                //    Console.WriteLine("Theorems generated exactly in one of two runs:");
                //    Console.WriteLine("-------------------\n");

                //    foreach (var s in set)
                //    {
                //        Console.WriteLine(s);
                //    }

                //    Console.WriteLine();
                //    Console.ReadKey();
                //}
    //        }

            //var theoremsOriginal = TryLoadTheorems();
            //var theorems = ConvertTheormes(result);

            //if (theoremsOriginal == null)
            //{
            //    using (var writer = new StreamWriter("theorems.txt"))
            //    {
            //        writer.Write(string.Join(";",theorems));
            //    }
            //}
            //else
            //{
            //    var firstDifferent = theorems.FirstOrDefault(t => !theoremsOriginal.Contains(t));

            //    if(firstDifferent == null)
            //        return;

            //    var id = int.Parse(new Regex("^(\\d+)").Match(firstDifferent).Groups[1].Value);

            //    Console.WriteLine($"Original:{theoremsOriginal[id]}");
            //    Console.WriteLine($"New:{theorems[id]}");
            //}

            //PrintTheorems(result);
        }

        private static HashSet<string> Differences(HashSet<string> firstSet, HashSet<string> secondSet)
        {
            var all = firstSet.Union(secondSet).ToList();

            var result = new HashSet<string>();

            foreach (var s in all)
            {
                if (firstSet.Contains(s) && secondSet.Contains(s))
                    continue;

                result.Add(s);
            }

            return result;
        }

        private static List<string> ConvertTheormes(List<GeneratorOutput> result)
        {
            var formatter = new OutputFormatter(_constructionsContainer);

            var i = 0;

            return result.SelectMany(r =>
            {
                var configurationString = formatter.Format(r.Configuration);

                var theoremsStrings = r.Theorems.Select(formatter.ConvertToString).ToList();

                theoremsStrings.Sort();

                return theoremsStrings.Select(s => $"{i++}---{configurationString}---{s}");
            }).ToList();
        }

        private static List<string> TryLoadTheorems()
        {
            try
            {
                using (var reader = new StreamReader("theorems.txt"))
                {
                    return reader.ReadToEnd().Split(';').ToList();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<Construction> Constructions()
        {
            return new List<Construction>
            {
                //_composedConstructions.AddIncenterFromPoints(),
                _constructionsContainer.Get(IntersectionOfLinesFromPoints),
                _constructionsContainer.Get(MidpointFromPoints),
                //_constructionsContainer.Get(CircumcenterFromPoints),
                //_constructionsContainer.Get(IntersectionOfLines),
                //_constructionsContainer.Get(IntersectionOfLinesFromLineAndPoints),
                //_constructionsContainer.Get(PerpendicularLineFromPoints),
                //_constructionsContainer.Get(InternalAngleBisectorFromPoints)
            };
        }

        private static List<ConstructedConfigurationObject> ConstructedObjects(List<LooseConfigurationObject> points)
        {
            _composedConstructions.AddIncenterFromPoints();

            var o = _constructorHelper.CreateCircumcenter(points[0], points[1], points[2]);
            var i = _constructorHelper.CreateIncenter(points[0], points[1], points[2]);
            //var p = _constructorHelper.CreateIntersection(o, i, points[0], points[1]);

            return new List<ConstructedConfigurationObject>
            {
                //o , i, //p
            };
        }

        private static void PrintTheorems(IEnumerable<GeneratorOutput> result)
        {
            var formatter = new OutputFormatter(_constructionsContainer);

            Console.ReadKey(true);
            Console.WriteLine("Results:\n");

            var i = 1;

            foreach (var generatorOutput in result.Where(t => t.Theorems.Any()))
            {
                Console.Clear();
                Console.WriteLine($"{i++}.");
                Console.WriteLine("-------------------\n");
                Console.WriteLine(formatter.Format(generatorOutput.Configuration));
                Console.WriteLine("-------------------\n");
                Console.WriteLine("Theorems:");
                Console.WriteLine(formatter.Format(generatorOutput.Theorems));
                Console.ReadKey(true);
            }
        }
    }
}