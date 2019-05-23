using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;



namespace DrawToolsLib
{
    /// <summary>
    /// Line object properties
    /// </summary>
    public class PropertiesGraphicsLine : PropertiesGraphicsBase
    {
        private int layer;
        private int page;

        private Point start;
        private Point end;
        private double lineWidth;
        private Color objectColor;

        private string reportContent;
        private string reporter;
        private string reportDate;

        public PropertiesGraphicsLine()
        {

        }

        public PropertiesGraphicsLine(GraphicsLine line)
        {
            if ( line == null )
            {
                throw new ArgumentNullException("line");
            }

            this.start = line.Start;
            this.end = line.End;
            this.lineWidth = line.LineWidth;
            this.objectColor = line.ObjectColor;
            this.actualScale = line.ActualScale;
            this.ID = line.Id;
            this.selected = line.IsSelected;

            this.layer = line.Layer;
            this.page = line.Page;

            this.reportContent = line.ReportContent;
            this.reporter = line.Reporter;
            this.reportDate = line.ReportDate;
        }

        public override GraphicsBase CreateGraphics()
        {
            GraphicsBase b = new GraphicsLine(start, end, lineWidth, objectColor, actualScale,layer, page, reportContent, reporter, reportDate);

            if (this.ID != 0)
            {
                b.Id = this.ID;
                b.IsSelected = this.selected;
            }

            return b;
        }

        #region Properties

        /// <summary>
        /// Start Point
        /// </summary>
        public Point Start
        {
            get { return start; }
            set { start = value; }
        }

        /// <summary>
        /// End Point
        /// </summary>
        public Point End
        {
            get { return end; }
            set { end = value; }
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
