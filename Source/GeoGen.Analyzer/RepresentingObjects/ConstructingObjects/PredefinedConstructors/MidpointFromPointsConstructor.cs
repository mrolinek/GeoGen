﻿using System.Collections.Generic;
using GeoGen.AnalyticGeometry;
using GeoGen.Core;

namespace GeoGen.Analyzer
{
    /// <summary>
    /// An <see cref="IObjectsConstructor"/> for <see cref="PredefinedConstructionType.MidpointFromPoints"/>>.
    /// </summary>
    public class MidpointFromPointsConstructor : PredefinedConstructorBase
    {
        /// <summary>
        /// Constructs a list of analytic objects from a given list of 
        /// flattened objects from the arguments and a container that is used to 
        /// obtain the actual analytic versions of these objects.
        /// </summary>
        /// <param name="flattenedObjects">The flattened argument objects.</param>
        /// <param name="container">The objects container.</param>
        /// <returns>The list of constructed analytic objects.</returns>
        protected override List<AnalyticObject> Construct(IReadOnlyList<ConfigurationObject> flattenedObjects, IObjectsContainer container)
        {
            // Pull points
            var point1 = container.Get<Point>(flattenedObjects[0]);
            var point2 = container.Get<Point>(flattenedObjects[1]);

            // Construct the result
            return new List<AnalyticObject> {point1.Midpoint(point2)};
        }

        /// <summary>
        /// Constructs a list of default theorems using a newly constructed objects and
        /// flattened objects from the passed arguments.
        /// </summary>
        /// <param name="input">The constructed objects.</param>
        /// <param name="flattenedObjects">The flattened argument objects.</param>
        /// <returns>The list of default theorems.</returns>
        protected override List<Theorem> FindDefaultTheorms(IReadOnlyList<ConstructedConfigurationObject> input, IReadOnlyList<ConfigurationObject> flattenedObjects)
        {
            return new List<Theorem>
            {
                // The midpoint is collinear with the two points
                new Theorem(TheoremType.CollinearPoints, new List<TheoremObject>
                {
                    new TheoremObject(flattenedObjects[0]),
                    new TheoremObject(flattenedObjects[1]),
                    new TheoremObject(input[0])
                }),
                // The midpoint is equally distanced from both of its endpoints
                new Theorem(TheoremType.EqualLineSegments, new List<TheoremObject>
                {
                    new TheoremObject(input[0]),
                    new TheoremObject(flattenedObjects[0]),
                    new TheoremObject(input[0]),
                    new TheoremObject(flattenedObjects[1])
                }),
            };
        }
    }
}