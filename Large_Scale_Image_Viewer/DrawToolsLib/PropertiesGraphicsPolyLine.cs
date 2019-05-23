using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace DrawToolsLib
{
    /// <summary>
    /// Polyline object properties
    /// </summary>
    public class PropertiesGraphicsPolyLine : PropertiesGraphicsBase
    {
        private int layer;
        private int page;

        private Point[] points;
        private double lineWidth;
        private Color objectColor;

        private string reportContent;
        private string reporter;
        private string reportDate;

        public PropertiesGraphicsPolyLine()
        {

        }

        public PropertiesGraphicsPolyLine(GraphicsPolyLine polyLine)
        {
            if (polyLine == null)
            {
                throw new ArgumentNullException("polyLine");
            }

            this.points = polyLine.GetPoints();
            this.lineWidth = polyLine.LineWidth;
            this.objectColor = polyLine.ObjectColor;
            this.actualScale = polyLine.ActualScale;
            this.ID = polyLine.Id;
            this.selected = polyLine.IsSelected;

            this.layer = polyLine.Layer;
            this.page = polyLine.Page;

            this.reportContent = polyLine.ReportContent;
            this.reporter = polyLine.Reporter;
            this.reportDate = polyLine.ReportDate;
        }

        public override GraphicsBase CreateGraphics()
        {
            GraphicsBase b = new GraphicsPolyLine(points, lineWidth, objectColor, actualScale, layer, page, reportContent, reporter, reportDate);

            if (this.ID != 0)
            {
                b.Id = this.ID;
                b.IsSelected = this.selected;
            }

            return b;

        }

        #region Properties
        /// <summary>
        /// Points
        /// </summary>
        public Point[] Points
        {
            get { return points; }
            set { points = value; }
        }


        /// <summary>
        /// Line Width
        /// </summary>
        public double LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        /// <summary>
        /// Layer
        /// </summary>
        public int Layer
        {
            get { return layer; }
            set { layer = value; }
        }

        /// <summary>
        /// Page
        /// </summary>
        public int Page
        {
            get { return page; }
            set { page = value; }
        }

        /// <summary>
        /// Color
        /// </summary>
        public Color ObjectColor
        {
            get { return objectColor; }
            set { objectColor = value; }
        }

        /// <summary>
        /// Annotation Report Content
        /// </summary>
        public string ReportContent
        {
            get { return reportContent; }
            set { reportContent = value; }
        }

        /// <summary>
        /// Annotation Reporter
        /// </summary>
        public string Reporter
        {
            get { return reporter; }
            set { reporter = value; }
        }

        /// <summary>
        /// Annotation Report Date
        /// </summary>
        public string ReportDate
        {
            get { return reportDate; }
            set { reportDate = value; }
        }
        #endregion Properties
    }
}
