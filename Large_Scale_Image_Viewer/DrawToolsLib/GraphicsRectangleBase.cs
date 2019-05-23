using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace DrawToolsLib
{
    /// <summary>
    /// Base class for rectangle-based graphics:
    /// rectangle and ellipse.
    /// </summary>
    public abstract class GraphicsRectangleBase : GraphicsBase
    {
        #region Class Members  
        protected double rectangleLeft;
        protected double rectangleTop;
        protected double rectangleRight;
        protected double rectangleBottom;

        // Position for Annotation
        protected Point rectangleStart;
        protected Point rectangleEnd;
        protected Point rectangleCenter;

        protected string rectangleReportContent;
        protected string rectangleReporter;
        protected string rectangleReportDate;
        #endregion Class Members

        #region Properties      

        public Point Start
        {
            get { return new Point(rectangleLeft, rectangleTop); }
            set { rectangleStart = value; }
        }

        public Point End
        {
            get { return new Point(rectangleRight, rectangleBottom); }
            set { rectangleEnd = value; }
        }

        public Point Center
        {
            get
            {
                rectangleCenter = new Point(rectangleLeft + (rectangleRight - rectangleLeft) / 2, rectangleTop + (rectangleBottom - rectangleTop) / 2);
                return rectangleCenter; 
            }    
        }
   
        /// <summary>
        /// Read-only property, returns Rect calculated on the fly from four points.
        /// Points can make inverted rectangle, fix this.
        /// </summary>
        public Rect Rectangle
        {   
            get
            {
                double l, t, w, h;

                if ( rectangleLeft <= rectangleRight)
                {
                    l = rectangleLeft;
                    w = rectangleRight - rectangleLeft;
                }
                else
                {
                    l = rectangleRight;
                    w = rectangleLeft - rectangleRight;
                }

                if ( rectangleTop <= rectangleBottom )
                {
                    t = rectangleTop;
                    h = rectangleBottom - rectangleTop;
                }
                else
                {
                    t = rectangleBottom;
                    h = rectangleTop - rectangleBottom;
                }

                return new Rect(l, t, w, h);
            }
        }

        public double Left
        {
            get { return rectangleLeft; }
            set { rectangleLeft = value; }
        }

        public double Top
        {
            get { return rectangleTop; }
            set { rectangleTop = value; }
        }

        public double Right
        {
            get { return rectangleRight; }
            set { rectangleRight = value; }
        }

        public double Bottom
        {
            get { return rectangleBottom; }
            set { rectangleBottom = value; }
        }

        /// <summary>
        /// Annotation Report Content
        /// </summary>
        public string ReportContent
        {
            get { return rectangleReportContent; }
            set { rectangleReportContent = value; }
        }

        /// <summary>
        /// Annotation Reporter
        /// </summary>
        public string Reporter
        {
            get { return rectangleReporter; }
            set { rectangleReporter = value; }
        }

        /// <summary>
        /// Annotation Report Date
        /// </summary>
        public string ReportDate
        {
            get { return rectangleReportDate; }
            set { rectangleReportDate = value; }
        }

        #endregion Properties

        #region Overrides
        /// <summary>
        /// Get number of handles
        /// </summary>
        public override int HandleCount
        {
            get
            {
                return 8;
            }
        }

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        public override Point GetHandle(int handleNumber)
        {
            double x, y, xCenter, yCenter;

            xCenter = (rectangleRight + rectangleLeft) / 2;
            yCenter = (rectangleBottom + rectangleTop) / 2;
            x = rectangleLeft;
            y = rectangleTop;

            switch (handleNumber)
            {
                case 1:
                    x = rectangleLeft;
                    y = rectangleTop;
                    break;
                case 2:
                    x = xCenter;
                    y = rectangleTop;
                    break;
                case 3:
                    x = rectangleRight;
                    y = rectangleTop;
                    break;
                case 4:
                    x = rectangleRight;
                    y = yCenter;
                    break;
                case 5:
                    x = rectangleRight;
                    y = rectangleBottom;
                    break;
                case 6:
                    x = xCenter;
                    y = rectangleBottom;
                    break;
                case 7:
                    x = rectangleLeft;
                    y = rectangleBottom;
                    break;
                case 8:
                    x = rectangleLeft;
                    y = yCenter;
                    break;
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Hit test.
        /// Return value: -1 - no hit
        ///                0 - hit anywhere
        ///                > 1 - handle number
        /// </summary>
        public override int MakeHitTest(Point point)
        {
            if (IsSelected)
            {
                for (int i = 1; i <= HandleCount; i++)
                {
                    if (GetHandleRectangle(i).Contains(point))
                        return i;
                }
            }

            if (Contains(point))
                return 0;

            return -1;
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    return Cursors.SizeNWSE;
                case 2:
                    return Cursors.SizeNS;
                case 3:
                    return Cursors.SizeNESW;
                case 4:
                    return Cursors.SizeWE;
                case 5:
                    return Cursors.SizeNWSE;
                case 6:
                    return Cursors.SizeNS;
                case 7:
                    return Cursors.SizeNESW;
                case 8:
                    return Cursors.SizeWE;
                default:
                    return HelperFunctions.DefaultCursor;
            }
        }

        /// <summary>
        /// Move handle to new point (resizing)
        /// </summary>
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                    rectangleLeft = point.X;
                    rectangleTop = point.Y;
                    break;
                case 2:
                    rectangleTop = point.Y;
                    break;
                case 3:
                    rectangleRight = point.X;
                    rectangleTop = point.Y;
                    break;
                case 4:
                    rectangleRight = point.X;
                    break;
                case 5:
                    rectangleRight = point.X;
                    rectangleBottom = point.Y;
                    break;
                case 6:
                    rectangleBottom = point.Y;
                    break;
                case 7:
                    rectangleLeft = point.X;
                    rectangleBottom = point.Y;
                    break;
                case 8:
                    rectangleLeft = point.X;
                    break;
            }

            RefreshDrawing(graphicsRate);
        }

        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            return Rectangle.IntersectsWith(rectangle);
        }

        /// <summary>
        /// Move object
        /// </summary>
        public override void Move(double deltaX, double deltaY)
        {
            rectangleLeft += deltaX;
            rectangleRight += deltaX;

            rectangleTop += deltaY;
            rectangleBottom += deltaY;

            RefreshDrawing(graphicsRate);
        }

        /// <summary>
        /// Map position to the virtual slide image
        /// </summary>
        public void MapToSlideImage(double xGrid, double yGrid)
        {
            rectangleLeft += xGrid;
            rectangleRight += yGrid;

            rectangleTop += xGrid;
            rectangleBottom += yGrid;

            RefreshDrawing(graphicsRate);
        }

        /// <summary>
        /// Map position to the target image
        /// </summary>
        public void MapToTargetImage(double xGrid, double yGrid)
        {
            rectangleLeft -= xGrid;
            rectangleRight -= yGrid;

            rectangleTop -= xGrid;
            rectangleBottom -= yGrid;

            RefreshDrawing(graphicsRate);
        }

        /// <summary>
        /// Normalize rectangle
        /// </summary>
        public override void Normalize()
        {
            if ( rectangleLeft > rectangleRight )
            {
                double tmp = rectangleLeft;
                rectangleLeft = rectangleRight;
                rectangleRight = tmp;
            }

            if ( rectangleTop > rectangleBottom )
            {
                double tmp = rectangleTop;
                rectangleTop = rectangleBottom;
                rectangleBottom = tmp;
            }
        }
        #endregion Overrides
    }
}
