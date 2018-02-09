﻿using GeoGen.Core;

namespace GeoGen.Analyzer
{
    /// <summary>
    /// Represents a geometrical object that is aware of other geometrical objects
    /// in a single configuration. The main goal is to have a line or a circle
    /// that know all configuration points that it contains.
    /// </summary>
    internal abstract class GeometricalObject
    {
        #region Public properties

        /// <summary>
        /// Gets the id of the geometrical object.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the configuration object that represents this object. It could
        /// be null, if this is a line or a circle and its defined by points.
        /// </summary>
        public ConfigurationObject ConfigurationObject { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="id">The id of the object.</param>
        /// <param name="configurationObject">The configuration object that represents the object.</param>
        protected GeometricalObject(int id, ConfigurationObject configurationObject)
        {
            ConfigurationObject = configurationObject;
            Id = id;
        }

        #endregion

        #region Equals and hash code

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return ((GeometricalObject) obj).Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        #endregion
    }
}