using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;



namespace DrawToolsLib
{
    /// <summary>
    /// Ruler object properties
    /// </summary>
    public class PropertiesGraphicsRuler : PropertiesGraphicsBase
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
        public PropertiesGraphicsRuler()
        {

        }

        public PropertiesGraphicsRuler(GraphicsRuler ruler)
        {
            if (ruler == null )
            {
                throw new ArgumentNullException("ruler");
            }

            start = ruler.Start;
            end = ruler.End;
            lineWidth = ruler.LineWidth;
            objectColor = ruler.ObjectColor;
            actualScale = ruler.ActualScale;
            ID = ruler.Id;
            selected = ruler.IsSelected;

            this.layer = ruler.Layer;
            this.page = ruler.Page;

            this.reportContent = ruler.ReportContent;
            this.reporter = ruler.Reporter;
            this.reportDate = ruler.ReportDate;
        }
    

        public override GraphicsBase CreateGraphics()
        {
            GraphicsBase b = new GraphicsRuler(start, end, lineWidth, objectColor, actualScale, layer, page, reportContent, reporter, reportDate);

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
