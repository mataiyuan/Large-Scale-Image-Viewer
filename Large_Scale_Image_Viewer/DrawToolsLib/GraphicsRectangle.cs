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
    public class GraphicsRectangle : GraphicsRectangleBase
    {
        #region Constructors

        public GraphicsRectangle(double left, double top, double right, double bottom,
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

        public GraphicsRectangle()
            :
            this(0.0, 0.0, 100.0, 100.0, 1.0, Colors.Black, 1.0, 0, 0, "", "", "")
        {
        }

        #endregion Constructors

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

            Rect _rectangle = new Rect(new Point(Rectangle.TopLeft.X * rate, Rectangle.TopLeft.Y * rate),
              new Point(Rectangle.BottomRight.X * rate, Rectangle.BottomRight.Y * rate));

            drawingContext.DrawRectangle(
                null,
                new Pen(new SolidColorBrush(ObjectColor), ActualLineWidth),
                _rectangle);

            base.Draw(drawingContext, graphicsRate);
        }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            return this.Rectangle.Contains(point);
        }

        /// <summary>
        /// Serialization support
        /// </summary>
        public override PropertiesGraphicsBase CreateSerializedObject()
        {
            return new PropertiesGraphicsRectangle(this);
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
