using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawToolsLib
{
    /// <summary>
    ///  Line graphics object.
    /// </summary>
    public class GraphicsLine : GraphicsBase
    {
        #region Class Members

        protected Point lineStart;
        protected Point lineEnd;
        protected Point lineCenter;

        protected string lineReportContent;
        protected string lineReporter;
        protected string lineReportDate;
        #endregion Class Members

        #region Constructors

        public GraphicsLine(Point start, Point end, double lineWidth, Color objectColor, double actualScale, int layer, int page,
            string reportContent, string reporter, string reportDate)
        {
            this.lineStart = start;
            this.lineEnd = end;
            this.graphicsLineWidth = lineWidth;
            this.graphicsObjectColor = objectColor;
            this.graphicsActualScale = actualScale;

            //-----------------------//
            this.Layer = layer;
            this.Page = page;

            this.lineReportContent = reportContent;
            this.lineReporter = reporter;
            this.lineReportDate = reportDate;

        }

        public GraphicsLine()
            :
            this(new Point(0.0, 0.0), new Point(100.0, 100.0), 1.0,  Colors.Black, 1.0, 0, 0,"","","")
        {
        }

        #endregion Constructors

        #region Properties

        public Point Start
        {
            get { return lineStart; }
            set { lineStart = value; }
        }

        public Point End
        {
            get { return lineEnd; }
            set { lineEnd = value; }
        }

        public Point Center
        {
            get 
            {
                lineCenter = new Point(lineStart.X + (lineEnd.X - lineStart.X) / 2, lineStart.Y + (lineEnd.Y - lineStart.Y) / 2);
                return lineCenter;
            }       
        }

        /// <summary>
        /// Annotation Report Content
        /// </summary>
        public string ReportContent
        {
            get { return lineReportContent; }
            set { lineReportContent = value; }
        }

        /// <summary>
        /// Annotation Reporter
        /// </summary>
        public string Reporter
        {
            get { return lineReporter; }
            set { lineReporter = value; }
        }

        /// <summary>
        /// Annotation Report Date
        /// </summary>
        public string ReportDate
        {
            get { return lineReportDate; }
            set { lineReportDate = value; }
        }
        #endregion Properties

        #region Overrides
        
        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext drawingContext, double rate)
        {
            
            if ( drawingContext == null )
            {
                throw new ArgumentNullException("drawingContext");
            }

            Point _lineStart = new Point(lineStart.X * rate, lineStart.Y * rate);
            Point _lineEnd = new Point(lineEnd.X * rate, lineEnd.Y * rate);

            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                _lineStart,
                _lineEnd);

            base.Draw(drawingContext, graphicsRate);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            LineGeometry g = new LineGeometry(lineStart, lineEnd);

       //      return g.FillContains(pointm);
           return g.StrokeContains(new Pen(Brushes.Black, LineHitTestWidth), point, 12, ToleranceType.Absolute);
            
        }

        /// <summary>
        /// XML serialization support
        /// </summary>
        /// <returns></returns>
        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsLine(this);
        }

        /// <summary>
        /// Get number of handles
        /// </summary>
        public override int HandleCount
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        public override Point GetHandle(int handleNumber)
        {
            if (handleNumber == 1)
                return lineStart;
            else
                return lineEnd;
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
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            RectangleGeometry rg = new RectangleGeometry(rectangle);

            LineGeometry lg = new LineGeometry(lineStart, lineEnd);
            PathGeometry widen = lg.GetWidenedPathGeometry(new Pen(Brushes.Black, LineHitTestWidth));

            PathGeometry p = Geometry.Combine(rg, widen, GeometryCombineMode.Intersect, null);

            return (!p.IsEmpty());
        }

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        public override Cursor GetHandleCursor(int handleNumber)
        {
            switch (handleNumber)
            {
                case 1:
                case 2:
                    return Cursors.SizeAll;
                default:
                    return HelperFunctions.DefaultCursor;
            }
        }

        /// <summary>
        /// Move handle to new point (resizing)
        /// </summary>
        public override void MoveHandleTo(Point point, int handleNumber)
        {
            if (handleNumber == 1)
                lineStart = point;
            else
                lineEnd = point;

            RefreshDrawing(graphicsRate);
        }

        /// <summary>
        /// Move object
        /// </summary>
        public override void Move(double deltaX, double deltaY)
        {
            lineStart.X += deltaX;
            lineStart.Y += deltaY;

            lineEnd.X += deltaX;
            lineEnd.Y += deltaY;

            RefreshDrawing(graphicsRate);
        }

        /// <summary>
        /// Covert to string
        /// </summary>
        public override string ToString()
        {
            string graphicInfo = "Annotation Type: Line" + "\n" +
           "Position: " + "[ X " + Start.X.ToString("f2") + ", Y " + Start.Y.ToString("f2") + "] Size: [ Width " + Math.Abs(End.X - Start.X).ToString("f2") + ", Height " + Math.Abs(End.Y - Start.Y).ToString("f2") + "]" + "\n" +
           "Color: " + ObjectColor.ToString() + ", LineWidth: " + LineWidth.ToString() + "\n";

            return graphicInfo;
        }
        #endregion Overrides
    }
}
