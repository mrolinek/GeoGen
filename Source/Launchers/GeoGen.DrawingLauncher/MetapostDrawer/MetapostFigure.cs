﻿using GeoGen.AnalyticGeometry;
using GeoGen.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GeoGen.AnalyticGeometry.AnalyticHelpers;
using static GeoGen.AnalyticGeometry.MathHelpers;
using static System.Math;

namespace GeoGen.DrawingLauncher
{
    /// <summary>
    /// Represents a MetaPost figure accepts things to be drawn via Add methods and then is able
    /// to generate the actual MetaPost code that would perform the drawing. 
    /// </summary>
    public class MetapostFigure
    {
        #region Private fields

        /// <summary>
        /// The dictionary mapping objects to the labels that have been assigned.
        /// </summary>
        private readonly Dictionary<IAnalyticObject, string> _labels = new Dictionary<IAnalyticObject, string>();

        /// <summary>
        /// The dictionary mapping points to be marked to the styles that are supposed to be used.
        /// </summary>
        private readonly Dictionary<Point, ObjectDrawingStyle> _pointStyles = new Dictionary<Point, ObjectDrawingStyle>();

        /// <summary>
        /// The dictionary mapping circles to be drawn to the styles that are supposed to be used.
        /// </summary>
        private readonly Dictionary<Circle, ObjectDrawingStyle> _circleStyles = new Dictionary<Circle, ObjectDrawingStyle>();

        /// <summary>
        /// The dictionary mapping lines to be drawn to the styles that are supposed to be used.
        /// </summary>
        private readonly Dictionary<Line, ObjectDrawingStyle> _lineStyles = new Dictionary<Line, ObjectDrawingStyle>();

        /// <summary>
        /// The dictionary mapping lines to the points that are supposed to visually lie on them.
        /// </summary>
        private readonly Dictionary<Line, List<Point>> _linePoints = new Dictionary<Line, List<Point>>();

        /// <summary>
        /// The dictionary mapping lines to the points that are supposed to visually lie on them and be shifted at them a bit.
        /// </summary>
        private readonly Dictionary<Line, List<Point>> _shiftLinePoints = new Dictionary<Line, List<Point>>();

        /// <summary>
        /// The dictionary mapping lines to segments with their styles that are part of them.
        /// </summary>
        private readonly Dictionary<Line, List<(Point, Point, ObjectDrawingStyle)>> _lineSegments = new Dictionary<Line, List<(Point, Point, ObjectDrawingStyle)>>();

        /// <summary>
        /// The dictionary mapping lines to shifted segments with their styles that are part of them.
        /// </summary>
        private readonly Dictionary<Line, List<(Point, Point, ObjectDrawingStyle)>> _lineShiftedSegments = new Dictionary<Line, List<(Point, Point, ObjectDrawingStyle)>>();

        /// <summary>
        /// The text to be added to the right of the picture.
        /// </summary>
        private string _text;

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a given point with a given style to the figure.
        /// </summary>
        /// <param name="point">The point to be drawn.</param>
        /// <param name="style">The style to be used while drawing.</param>
        public void AddPoint(Point point, ObjectDrawingStyle style)
        {
            // If it's not there
            if (!_pointStyles.ContainsKey(point))
            {
                // Then simply add it
                _pointStyles.Add(point, style);

                // We're done
                return;
            }

            // Otherwise it's there. If we got a higher style, then override it
            if (style > _pointStyles[point])
            {
                // Remove it
                _pointStyles.Remove(point);

                // Add it with this higher style
                _pointStyles.Add(point, style);
            }
        }

        /// <summary>
        /// Adds a given line to be drawn with a given style. It can be specified which points are supposed
        /// to visually on the line when it's drawn. The line can be also drawn to be shifted at them.
        /// </summary>
        /// <param name="line">The line to be drawn.</param>
        /// <param name="points">The points that are supposed to visually lie on the line.</param>
        /// <param name="style">The style to be used while drawing.</param>
        /// <param name="shifted">Indicates whether the line should be shifted around the <paramref name="points"/>.</param>
        public void AddLine(Line line, Point[] points, ObjectDrawingStyle style, bool shifted)
        {
            // Make sure the points lie on the line
            if (points.Any(point => !LiesOn(line, point)))
                throw new DrawingLauncherException("There is a line that should have contained the passed point, but it did not.");

            // Handle the points
            points.ForEach(point =>
                // Get the right dictionary according to the fact whether the line should be shifted around points
                (shifted ? _shiftLinePoints : _linePoints)
                // Make sure it contains the line and the corresponding list the point
                .GetValueOrCreateNewAddAndReturn(line).Add(point));

            // Make sure the line's style is added to the lines dictionary
            // If it's not there...
            if (!_lineStyles.ContainsKey(line))
            {
                // Then simply add it
                _lineStyles.Add(line, style);

                // We're done
                return;
            }

            // Otherwise it's there. If we got a higher style, then override it
            if (style > _lineStyles[line])
            {
                // Remove it
                _lineStyles.Remove(line);

                // Add it with this higher style
                _lineStyles.Add(line, style);
            }
        }

        /// <summary>
        /// Adds a given line to be drawn with a given style. 
        /// </summary>
        /// <param name="circle">The circle to be drawn.</param>
        /// <param name="style">The style to be used while drawing.</param>
        public void AddCircle(Circle circle, ObjectDrawingStyle style)
        {
            // If it's not there
            if (!_circleStyles.ContainsKey(circle))
            {
                // Then simply add it
                _circleStyles.Add(circle, style);

                // We're done
                return;
            }

            // Otherwise it's there. If we got a higher style, then override it
            if (style > _circleStyles[circle])
            {
                // Remove it
                _circleStyles.Remove(circle);

                // Add it with this higher style
                _circleStyles.Add(circle, style);
            }
        }

        /// <summary>
        /// Adds a given segment to be drawn with a given style. It can be specified whether the segment 
        /// should be shifted at the second point.
        /// </summary>
        /// <param name="point1">The first point of the segment.</param>
        /// <param name="point2">The second point of the segment.</param>
        /// <param name="style">The style to be used while drawing.</param>
        /// <param name="shifted">Indicates whether the segment should be shifted at the second point.</param>
        public void AddSegment(Point point1, Point point2, ObjectDrawingStyle style, bool shifted)
        {
            // Get the right dictionary where we're going to add the segment based on the shift state
            (shifted ? _lineShiftedSegments : _lineSegments)
                // Make sure the line is in the dictionary and add the segment
                .GetValueOrCreateNewAddAndReturn(new Line(point1, point2)).Add((point1, point2, style));
        }

        /// <summary>
        /// Adds a label of a given object to the figure.
        /// </summary>
        /// <param name="analyticObject">The object to be labeled.</param>
        /// <param name="label">The label of the object.</param>
        public void AddLabel(IAnalyticObject analyticObject, string label)
        {
            // If the object is already there, make aware
            if (_labels.ContainsKey(analyticObject))
                throw new DrawingLauncherException("Cannot label the same object twice.");

            // Otherwise add the label
            _labels.Add(analyticObject, label);
        }

        /// <summary>
        /// Adds the text. The text will be displayed to the right of the picture. It will be rendered with TeX,
        /// i.e. it might (and should) include TeX commands.
        /// </summary>
        /// <param name="text">The text to be displayed.</param>
        public void AddText(string text) => _text = text;

        /// <summary>
        /// Generates a code of the picture using the drawing data holding necessary MetaPost-related information to do so.
        /// </summary>
        /// <param name="drawingData">The data for drawing holding for example names of needed macros.</param>
        /// <returns>The MetaPost code of the figure.</returns>
        public string ToCode(MetapostDrawingData drawingData)
        {
            // Helper function that converts a point to a string readable by MetaPost
            string ConvertPoint(Point point) =>
                // Include the scaling variable and both coordinates
                $"{drawingData.ScaleVariable}*({point.X.ToStringReadableByMetapost()},{point.Y.ToStringReadableByMetapost()})";

            // Prepare the resulting code
            var code = new StringBuilder();

            #region Lines

            // Clone the line segments dictionary so we can safely add things to this cloned one
            var segments = _lineSegments.ToDictionary(pair => pair.Key, pair => new List<(Point, Point, ObjectDrawingStyle)>(pair.Value));

            #region Extend shifted segments

            // Go through the shifted segments...
            _lineShiftedSegments.ForEach(pair =>
            {
                // Deconstruct
                var (line, shiftedSegments) = pair;

                // Make sure this line is in the new dictionary and get its segments
                var currentSegments = segments.GetValueOrCreateNewAddAndReturn(line);

                // Shift each segment
                shiftedSegments.ForEach(triple =>
                {
                    // Deconstruct
                    var (A, B, style) = triple;

                    // Get the shift value
                    var shift = drawingData.ShiftLength;

                    // Shift
                    var C = ShiftSegment(A, B, shift);

                    // Our extended segment is AC and inherits the style
                    currentSegments.Add((A, C, style));
                });
            });

            #endregion

            #region Prepare segments for all lines

            // Prepare the lines that we're going to handle by taking ones that have a segment
            // and adding those that are stated to be drawn explicitly. We're going to draw each
            segments.Keys.Union(_lineStyles.Keys).ForEach(line =>
            {
                // We need to be able to sort points on lines, so we create a simple comparer that does it
                // The 'smallest' point will be the topmost leftmost one and the 'largest' will be the bottommost rightmost one.
                var comparer = Comparer<Point>.Create((point1, point2) =>
                {
                    // If there the first point is more on the left than the other, then it's 'smaller'
                    if (point1.X.Rounded() < point2.X.Rounded())
                        return -1;

                    // Or the other way around
                    if (point1.X.Rounded() > point2.X.Rounded())
                        return 1;

                    // Otherwise they lie on the same vertical lie
                    // We say if the first one is more at the top, then it's 'smaller'
                    if (point1.Y.Rounded() > point2.Y.Rounded())
                        return -1;

                    // Or the other way around
                    if (point1.Y.Rounded() < point2.Y.Rounded())
                        return 1;

                    // Or they're the same
                    return 0;
                });

                // We're going to keep our points in a SortedList
                var segmentPoints = new SortedList<Point>(comparer);

                // We also need to maintain a list of styles of particular segments that we have
                // Each segment has a starting point that is mapped to the style of the segment
                // The null value for a style indicates that the segment is not drawn at all
                var segmentStyles = new Dictionary<Point, ObjectDrawingStyle?>();

                // Prepare an empty list of segments to be drawn
                var segmentsToDraw = new List<(Point, Point, ObjectDrawingStyle)>();

                // If there are segments of this line, add them to be drawn
                if (segments.ContainsKey(line))
                    segmentsToDraw.AddRange(segments[line]);

                #region Creating a segment from passing points

                // First find the points that are supposed to visually lie on this line
                var passingPoints = (_linePoints.GetValueOrDefault(line) ?? Enumerable.Empty<Point>())
                        // Append those of them that are shifted (the shift will be resolved later)
                        .Concat(_shiftLinePoints.GetValueOrDefault(line) ?? Enumerable.Empty<Point>())
                        // Take distinct 
                        .Distinct()
                        // Sort them
                        .OrderBy(point => point, comparer)
                        // Enumerate
                        .ToArray();

                // If there are at least two of them... 
                if (passingPoints.Length >= 2)
                {
                    // Then the first (leftmost) and last (rightmost) make a segment
                    var left = passingPoints[0];
                    var right = passingPoints[^1];

                    // Some of them might be shifted, we need to check that
                    var isLeftShifted = _shiftLinePoints.GetValueOrDefault(line)?.Contains(left) ?? false;
                    var isRightShifted = _shiftLinePoints.GetValueOrDefault(line)?.Contains(right) ?? false;

                    // If the left is shifted, do the shift
                    if (isLeftShifted)
                        left = ShiftSegment(right, left, drawingData.ShiftLength);

                    // If the right is shifted, do the shift
                    if (isRightShifted)
                        right = ShiftSegment(left, right, drawingData.ShiftLength);

                    // Add the segment
                    segmentsToDraw.Add((left, right, _lineStyles[line]));
                }
                // Otherwise we want to construct the line across to the pictures, but only if 
                // it has a style, i.e. it is even requested to be drawn (it might happen 
                // that we're drawing just some segments of this line)
                else if (_lineStyles.ContainsKey(line))
                {
                    // In order to make the line to go across the picture, we need the bounding box of 
                    // points and circles. We need to find the relevant points that are in the picture
                    // after taking into account shifted segments
                    var points = segments.Values.Flatten()
                        // Include every segment
                        .SelectMany(triple => triple.Item1.ToEnumerable().Concat(triple.Item2));

                    // Prepare the bounding box
                    var intersections = new BoundingBox(points, _circleStyles.Keys)
                        // Intersect it with our line
                        .IntersectWith(line)
                        // Order by our comparer
                        .OrderBy(point => point, comparer)
                        // Enumerate
                        .ToArray();

                    // There should be two of them
                    if (intersections.Length != 2)
                        throw new AnalyticException("Analytic geometric must be flawed, cannot determine intersections with the bounding box correctly");

                    // If we have two of them, we can create the segment from them
                    segmentsToDraw.Add((intersections[0], intersections[1], _lineStyles[line]));
                }

                #endregion

                #region Preparing the final non-overlapping segments 

                // Go through the prepared segments to draw
                segmentsToDraw.ForEach(triple =>
                {
                    // Deconstruct
                    var (point1, point2, segmentStyle) = triple;

                    // Order them
                    var newLeftPoint = comparer.Compare(point1, point2) < 0 ? point1 : point2;
                    var newRightPoint = comparer.Compare(point1, point2) < 0 ? point2 : point1;

                    // If this is the first segment...
                    if (segmentPoints.IsEmpty())
                    {
                        // Then add the points
                        segmentPoints.TryAdd(newLeftPoint);
                        segmentPoints.TryAdd(newRightPoint);

                        // Add the segment with the style
                        segmentStyles.Add(newLeftPoint, segmentStyle);

                        // We're done for now
                        return;
                    }

                    // At this point we're sure this is not the first segment being drawn on this line
                    // Therefore there is the leftmost and the rightmost point. Find them
                    var leftmostPoint = segmentPoints[0];
                    var rightmostPoint = segmentPoints[^1];

                    // Make sure the points are added to the points of the already segmented line
                    segmentPoints.TryAdd(newLeftPoint);
                    segmentPoints.TryAdd(newRightPoint);

                    #region Reevaluating individual segment styles

                    // We're going to rebuild the styles
                    var newSegmentStyles = new Dictionary<Point, ObjectDrawingStyle?>();

                    // Go through the segments
                    for (var i = 0; i < segmentPoints.Count - 1; i++)
                    {
                        // Get the left and right point
                        var leftPoint = segmentPoints[i];
                        var rightPoint = segmentPoints[i + 1];

                        // It's going to be useful to know whether this segment is part of the segment being inserted
                        // This happens when the new one has its left point to the right (or equal)
                        var isThisSubsegmentOfNewOne = comparer.Compare(newLeftPoint, leftPoint) <= 0
                            // And its left point to the left (or equal)
                            && comparer.Compare(rightPoint, newRightPoint) <= 0;

                        // If the segment has a style...
                        if (segmentStyles.ContainsKey(leftPoint))
                        {
                            // Get the style 
                            var currentStyle = segmentStyles[leftPoint];

                            // This style might be overridden by the current style, if this 
                            // segment is contained in the one being inserted
                            var shouldWeOverrideStyle = isThisSubsegmentOfNewOne
                                // And is higher than the current one
                                && (currentStyle == null || segmentStyle > currentStyle.Value);

                            // If we should override the style, do it
                            if (shouldWeOverrideStyle)
                                currentStyle = segmentStyle;

                            // Add the style to the new dictionary
                            newSegmentStyles.Add(leftPoint, currentStyle);

                            // And we're done
                            continue;
                        }

                        // If we got here, the segment doesn't have a style yet. These options are possible:
                        // 
                        // 1. It is the new segment being inserted so that it doesn't overlap any other segment. 
                        // 2. It is an empty segment that got inserted when the new segment has been inserted to the very right or left.
                        // 3. It is a segment that was created by splitting an existing segment.
                        //
                        // Let us handle these cases individually
                        // 
                        // The first case happens when the current segment is to the left of the initially leftmost one
                        var firstCaseHappened = comparer.Compare(rightPoint, leftmostPoint) <= 0
                            // Or the current segment is to the right of the initially rightmost one
                            || comparer.Compare(rightmostPoint, leftPoint) <= 0;

                        // If it happened...
                        if (firstCaseHappened)
                        {
                            // Then the segment gets the style it's been assigned to it
                            newSegmentStyles.Add(leftPoint, segmentStyle);

                            // And we're done
                            continue;
                        }

                        // The second case happens either on the left, when the originally leftmost point is the current right point
                        var secondCaseHappened = leftmostPoint == rightPoint
                            // Or on the right, where the originally rightmost point is the current left one
                            || rightmostPoint == leftPoint;

                        // If it happened...
                        if (secondCaseHappened)
                        {
                            // Then the segment isn't drawn and gets 'null' style
                            newSegmentStyles.Add(leftPoint, null);

                            // And we're done
                            continue;
                        }

                        // We're left with the third case. In this case we need to have 
                        // a look at the segment that got split in order for us to get this 
                        // segment in the first place. We know it exists at the left index
                        var originalStyle = segmentStyles[segmentPoints[i - 1]];

                        // This style might get changed if our split part is contained in it
                        var shouldWeOverrideOriginalStyle = isThisSubsegmentOfNewOne
                            // And is higher than the current one
                            && (originalStyle == null || segmentStyle > originalStyle.Value);

                        // If we should override the style, do it
                        if (shouldWeOverrideOriginalStyle)
                            originalStyle = segmentStyle;

                        // We finally know the style for this segment
                        newSegmentStyles.Add(leftPoint, originalStyle);
                    }

                    // Now we can finally override the current segment styles
                    segmentStyles = newSegmentStyles;

                    #endregion
                });

                #endregion

                #region Write drawing commands

                // Go through the points starting segments
                segmentStyles.ForEach(pair =>
                {
                    // Deconstruct
                    var (leftPoint, style) = pair;

                    // If the style is null, i.e. this is not a segment to be drawn, we're done
                    if (style == null)
                        return;

                    // Otherwise get the right point for the segment, i.e. the one after left
                    var rightPoint = segmentPoints[segmentPoints.IndexOf(leftPoint) + 1];

                    // Get the macro name
                    var macroName = drawingData.LineSegmentMacros.GetValueOrDefault(style.Value)
                        // Make sure it's known when it's not present
                        ?? throw new DrawingLauncherException($"The style {style} doesn't have its macro defined for drawing line segments.");

                    // Append the drawing command
                    code.AppendLine($"draw {macroName}({ConvertPoint(leftPoint)}, {ConvertPoint(rightPoint)});");
                });

                #endregion
            });

            #endregion

            #endregion

            #region Circles

            // Go through the circles
            _circleStyles.ForEach(pair =>
            {
                // Deconstruct
                var (circle, style) = pair;

                // Get the macro name
                var macroName = drawingData.CircleMacros.GetValueOrDefault(style)
                    // Make sure it's known when it's not present
                    ?? throw new DrawingLauncherException($"The style {style} doesn't have its macro defined for drawing circles.");

                // Use the macro to draw the circle
                code.AppendLine($"draw {macroName}({ConvertPoint(circle.Center)}, {drawingData.ScaleVariable}*{circle.Radius.ToStringReadableByMetapost()});");
            });

            #endregion

            #region Point marks

            // Go through the points
            _pointStyles.ForEach(pair =>
            {
                // Deconstruct
                var (point, style) = pair;

                // Get the macro name
                var macroName = drawingData.PointMarkMacros.GetValueOrDefault(style)
                    // Make sure it's known when it's not present
                    ?? throw new DrawingLauncherException($"The style {style} doesn't have its macro defined for marking points.");

                // Use the macro to mark the point
                code.AppendLine($"draw {macroName}({ConvertPoint(point)});");
            });

            #endregion

            #region Labels

            // Go through the labels
            _labels.ForEach(pair =>
            {
                // Deconstruct
                var (analyticObject, label) = pair;

                // Switch based on the object type
                switch (analyticObject)
                {
                    // Point case
                    case Point point:

                        // Use the macro to do the drawing of the label
                        code.AppendLine($"draw {drawingData.PointLabelMacro}(\"{label}\", {ConvertPoint(point)});");

                        break;

                    // These cases are currently not supported
                    case Line _:
                    case Circle _:
                        break;

                    // Unhandled cases
                    default:
                        throw new DrawingLauncherException($"Unhandled type of {nameof(IAnalyticObject)}: {analyticObject.GetType()}");
                }
            });

            #endregion

            #region Clip large circles

            // Find the bounding box without circles scaled by the scale from settings
            var boudingBoxWithoutCircles = new BoundingBox(_pointStyles.Keys).Scale(drawingData.PointBoundingBoxScale);

            // Find the bounding box of the entire figure, i.e. points, including the 
            // points of the accepted circle-free bounding box, including the circles
            var boudingBoxWithCircles = new BoundingBox(_pointStyles.Keys.Concat(boudingBoxWithoutCircles.BoundaryPoints), _circleStyles.Keys)
                // We need to scale it by a small value so we don't accidentally
                // clip a circle by a point, which may cause visual glitches
                .Scale(1.01);

            // Find out if we are going to clip the width and height based on the 
            // provided accepted thresholds
            var areWeClippingWidth = boudingBoxWithCircles.Width > drawingData.MinimalWidthForClipping;
            var areWeClippingHeight = boudingBoxWithCircles.Height > drawingData.MinimalHeightForClipping;

            // If we are going to be clipping
            if (areWeClippingWidth || areWeClippingHeight)
            {
                // Get the center of the smaller box
                var innerBoxCenter = boudingBoxWithoutCircles.Center;

                // Get the projections of the center on the inner boundary
                var leftInnerProjection = innerBoxCenter.Project(boudingBoxWithoutCircles.LeftLine);
                var righInnerProjection = innerBoxCenter.Project(boudingBoxWithoutCircles.RightLine);
                var upperInnerProjection = innerBoxCenter.Project(boudingBoxWithoutCircles.UpperLine);
                var bottomInnerProjection = innerBoxCenter.Project(boudingBoxWithoutCircles.BottomLine);

                // Get the projections of the center on the outer boundary
                var leftOuterProjection = innerBoxCenter.Project(boudingBoxWithCircles.LeftLine);
                var rightOuterProjection = innerBoxCenter.Project(boudingBoxWithCircles.RightLine);
                var upperOuterProjection = innerBoxCenter.Project(boudingBoxWithCircles.UpperLine);
                var bottomOuterProjection = innerBoxCenter.Project(boudingBoxWithCircles.BottomLine);

                // We will be moving the inner projections towards the outer ones. Each direction will be
                // tested for a fixed number of moves, and they will be combined in every way (i.e. O(n^4))
                // In practice, we do not need lots of iterations
                const int iterations = 10;

                // We will figure out what clipping boxes are valid. We're generally moving in 4 directions
                var validClipBoxes = Enumerable.Range(0, 4)
                    // For each direction we will decide whether we're moving or not
                    // First two directions are left and right, i.e. we're clipping width in that direction
                    // The second two directions are up and down, i.e. we're clipping height
                    .Select(directionId => (directionId < 2 && areWeClippingWidth) || (directionId >= 2 && areWeClippingHeight)
                        // When we're clipping, we need numbers 0, 1, ..., iterations - 1
                        ? Enumerable.Range(0, iterations).Select(i => (double)i)
                        // When we're not clipping, we need 'iterations' 
                        : ((double)iterations).ToEnumerable())
                    // Make every possible quadruple of these values (iterations^4 results)
                    .Combine()
                    // Construct the corresponding box for every combination
                    .Select(values =>
                    {
                        // Shift the inner projections in the direction of the outer ones
                        var shiftedLeftProjection = leftInnerProjection + values[0] / iterations * (leftOuterProjection - leftInnerProjection);
                        var shiftedRightProjection = righInnerProjection + values[1] / iterations * (rightOuterProjection - righInnerProjection);
                        var shiftedUpperProjection = upperInnerProjection + values[2] / iterations * (upperOuterProjection - upperInnerProjection);
                        var shiftedBottomProjection = bottomInnerProjection + values[3] / iterations * (bottomOuterProjection - bottomInnerProjection);

                        // Create a new box enclosing them
                        return new BoundingBox(shiftedLeftProjection, shiftedRightProjection, shiftedBottomProjection, shiftedUpperProjection);
                    })
                    // Take only valid clipping boxes
                    .Where(currentCutBox => _circleStyles.Keys.All(circle =>
                    {
                        // First find the intersection points of the circle with the box
                        var intersections = currentCutBox.IntersectWith(circle);

                        // If there are 0 intersection points, we're cool. The circle is inside
                        if (intersections.Length == 0)
                            return true;

                        // If there is 1, then the circle is tangent, clipping might look weird
                        // If there are at least 3, then clipping is not acceptable, there will be 
                        // a weird fragment left. In other words, we need to have 2 to judge further
                        if (intersections.Length != 2)
                            return false;

                        // Two intersection mean that the circle will be clipped without a hanging fragment
                        // But it might happen that we clip a very small portion of the circles and it
                        // looks weird. To prevent that we will have a look at the angle of the clipped part

                        // Let's name the intersections for comfort
                        var A = intersections[0];
                        var B = intersections[1];

                        // We will calculate the midpoint of the smaller arc AB of our circle. That
                        // will be used to detect whether this smaller arc is the one that was clipped, or
                        // the other one. We could use some vectors. We can use points to simulate them
                        var aCenter = A - circle.Center;
                        var bCenter = B - circle.Center;

                        // If we sum (A-center) and (B-center), we will get a vector passing
                        // through the desired midpoint
                        var centerMidpoint = aCenter + bCenter;

                        // If the length of this vector is 'l' and the radius of the circle is 'r',
                        // then the needed midpoint is exactly center + r / l * centerMidpointVector
                        var arcMidpoint = circle.Center + circle.Radius / centerMidpoint.DistanceToOrigin * centerMidpoint;

                        // We will find the angle between vectors (A-center) and (B-center), 
                        // using the standard formula arccos( u.v / (||u|| * ||v||) )
                        var arcAngle = ToDegrees(Acos((aCenter.X * bCenter.X + aCenter.Y * bCenter.Y) / (aCenter.DistanceToOrigin * bCenter.DistanceToOrigin)));

                        // Finally we can find the actual angle corresponding to the cut arc
                        // If the midpoint of our arc is inside the box
                        var cutArcAngle = currentCutBox.IsPointWithinTheBox(arcMidpoint)
                            // Then the answer is the other angle
                            ? 360 - arcAngle
                            // Otherwise it's our angle
                            : arcAngle;

                        // Now, the current box cuts our circle nicely if and only if the angle
                        // is at least our defined threshold
                        return cutArcAngle >= drawingData.MinimalAngleOfClippedCircleArc;
                    }))
                    // Enumerate
                    .ToArray();

                // If there is a valid box
                if (validClipBoxes.Any())
                {
                    // Find the 'smallest' one, which could be detected by the area
                    var finalClipBox = validClipBoxes.MinItem(box => box.Area);

                    // Append a simple piece of MetaPost clipping code to the final code
                    code.AppendLine($"clip currentpicture to" +
                        $"    {ConvertPoint(finalClipBox.UpperLeftCorner)}" +
                        $" -- {ConvertPoint(finalClipBox.UpperRightCorner)}" +
                        $" -- {ConvertPoint(finalClipBox.BottomRightCorner)}" +
                        $" -- {ConvertPoint(finalClipBox.BottomLeftCorner)}" +
                        $" -- cycle;");
                }
            }

            #endregion

            #region Text

            // If there is text to be included, do so via the provided macro
            if (!string.IsNullOrEmpty(_text))
                code.AppendLine($"draw {drawingData.TextMacro}({_text});");

            #endregion

            // Return the resulting code
            return code.ToString();
        }

        /// <summary>
        /// Calculates a heuristic numeric evaluation of the badness of the figure. The higher the ranking, the worse the figure.
        /// </summary>
        /// <returns>The badness ranking from the interval [0, MaxValue].</returns>
        public double CalculateVisualBadness()
        {
            // Prepare the value of total badness that will be accumulated
            var totalBadness = 0d;

            // Go through the pairs of points in the figure
            foreach (var (point1, point2) in _pointStyles.Keys.ToArray().UnorderedPairs())
            {
                // Calculate the distance between the current pair
                var distance = point1.DistanceTo(point2);

                try
                {
                    // Based on the distance decide which badness function should be used
                    var localBadness = distance < 1
                        // For distances smaller than 1 a good function is -log(x), because its limit
                        // when x-->0 is +infinity and it increases very fast. This corresponds to 
                        // the fact that we don't want to have points that are too close to each other
                        ? -Log(distance)
                        // For distances at least 1 we will use (x-1)^2. This function punishes higher
                        // distances very well and not so well smaller ones <=2. 
                        : checked((distance - 1).Squared());

                    // If the number couldn't be calculated (perhaps because it's too close to zero for log(x)),
                    // then we have an extremely bad figure, which can be represented by the highest badness.
                    if (double.IsNaN(localBadness))
                        return double.MaxValue;

                    // Otherwise we count in the local badness to the total one
                    totalBadness = checked(localBadness + totalBadness);
                }
                catch (OverflowException)
                {
                    // If there is any overflow, then since we're working with positive numbers it means
                    // that we have an extremely bad figure, which can be represented by the highest badness.
                    return double.MaxValue;
                }
            }

            // Return the calculated badness
            return totalBadness;
        }

        #endregion
    }
}