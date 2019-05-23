using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;



namespace DrawToolsLib
{
    /// <summary>
    /// Text object properties
    /// </summary>
    public class PropertiesGraphicsText : PropertiesGraphicsBase
    {
        private int layer;
        private int page;

        private string text;
        private double left;
        private double top;
        private double right;
        private double bottom;
        private Color objectColor;
        private double textFontSize;
        private string textFontFamilyName;
        private string textFontStyle;
        private string textFontWeight;
        private string textFontStretch;
        private string reportContent;
        private string reporter;
        private string reportDate;

        public PropertiesGraphicsText()
        {

        }

        public PropertiesGraphicsText(GraphicsText graphicsText)
        {
            if ( graphicsText == null )
            {
                throw new ArgumentNullException("graphicsText");
            }

            this.text = graphicsText.Text;
            this.left = graphicsText.Left;
            this.top = graphicsText.Top;
            this.right = graphicsText.Right;
            this.bottom = graphicsText.Bottom;
            this.objectColor = graphicsText.ObjectColor;
            this.textFontSize = graphicsText.TextFontSize;
            this.textFontFamilyName = graphicsText.TextFontFamilyName;
            this.textFontStyle = FontConversions.FontStyleToString(graphicsText.TextFontStyle);
            this.textFontWeight = FontConversions.FontWeightToString(graphicsText.TextFontWeight);
            this.textFontStretch = FontConversions.FontStretchToString(graphicsText.TextFontStretch);
            this.actualScale = graphicsText.ActualScale;
            this.ID = graphicsText.Id;
            this.selected = graphicsText.IsSelected;

            this.layer = graphicsText.Layer;
            this.page = graphicsText.Page;

            this.reportContent = graphicsText.ReportContent;
            this.reporter = graphicsText.Reporter;
            this.reportDate = graphicsText.ReportDate;
        }

        public override GraphicsBase CreateGraphics()
        {
            GraphicsBase b = new GraphicsText(
                text,
                left,
                top,
                right,
                bottom,
                objectColor,
                textFontSize,
                textFontFamilyName,
                FontConversions.FontStyleFromString(textFontStyle),
                FontConversions.FontWeightFromString(textFontWeight),
                FontConversions.FontStretchFromString(textFontStretch),
                actualScale, reportContent, reporter, reportDate);

            if (this.ID != 0)
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
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// Top bounding rectangle side, Y
        /// </summary>
        public double Left
        {
            get { return left; }
            set { left = value; }
        }

        /// <summary>
        /// Right bounding rectangle side, X
        /// </summary>
        public double Top
        {
            get { return top; }
            set { top = value; }
        }

        /// <summary>
        /// Bottom bounding rectangle side, Y
        /// </summary>
        public double Right
        {
            get { return right; }
            set { right = value; }
        }

        public double Bottom
        {
            get { return bottom; }
            set { bottom = value; }
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
        /// Font Size
        /// </summary>
        public double TextFontSize
        {
            get { return textFontSize; }
            set { textFontSize = value; }
        }

        /// <summary>
        /// Font Family Name
        /// </summary>
        public string TextFontFamilyName
        {
            get { return textFontFamilyName; }
            set { textFontFamilyName = value; }
        }

        /// <summary>
        /// Font Style
        /// </summary>
        public string TextFontStyle
        {
            get { return textFontStyle; }
            set { textFontStyle = value; }
        }

        /// <summary>
        /// Font Weight
        /// </summary>
        public string TextFontWeight
        {
            get { return textFontWeight; }
            set { textFontWeight = value; }
        }

        /// <summary>
        /// Font Stretch
        /// </summary>
        public string TextFontStretch
        {
            get { return textFontStretch; }
            set { textFontStretch = value; }
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
