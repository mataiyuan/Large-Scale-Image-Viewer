using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawToolsLib
{
    /// <summary>
    ///  Ruler graphics object.
    /// </summary>
    public class GraphicsRuler : GraphicsBase
    {
        #region Class Members
        protected Point rulerStart;
        protected Point rulerEnd;
        protected Point rulerCenter;
        
        protected Point rulerStart1;
        protected Point rulerStart2;
        protected Point rulerEnd1;
        protected Point rulerEnd2;
        
        protected string rulerReportContent;
        protected string rulerReporter;
        protected string rulerReportDate;
        #endregion Class Members

        #region Constructors

        public GraphicsRuler(Point start, Point end, double lineWidth, Color objectColor, double actualScale, int layer, int page,
              string reportContent, string reporter, string reportDate)
        {
            this.rulerStart = start;
            this.rulerEnd = end;
            this.graphicsLineWidth = lineWidth;
            this.graphicsObjectColor = objectColor;
            this.graphicsActualScale = actualScale;

            //-----------------------//
            this.Layer = layer;
            this.Page = page;

            this.rulerReportContent = reportContent;
            this.rulerReporter = reporter;
            this.rulerReportDate = reportDate;

            //RefreshDrawng();
        }

        public GraphicsRuler()
            :
            this(new Point(0.0, 0.0), new Point(100.0, 100.0), 1.0, Colors.Black, 1.0, 0, 0, "", "", "")
        {
        }

        #endregion Constructors

        #region Properties

        public Point Start
        {
            get { return rulerStart; }
            set { rulerStart = value; }
        }

        public Point End
        {
            get { return rulerEnd; }
            set { rulerEnd = value; }
        }

        public Point Center
        {
            get
            {
                rulerCenter = new Point(rulerStart.X + (rulerEnd.X - rulerStart.X) / 2, rulerStart.Y + (rulerEnd.Y - rulerStart.Y) / 2);
                return rulerCenter;
            }
        }

        /// <summary>
        /// Annotation Report Content
        /// </summary>
        public string ReportContent
        {
            get { return rulerReportContent; }
            set { rulerReportContent = value; }
        }

        /// <summary>
        /// Annotation Reporter
        /// </summary>
        public string Reporter
        {
            get { return rulerReporter; }
            set { rulerReporter = value; }
        }

        /// <summary>
        /// Annotation Report Date
        /// </summary>
        public string ReportDate
        {
            get { return rulerReportDate; }
            set { rulerReportDate = value; }
        }

        #endregion Properties

        #region Overrides
        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext drawingContext, double rate)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            double theta = Math.Atan((rulerStart.Y - rulerEnd.Y) / (rulerEnd.X - rulerStart.X));
            this.rulerStart1 = new Point(rulerStart.X - 8 * Math.Sin(theta), rulerStart.Y - 8 * Math.Cos(theta));
            this.rulerStart2 = new Point(rulerStart.X + 8 * Math.Sin(theta), rulerStart.Y + 8 * Math.Cos(theta));
            this.rulerEnd1 = new Point(rulerEnd.X - 8 * Math.Sin(theta), rulerEnd.Y - 8 * Math.Cos(theta));
            this.rulerEnd2 = new Point(rulerEnd.X + 8 * Math.Sin(theta), rulerEnd.Y + 8 * Math.Cos(theta));

            Point _rulerStart = new Point(rulerStart.X * rate, rulerStart.Y * rate);
            Point _rulerEnd = new Point(rulerEnd.X * rate, rulerEnd.Y * rate);

            Point _rulerStart1 = new Point(rulerStart1.X * rate, rulerStart1.Y * rate);
            Point _rulerEnd1 = new Point(rulerEnd1.X * rate, rulerEnd1.Y * rate);
            Point _rulerStart2 = new Point(rulerStart2.X * rate, rulerStart2.Y * rate);
            Point _rulerEnd2 = new Point(rulerEnd2.X * rate, rulerEnd2.Y * rate);
            
            double distance = Math.Round(Math.Sqrt(Math.Pow(Math.Abs(rulerEnd.X - rulerStart.X), 2) + Math.Pow(Math.Abs(rulerEnd.Y - rulerStart.Y), 2)),2);
            string dis = distance.ToString();
            string corstart = "(" + Math.Round(rulerStart.X,2).ToString() + "," + Math.Round(rulerStart.Y,2).ToString() + ")";
            string corend = "(" + Math.Round(rulerEnd.X,2).ToString() + "," + Math.Round(rulerEnd.Y,2).ToString() + ")";

            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                _rulerStart,
                _rulerEnd);
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                _rulerStart1,
                _rulerStart2);
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                _rulerEnd1,
                _rulerEnd2);

            drawingContext.DrawText(new FormattedText(corstart, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface("Verdana"),8, Brushes.Black),_rulerStart);
            drawingContext.DrawText(new FormattedText(corend, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface("Verdana"), 8, Brushes.Black),_rulerEnd);
            drawingContext.DrawText(new FormattedText(dis, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface("Verdana"), 8, Brushes.Black), new Point((_rulerEnd.X + _rulerStart.X) / 2, 
                (_rulerEnd.Y + _rulerStart.Y) / 2));

            base.Draw(drawingContext, graphicsRate);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            LineGeometry g = new LineGeometry(rulerStart, rulerEnd);

            return g.StrokeContains(new Pen(Brushes.Black, LineHitTestWidth), point);
        }

        /// <summary>
        /// XML serialization support
        /// </summary>
        /// <returns></returns>
        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsRuler(this);
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
                return rulerStart;
            else
                return rulerEnd;
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

            LineGeometry lg = new LineGeometry(rulerStart, rulerEnd);
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
                rulerStart = point;
            else
                rulerEnd = point;

            RefreshDrawing(graphicsRate);
        }

        /// <summary>
        /// Move object
        /// </summary>
        public override void Move(double deltaX, double deltaY)
        {
            rulerStart.X += deltaX;
            rulerStart.Y += deltaY;

            rulerEnd.X += deltaX;
            rulerEnd.Y += deltaY;

            rulerStart1.X += deltaX;
            rulerStart1.Y += deltaY;

            rulerStart2.X += deltaX;
            rulerStart2.Y += deltaY;

            rulerEnd1.X += deltaX;
            rulerEnd1.Y += deltaY;

            rulerEnd2.X += deltaX;
            rulerEnd2.Y += deltaY;

            RefreshDrawing(graphicsRate);
        }


        /// <summary>
        /// Covert to string
        /// </summary>
        public override string ToString()
        {
            string graphicInfo = "Annotation Type: Ruler" + "\n" +
           "Position: " + "[ X " + Start.X.ToString("f2") + ", Y " + Start.Y.ToString("f2") + "] Size: [ Width " + Math.Abs(End.X - Start.X).ToString("f2") + ", Height " + Math.Abs(End.Y - Start.Y).ToString("f2") + "]" + "\n" +
           "Color: " + ObjectColor.ToString() + ", LineWidth: " + LineWidth.ToString() + "\n";

            return graphicInfo;
        }

        #endregion Overrides
    }
}
