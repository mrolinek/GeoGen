using System.Collections.Generic;
using GeoGen.AnalyticalGeometry;
using GeoGen.Core.Configurations;
using GeoGen.Core.Utilities;

namespace GeoGen.Analyzer.Objects.GeometricalObjects.Container
{
    internal interface IContextualContainer
    {
        void Add(ConfigurationObject configurationObject);

        IEnumerable<T> GetNewObjects<T>(ConfigurationObjectsMap oldObjects, ConfigurationObjectsMap newObjects) where T : GeometricalObject;

        IEnumerable<T> GetObjects<T>(ConfigurationObjectsMap objects) where T : GeometricalObject;

        IAnalyticalObject GetAnalyticalObject(GeometricalObject geometricalObject, IObjectsContainer objectsContainer);
    }
}