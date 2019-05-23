using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DrawToolsLib
{
    /// <summary>
    /// Arrow object properties
    /// </summary>
    public class PropertiesGraphicsArrow : PropertiesGraphicsBase
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

        public PropertiesGraphicsArrow()
        {

        }

        public PropertiesGraphicsArrow(GraphicsArrow arrow)
        {
            if (arrow == null )
            {
                throw new ArgumentNullException("arrow");
            }

            start = arrow.Start;
            end = arrow.End;
            lineWidth = arrow.LineWidth;
            objectColor = arrow.ObjectColor;
            actualScale = arrow.ActualScale;
            ID = arrow.Id;
            selected = arrow.IsSelected;

            this.layer = arrow.Layer;
            this.page = arrow.Page;

            this.reportContent = arrow.ReportContent;
            this.reporter = arrow.Reporter;
            this.reportDate = arrow.ReportDate;
        }

        public override GraphicsBase CreateGraphics()
        {
            GraphicsBase b = new GraphicsArrow(start, end, lineWidth, objectColor, actualScale, layer, page, reportContent, reporter, reportDate);

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

        #endregion Properties

    }
}
