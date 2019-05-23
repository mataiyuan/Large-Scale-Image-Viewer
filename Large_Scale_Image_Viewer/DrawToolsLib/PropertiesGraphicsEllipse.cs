using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;


namespace DrawToolsLib
{
    /// <summary>
    /// Ellipse object properties.
    /// </summary>
    public class PropertiesGraphicsEllipse : PropertiesGraphicsBase
    {
        private int layer;
        private int page;

        private double left;
        private double top;
        private double right;
        private double bottom;
        private double lineWidth;
        private Color objectColor;
        private string reportContent;
        private string reporter;
        private string reportDate;
        public PropertiesGraphicsEllipse()
        {

        }

        public PropertiesGraphicsEllipse(GraphicsEllipse ellipse)
        {
            if (ellipse == null)
            {
                throw new ArgumentNullException("ellipse");
            }

            this.left = ellipse.Left;
            this.top = ellipse.Top;
            this.right = ellipse.Right;
            this.bottom = ellipse.Bottom;

            this.lineWidth = ellipse.LineWidth;
            this.objectColor = ellipse.ObjectColor;
            this.actualScale = ellipse.ActualScale;
            this.ID = ellipse.Id;
            this.selected = ellipse.IsSelected;

            this.layer = ellipse.Layer;
            this.page = ellipse.Page;

            this.reportContent = ellipse.ReportContent;
            this.reporter = ellipse.Reporter;
            this.reportDate = ellipse.ReportDate;
        }

        public override GraphicsBase CreateGraphics()
        {
            GraphicsBase b = new GraphicsEllipse(left, top, right, bottom, lineWidth, 
                objectColor, actualScale, layer, page, reportContent, reporter, reportDate);

            if ( this.ID != 0 )
            {
                b.Id = this.ID;
                b.IsSelected = this.selected;
            }

            return b;
        }

        #region Properties
        /// <summary>
        /// Left bounding rectangle side, X
        /// </summary>
        public double Left
        {
            get { return left; }
            set { left = value; }
        }

        /// <summary>
        /// Top bounding rectangle side, Y
        /// </summary>
        public double Top
        {
            get { return top; }
            set { top = value; }
        }

        /// <summary>
        /// Right bounding rectangle side, X
        /// </summary>
        public double Right
        {
            get { return right; }
            set { right = value; }
        }

        /// <summary>
        /// Bottom bounding rectangle side, Y
        /// </summary>
        public double Bottom
        {
            get { return bottom; }
            set { bottom = value; }
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
        /// Color
        /// </summary>
        public Color ObjectColor
        {
            get { return objectColor; }
            set { objectColor = value; }
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
