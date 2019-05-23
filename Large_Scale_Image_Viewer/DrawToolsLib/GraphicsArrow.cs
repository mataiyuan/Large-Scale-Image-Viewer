using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace DrawToolsLib
{
    /// <summary>
    ///  Arrow graphics object.
    /// </summary>
    public class GraphicsArrow : GraphicsBase
    {
        #region Class Members

        protected Point arrowStart;
        protected Point arrowEnd;
        protected Point arrowCenter;

        protected Point arrowLeft;
        protected Point arrowRight;

        protected string arrowReportContent;
        protected string arrowReporter;
        protected string arrowReportDate;
        #endregion Class Members

        #region Constructors

        public GraphicsArrow(Point start, Point end, double lineWidth, Color objectColor, double actualScale, int layer, int page,
            string reportContent, string reporter, string reportDate)
        {
            this.arrowStart = start;
            this.arrowEnd = end; 
            this.graphicsLineWidth = lineWidth;
            this.graphicsObjectColor = objectColor;
            this.graphicsActualScale = actualScale;

            //-----------------------//
            this.Layer = layer;
            this.Page = page;

            this.arrowReportContent = reportContent;
            this.arrowReporter = reporter;
            this.arrowReportDate = reportDate;

            //RefreshDrawng();
        }

        public GraphicsArrow()
            :
            this(new Point(0.0, 0.0), new Point(100.0, 100.0), 1.0, Colors.Black, 1.0, 0, 0, "", "", "")
        {
        }

        #endregion Constructors

        #region Properties

        public Point Start
        {
            get { return arrowStart; }
            set { arrowStart = value; }
        }

        public Point End
        {
            get { return arrowEnd; }
            set { arrowEnd = value; }
        }

        public Point Center
        {
            get
            {
                arrowCenter = new Point(arrowStart.X + (arrowEnd.X - arrowStart.X) / 2, arrowStart.Y + (arrowEnd.Y - arrowStart.Y) / 2);
                return arrowCenter;
            }
        }

        /// <summary>
        /// Annotation Report Content
        /// </summary>
        public string ReportContent
        {
            get { return arrowReportContent; }
            set { arrowReportContent = value; }
        }

        /// <summary>
        /// Annotation Reporter
        /// </summary>
        public string Reporter
        {
            get { return arrowReporter; }
            set { arrowReporter = value; }
        }

        /// <summary>
        /// Annotation Report Date
        /// </summary>
        public string ReportDate
        {
            get { return arrowReportDate; }
            set { arrowReportDate = value; }
        }

        #endregion Properties

        #region Overrides
        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext drawingContext, double rate)
        {
            // ¡¾Ôö¼Órate¡¿

            if ( drawingContext == null )
            {
                throw new ArgumentNullException("drawingContext");
            }

            if(arrowEnd.X>arrowStart.X)
            {
                double theta = Math.Atan((arrowStart.Y - arrowEnd.Y) / (arrowEnd.X - arrowStart.X));
                this.arrowLeft = new Point(arrowEnd.X - 20 * Math.Sin(Math.PI / 3 - theta), arrowEnd.Y + 20 * Math.Cos(Math.PI / 3 - theta));
                this.arrowRight = new Point(arrowEnd.X - 20 * Math.Cos(theta - Math.PI / 6), arrowEnd.Y + 20 * Math.Sin(theta - Math.PI / 6));
            }
            else
            {
                double theta = Math.Atan((arrowStart.Y - arrowEnd.Y) / (arrowStart.X - arrowEnd.X));
                this.arrowLeft = new Point(arrowEnd.X + 20 * Math.Sin(Math.PI / 3 - theta), arrowEnd.Y + 20 * Math.Cos(Math.PI / 3 - theta));
                this.arrowRight = new Point(arrowEnd.X + 20 * Math.Cos(theta - Math.PI / 6), arrowEnd.Y + 20 * Math.Sin(theta - Math.PI / 6));
            }

         /*
            drawingContext.DrawLine(
             new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
             arrowStart,
             arrowEnd);
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                arrowLeft,
                arrowEnd);
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                arrowRight,
                arrowEnd);

         */
            Point _arrowStart = new Point(arrowStart.X * rate, arrowStart.Y * rate);
            Point _arrowEnd = new Point(arrowEnd.X * rate, arrowEnd.Y * rate);

            Point _arrowLeft = new Point(arrowLeft.X * rate, arrowLeft.Y * rate);
            Point _arrowRight = new Point(arrowRight.X * rate, arrowRight.Y * rate);

            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                _arrowStart,
                _arrowEnd);
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                _arrowLeft,
                _arrowEnd);
            drawingContext.DrawLine(
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                _arrowRight,
                _arrowEnd);
          

            base.Draw(drawingContext, graphicsRate);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            LineGeometry g = new LineGeometry(arrowStart, arrowEnd);

            return g.StrokeContains(new Pen(Brushes.Black, LineHitTestWidth), point);
        }

        /// <summary>
        /// XML serialization support
        /// </summary>
        /// <returns></returns>
        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsArrow(this);
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
                return arrowStart;
            else
                return arrowEnd;
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

            LineGeometry lg = new LineGeometry(arrowStart, arrowEnd);
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
                arrowStart = point;
            else
                arrowEnd = point;

            RefreshDrawing(graphicsRate);
        }

        /// <summary>
        /// Move object
        /// </summary>
        public override void Move(double deltaX, double deltaY)
        {
            arrowStart.X += deltaX;
            arrowStart.Y += deltaY;

            arrowEnd.X += deltaX;
            arrowEnd.Y += deltaY;

            arrowLeft.X += deltaX;
            arrowLeft.Y += deltaY;

            arrowRight.X += deltaX;
            arrowRight.Y += deltaY;

            RefreshDrawing(graphicsRate);
        }


        /// <summary>
        /// Covert to string
        /// </summary>
        public override string ToString()
        {
            string graphicInfo = "Annotation Type: ArrowLine" + "\n" +
           "Position: " + "[ X " + Start.X.ToString("f2") + ", Y " + Start.Y.ToString("f2") + "] Size: [ Width " + Math.Abs(End.X - Start.X).ToString("f2") + ", Height " + Math.Abs(End.Y - Start.Y).ToString("f2") + "]" + "\n" +
           "Color: " + ObjectColor.ToString() + ", LineWidth: " + LineWidth.ToString() + "\n";

            return graphicInfo;
        }
        #endregion Overrides
    }
}
