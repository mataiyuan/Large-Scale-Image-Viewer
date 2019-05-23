using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawToolsLib
{
    /// <summary>
    ///  Rectangle graphics object.
    /// </summary>
    public class GraphicsEllipse : GraphicsRectangleBase
    {
        #region Constructors

        public GraphicsEllipse(double left, double top, double right, double bottom,
            double lineWidth, Color objectColor, double actualScale, int layer, int page,
            string reportContent, string reporter, string reportDate)
        {
    
            this.rectangleLeft = left;
            this.rectangleTop = top;
            this.rectangleRight = right;
            this.rectangleBottom = bottom;
            this.graphicsLineWidth = lineWidth;
            this.graphicsObjectColor = objectColor;
            this.graphicsActualScale = actualScale;

            //-----------------------//
            this.Layer = layer;
            this.Page = page;

            this.rectangleReportContent = reportContent;
            this.rectangleReporter = reporter;
            this.rectangleReportDate = reportDate;
  
            //RefreshDrawng();
        }

        public GraphicsEllipse()
            :
            this(0.0, 0.0, 100.0, 100.0, 1.0, Colors.Black, 1.0, 0, 0, "","","")
        {
        }

        #endregion Constructors

        #region Overrides
        
        /// <summary>
        /// Draw object with rate
        /// </summary>
        public override void Draw(DrawingContext drawingContext, double rate)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            Rect r = Rectangle;

            Point center = new Point(
                (r.Left + r.Right) * rate / 2.0,
                (r.Top + r.Bottom) * rate / 2.0);

            double radiusX = (r.Right - r.Left) * rate / 2.0;
            double radiusY = (r.Bottom - r.Top) * rate / 2.0;

            drawingContext.DrawEllipse(
                null,
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                center,
                radiusX,
                radiusY);

            base.Draw(drawingContext, graphicsRate);
        }

   
        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            if ( IsSelected )
            {
                return this.Rectangle.Contains(point);
            }
            else
            {
                EllipseGeometry g = new EllipseGeometry(Rectangle);

                return g.FillContains(point) || g.StrokeContains(new Pen(Brushes.Black, ActualLineWidth), point);
            }
        }

        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public override bool IntersectsWith(Rect rectangle)
        {
            RectangleGeometry rg = new RectangleGeometry(rectangle);    // parameter
            EllipseGeometry eg = new EllipseGeometry(Rectangle);        // this object rectangle

            PathGeometry p = Geometry.Combine(rg, eg, GeometryCombineMode.Intersect, null);

            return (!p.IsEmpty());
        }

        /// <summary>
        /// Serialization support
        /// </summary>
        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsEllipse(this);
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
