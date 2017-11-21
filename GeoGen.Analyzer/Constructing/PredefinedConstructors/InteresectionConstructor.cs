﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeoGen.AnalyticalGeometry;
using GeoGen.AnalyticalGeometry.AnalyticalObjects;
using GeoGen.Analyzer.Objects;
using GeoGen.Core.Configurations;
using GeoGen.Core.Constructions.Arguments;
using GeoGen.Core.Constructions.PredefinedConstructions;
using GeoGen.Core.Theorems;

namespace GeoGen.Analyzer.Constructing.PredefinedConstructors
{
    internal sealed class InteresectionConstructor : IPredefinedConstructor
    {
        public Type PredefinedConstructionType { get; } = typeof(Intersection);

        public ConstructorOutput Construct(List<ConstructedConfigurationObject> constructedObjects)
        {
            try
            {
                var constructedObject = constructedObjects[0];
                var arguments = constructedObject.PassedArguments;

                var setArgument1 = (SetConstructionArgument) arguments[0];
                var setArgument2 = (SetConstructionArgument) arguments[1];

                var passedPoints1 = setArgument1.PassedArguments.ToList();
                var passedPoints2 = setArgument2.PassedArguments.ToList();

                var obj1 = ((ObjectConstructionArgument) passedPoints1[0]).PassedObject;
                var obj2 = ((ObjectConstructionArgument) passedPoints1[1]).PassedObject;
                var obj3 = ((ObjectConstructionArgument) passedPoints2[0]).PassedObject;
                var obj4 = ((ObjectConstructionArgument) passedPoints2[1]).PassedObject;

                List<IAnalyticalObject> ConstructorFunction(IObjectsContainer container)
                {
                    var point1 = container.Get<Point>(obj1);
                    var point2 = container.Get<Point>(obj2);
                    var point3 = container.Get<Point>(obj3);
                    var point4 = container.Get<Point>(obj4);

                    var points = new HashSet<Point> {point1, point2, point3, point4};

                    var line1 = new Line(point1, point2);
                    var line2 = new Line(point3, point4);

                    var result = line1.IntersectionWith(line2);

                    if (result == null || points.Contains(result.Value))
                        return null;

                    return new List<IAnalyticalObject> {result};
                }

                var objects1 = new List<TheoremObject>
                {
                    new TheoremObject(obj1),
                    new TheoremObject(obj2),
                    new TheoremObject(constructedObject)
                };
                var objects2 = new List<TheoremObject>
                {
                    new TheoremObject(obj3),
                    new TheoremObject(obj4),
                    new TheoremObject(constructedObject)
                };

                var collinearityTheorem1 = new Theorem(TheoremType.CollinearPoints, objects1);
                var collinearityTheorem2 = new Theorem(TheoremType.CollinearPoints, objects2);

                return new ConstructorOutput
                {
                    ConstructorFunction = ConstructorFunction,
                    Theorems = new List<Theorem> {collinearityTheorem1, collinearityTheorem2}
                };
            }
            catch (Exception)
            {
                throw new AnalyzerException("Incorrect arguments.");
            }
        }
    }
}