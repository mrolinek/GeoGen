﻿using System;
using GeoGen.Core.Utilities;

namespace GeoGen.AnalyticalGeometry.AnalyticalObjects
{
    /// <summary>
    /// Represents a geometrical 2D line.
    /// </summary>
    public struct Line : IAnalyticalObject
    {
        #region Public properties

        /// <summary>
        /// Gets the A coefficient of the equation Ax + By + C = 0.
        /// </summary>
        public RoundedDouble A { get; }

        /// <summary>
        /// Gets the B coefficient of the equation Ax + By + C = 0.
        /// </summary>
        public RoundedDouble B { get; }

        /// <summary>
        /// Gets the C coefficient of the equation Ax + By + C = 0.
        /// </summary>
        public RoundedDouble C { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new line passing through given two points.
        /// These points can't be the same.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        public Line(Point point1, Point point2)
        {
            if (point1 == point2)
                throw new ArgumentException("Points can't be the same");

            // Calculate the coefficients of the direction vector
            var dx = point1.X - point2.X;
            var dy = point1.Y - point2.Y;

            // The pair (a,b) should be the normal vector, which is (dy, -dx)
            var a = dy;
            var b = -dx;

            // And the c coefficient is calculated so that the point1 lies on the line
            var c = -a * point1.X - b * point1.Y;

            // For any line they're infinitely many equations of the form Ax + By + C = 0.
            // In order for us to have the unique representation for each one, we would
            // want to have A^2 + B^2 + C^2 = 1 and A > 0 if A != 0 or B > 0 otherwise.
            // Then the representation will be unique

            // If a is not zero, we want it to be positive
            if ((RoundedDouble) a != 0)
            {
                // If it's not positive
                if (a < 0)
                {
                    // We multiply the whole equation by -1
                    a = -a;
                    b = -b;
                    c = -c;
                }
            }
            // Otherwise if a is 0, we want b to be positive
            else
            {
                // If b is negative
                if (b < 0)
                {
                    // We multiply the whole equation by -1
                    b = -b;
                    c = -c;
                }
            }

            // Now we can finally scale the coefficients so that A^2 + B^2 + C^2 = 1 holds true
            var scale = Math.Sqrt(a * a + b * b + c * c);

            A = a / scale;
            B = b / scale;
            C = c / scale;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Construct the intersection of this line with 
        /// another given one. If the lines are the same,
        /// the exception will be thrown. If the lines 
        /// are parallel, the null will be returned.
        /// </summary>
        /// <param name="otherLine">The other line.</param>
        /// <returns>The intersection, or null, if there isn't any.</returns>
        public Point? IntersectionWith(Line otherLine)
        {
            if (this == otherLine)
                throw new ArgumentException("Equal lines");

            // We want to solve the system
            //
            // a1x + b1y + c1 = 0     (1)
            // a2x + b2y + c2 = 0     (2)
            //
            // Let's have a look at the equations
            // 
            // a2 (1) - a1 (2)
            // b1 (2) - b2 (1)
            // 
            // They give us
            //
            // y (a2b1 - a1b2) + c1a2 - c2a1 = 0
            // x (a2b1 - a1b2) + c2b1 - c1b2 = 0

            // Pull the coefficients
            var a1 = A;
            var b1 = B;
            var c1 = C;

            var a2 = otherLine.A;
            var b2 = otherLine.B;
            var c2 = otherLine.C;

            // Calculate the common delta from the last 2 equations
            var delta = a2 * b1 - a1 * b2;

            // If it's 0, then the lines are either parallel, or equal.
            // But we know they're not equal.
            if ((RoundedDouble) delta == 0)
                return null;

            // Otherwise we simply solve the simple linear equations
            var x = (c1 * b2 - c2 * b1) / delta;
            var y = (c2 * a1 - c1 * a2) / delta;

            // And construct the result
            return new Point(x, y);
        }

        /// <summary>
        /// Determines if a given point lies on this line.
        /// </summary>
        /// <param name="point">The given point.</param>
        /// <returns>true, if the point lies on the line, false otherwise.</returns>
        public bool Contains(Point point)
        {
            // We simply check if the point's coordinates meets the equation
            return (RoundedDouble) (A * point.X + B * point.Y + C) == 0;
        }

        /// <summary>
        /// Creates a line that is perpendicular to this one and passes through a
        /// given point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The perpendicular line.</returns>
        public Line PerpendicularLine(Point point)
        {
            // Math suggests that the directional vector of the result
            // will be (A,B). We have one point on the line, [x,y]
            // the other could be [x+A, y+B] (they will not be the same 
            // because either A != 0, or B != 0). Then we can simply
            // use the constructor for line from 2 points

            // Create the other point
            var otherPoint = new Point(point.X + A, point.Y + B);

            // Create the line from 2 points
            return new Line(point, otherPoint);
        }

        #endregion

        #region Overloaded operators

        /// <summary>
        /// Determines if two given lines are equal.
        /// </summary>
        /// <param name="left">The left line.</param>
        /// <param name="right">The right line.</param>
        /// <returns>true, if they are equal, false otherwise.</returns>
        public static bool operator ==(Line left, Line right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines if two given lines are not equal.
        /// </summary>
        /// <param name="left">The left line.</param>
        /// <param name="right">The right line.</param>
        /// <returns>true, if they are not equal, false otherwise.</returns>
        public static bool operator !=(Line left, Line right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Determines if this line is equal to a given other one.
        /// </summary>
        /// <param name="other">The other line.</param>
        /// <returns>true, if they are equal, false otherwise.</returns>
        private bool Equals(Line other)
        {
            return A == other.A && B == other.B && C == other.C;
        }

        #endregion

        #region Equals and HashCode

        /// <summary>
        /// Determines if a given object is equal to this line.
        /// </summary>
        /// <param name="obj">A given object.</param>
        /// <returns>true, if they are equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (!(obj is Line line))
                return false;

            return Equals(line);
        }

        /// <summary>
        /// Gets the hash code of the line.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = A.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                hashCode = (hashCode * 397) ^ C.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}