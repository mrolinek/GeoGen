﻿using System;
using System.Linq;
using GeoGen.Utilities;
using GeoGen.Utilities.Helpers;

namespace GeoGen.AnalyticalGeometry.AnalyticalObjects
{
    /// <summary>
    /// Represents a geometrical 2D point.
    /// </summary>
    public class Point : IAnalyticalObject
    {
        private Lazy<int> _hashCode;

        #region Public properties

        /// <summary>
        /// Gets the X coordinate.
        /// </summary>
        public RoundedDouble X { get; }

        /// Gets the X coordinate.
        public RoundedDouble Y { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a point with given coordinates.
        /// </summary>
        /// <param name="x">c</param>
        /// <param name="y">/// Gets the X coordinate.</param>
        public Point(double x, double y)
        {
            RoundedDouble roundedX = x;
            RoundedDouble roundedY = y;

            X = roundedX;
            Y = roundedY;

            _hashCode = new Lazy<int>(() => (roundedX.GetHashCode() * 397) ^ roundedY.GetHashCode());
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Constructs the perpendicular of the line consisting of this
        /// point and a given other point. The other point must be
        /// distinct from this one.
        /// </summary>
        /// <param name="otherPoint">The other points.</param>
        /// <returns>The perpendicular bisector.</returns>
        public Line PerpendicularBisector(Point otherPoint)
        {
            // Check equality of this point and the other point
            if (this == otherPoint)
                throw new ArgumentException("Equal points");

            // First we get the midpoint between these two points
            // This is the first point lying on the desired line
            var midpoint = Midpoint(otherPoint);

            // Rotate this point around the midpoint by 90 degrees 
            // This is the second point lying on the desired line
            var rotatedPoint = Rotate(midpoint, 90);

            // We can construct the line from these two points
            return new Line(midpoint, rotatedPoint);
        }

        /// <summary>
        /// Rotates this point around a given center by a given angle. 
        /// </summary>
        /// <param name="center">The center of rotation.</param>
        /// <param name="angleInDegrees">The given angle in degrees.</param>
        /// <returns>The rotated point.</returns>
        public Point Rotate(Point center, double angleInDegrees)
        {
            // First we convert the angle to radians
            var angleInRadians = MathUtils.ToRadians(angleInDegrees);

            // Precalculate sin and cos of the angle
            var cosT = Math.Cos(angleInRadians);
            var sinT = Math.Sin(angleInRadians);

            // The general rotation matrix in 2D is 
            // 
            // cos(T)  -sin(T)
            // sin(T)   cos(T)
            //
            // To use this we first need to translate the point to the origin

            // Calculate the coordinates of the translated point
            var dx = X - center.X;
            var dy = Y - center.Y;

            // Now we use the matrix
            var newX = cosT * dx - sinT * dy;
            var newY = sinT * dx + cosT * dy;

            // And we use the reversed translation to get the result
            return new Point(newX + center.X, newY + center.Y);
        }

        /// <summary>
        /// Gets the midpoint of the line segment connecting
        /// this point with a given other point.
        /// </summary>
        /// <param name="otherPoint">The other point.</param>
        /// <returns>The midpoint.</returns>
        public Point Midpoint(Point otherPoint)
        {
            return (this + otherPoint) / 2;
        }

        /// <summary>
        /// Calculates the euclidean distance to a given other
        /// point.
        /// </summary>
        /// <param name="otherPoint">The other point.</param>
        /// <returns>The distance to the other point.</returns>
        public double DistanceTo(Point otherPoint)
        {
            var dx = X - otherPoint.X;
            var dy = Y - otherPoint.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        #endregion

        #region Overloaded operators

        /// <summary>
        /// Adds up two points by adding their corresponding coordinates.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>The sum of the points.</returns>
        public static Point operator +(Point point1, Point point2)
        {
            return new Point(point1.X + point2.X, point1.Y + point2.Y);
        }

        /// <summary>
        /// Scales a point by dividing the coordinates
        /// by a given factor.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="factor">The factor.</param>
        /// <returns>The scaled point.</returns>
        public static Point operator /(Point point, double factor)
        {
            return new Point(point.X / factor, point.Y / factor);
        }

        /// <summary>
        /// Scales a point by multiplying the coordinates
        /// by a given factor.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="factor">The factor.</param>
        /// <returns>The scaled point.</returns>
        public static Point operator *(Point point, double factor)
        {
            return new Point(point.X * factor, point.Y * factor);
        }

        /// <summary>
        /// Determines if two given points are equal.
        /// </summary>
        /// <param name="left">The left point.</param>
        /// <param name="right">The right point.</param>
        /// <returns>true, if they are equal, false otherwise.</returns>
        public static bool operator ==(Point left, Point right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines if two given points are not equal.
        /// </summary>
        /// <param name="left">The left point.</param>
        /// <param name="right">The right point.</param>
        /// <returns>true, if they are not equal, false otherwise.</returns>
        public static bool operator !=(Point left, Point right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Creates the perpendicular project of this point onto
        /// a given line. If this point lines on the line, the result
        /// will be the point itself.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>The projection.</returns>
        public Point Project(Line line)
        {
            // Get the perpendicular line
            var perpendicularLine = line.PerpendicularLine(this);

            // Intersect it with the given line
            var intersection = perpendicularLine.IntersectionWith(line);

            // Get the result. It definitely should exist if the 
            // line methods are implemented correctly
            var result = intersection ?? throw new Exception("Math has stopped working. The world is about to end.");

            // Return the result
            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Determines if this point is equal to a given other one.
        /// </summary>
        /// <param name="other">The other point.</param>
        /// <returns>true, if they are equal, false otherwise.</returns>
        private bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        #endregion

        #region Equals and hash code

        /// <summary>
        /// Determines if a given object is equal to this point.
        /// </summary>
        /// <param name="obj">A given object.</param>
        /// <returns>true, if they are equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (!(obj is Point point))
                return false;

            return Equals(point);
        }

        /// <summary>
        /// Gets the hash code of the point.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return _hashCode.Value;
            unchecked
            {
                //return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        #endregion

        public Line InternalAngelBisector(Point ray1Point, Point ray2Point)
        {
            // First we create the circle with center in [this] and radius
            // equal to distance between dist and first ray point
            var circle = new Circle(this, ray1Point.DistanceTo(this));

            // Second we create the circle connecting [this] and the second ray point
            var line = new Line(this, ray2Point);

            // Now we intersect them
            var intersections = circle.IntersectWith(line);

            // Logically we must have 2 intersections. We need to pick
            // the one lying on the ray from [this] to the second point

            // Generally, if we have a point C that lies on the line AB, then
            // there exists a real number 't' such that t = (C-A)/(B-A).
            // Point C then lies on the ray AB if and only if t >= 0 (if t=0, then C=A)
            // Since we know that C lies on AB, we just need to calculate the expression
            // for let's say x-coordinates. In our case: 
            //
            // A = [this]
            // B = ray2Point
            // C = one of intersections
            //
            // The intersection must logically exist
            var correctIntersection = intersections.First(intersection => (intersection.X - X) / (ray2Point.X - X) >= 0);

            // Now we just return the perpendicular bisector of the line 
            // connecting the first ray point with this correct intersection
            return ray1Point.PerpendicularBisector(correctIntersection);
        }
    }
}