﻿using GeoGen.AnalyticGeometry;
using GeoGen.Core;
using System;
using System.Linq;

namespace GeoGen.Constructor
{
    /// <summary>
    /// The default implementation of <see cref="IComposedConstructor"/> that 
    /// receives a <see cref="ComposedConstruction"/> as a constructor parameter. 
    /// </summary>
    public class ComposedConstructor : ObjectsConstructorBase, IComposedConstructor
    {
        #region Dependencies

        /// <summary>
        /// The resolver of constructors used while constructing the internal configuration of the composed construction.
        /// </summary>
        private readonly IConstructorsResolver _constructionResolver;

        /// <summary>
        /// The factory for creating pictures in which we're constructing the internal configuration of the composed construction.
        /// </summary>
        private readonly IPictureFactory _pictureFactory;

        #endregion

        #region Private fields

        /// <summary>
        /// The composed construction performed by the constructor.
        /// </summary>
        private readonly ComposedConstruction _construction;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ComposedConstructor"/> class.
        /// </summary>
        /// <param name="construction">The composed construction performed by the constructor.</param>
        /// <param name="constructionResolver">The resolver of constructors used while constructing the internal configuration of the composed construction.</param>
        /// <param name="picturesFactory">The factory for creating pictures in which we're constructing the internal configuration of the composed construction.</param>
        public ComposedConstructor(ComposedConstruction construction, IConstructorsResolver constructionResolver, IPictureFactory picturesFactory)
        {
            _construction = construction ?? throw new ArgumentNullException(nameof(construction));
            _constructionResolver = constructionResolver ?? throw new ArgumentNullException(nameof(constructionResolver));
            _pictureFactory = picturesFactory ?? throw new ArgumentNullException(nameof(picturesFactory));
        }

        #endregion

        #region ObjectsConstructorBase implementation

        /// <summary>
        /// Performs the actual construction of an analytic object based on the analytic objects given as an input.
        /// The order of the objects of the input is based on the <see cref="Arguments.FlattenedList"/>.
        /// </summary>
        /// <param name="input">The analytic objects to be used as an input.</param>
        /// <returns>The constructed analytic object, if the construction was successful; or null otherwise.</returns>
        protected override IAnalyticObject Construct(IAnalyticObject[] input)
        {
            // Initialize an internal picture in which we're going to construct
            // the configuration that defines our composed construction
            var internalPicture = _pictureFactory.CreatePicture();

            // Pull the loose objects of this configuration
            var looseObjects = _construction.Configuration.LooseObjectsHolder.LooseObjects;

            // Add these objects to the internal picture
            // Their analytic versions should correspond to the passed input
            internalPicture.Add(looseObjects, () => input.ToList());

            // Add the constructed objects as well
            foreach (var constructedObject in _construction.Configuration.ConstructedObjects)
            {
                // For each one create the construction function
                var constructorFunction = _constructionResolver.Resolve(constructedObject.Construction).Construct(constructedObject);

                // Add the object to the picture using this function that gets passed the internal picture
                internalPicture.TryAdd(constructedObject, () => constructorFunction(internalPicture), out var objectConstructed, out var equalObject);

                // Find out if we have a correct result
                var correctResult = objectConstructed && equalObject == null;

                // If not, the construction failed
                if (!correctResult)
                    return null;
            }

            // If we are here, then the construction should be fine and the result
            // will be in the internal picture corresponding to the last object 
            // of the configuration that defines our composed construction
            return internalPicture.Get(_construction.Configuration.ConstructedObjects.Last());
        }

        #endregion
    }
}