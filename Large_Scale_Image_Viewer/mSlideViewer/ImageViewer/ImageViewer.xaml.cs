using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.ComponentModel;

using DrawToolsLib;
using Utilities;

namespace mSlideViewer
{
    #region Public Enum 
    /// <summary>
    /// Describes the zoom action occuring
    /// </summary>
    [Flags]
    public enum ImageBoxZoomActions
    {
        /// <summary>
        /// No action.
        /// </summary>
        None = 0,

        /// <summary>
        /// The control is increasing the zoom.
        /// </summary>
        ZoomIn = 1,

        /// <summary>
        /// The control is decreasing the zoom.
        /// </summary>
        ZoomOut = 2,

        /// <summary>
        /// The control zoom was reset.
        /// </summary>
        ActualSize = 4
    }
    
    #endregion

    public partial class ImageViewer : UserControl
    {
        #region Instance Fields
        private const double MaxZoom = 1.0;
        private const double MinZoom = 0.01;
        Point _currentPositionOnTarget;
        private double _fszoom = 1.0;
        private bool _isSlideImageLoaded = false;
        private int _currentPage = 0;
        bool _dragging;

        #endregion
        public ImageViewer()
        {
            InitializeComponent();
            _fszoom = 1;
            _currentPositionOnTarget = new Point();

            imageCanvas.ScrollOwner = ScrollImageViewer;
        
            TransformGroup g = new TransformGroup();
            ScaleTransform _scale = imageCanvas._scale;

            TranslateTransform _translate = imageCanvas._translate;
            annotationCanvas.SetTransform(g, _translate, _scale);

            // 2017-08-21
            imageCanvas.Translate.Changed += new EventHandler(OnTranslateChanged);
            imageCanvas.Scale.Changed += new EventHandler(OnZoomScaleChanged);
       //     imageCanvas.VisualsChanged += new EventHandler<VirtualCanvasLib.VisualChangeEventArgs>(OnVisualsChanged);

            zoomSlider.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(zoomSlider_MouseLeftButtonDown), true);
            // zoomSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Slider_MouseLeftButtonUp), true);


        }

        /*
        Point CenterOfTargetView;

        void OnVisualsChanged(object sender, EventArgs e)
        {

            CenterOfTargetView.X = imageCanvas.HorizontalOffset + ScrollImageViewer.ViewportWidth / 2;
            CenterOfTargetView.Y = imageCanvas.VerticalOffset + ScrollImageViewer.ViewportHeight / 2;
        }
        */


        void OnTranslateChanged(object sender, EventArgs e)
        {


        }

        double gridContainer_xoffset;
        double gridContainer_yoffset;
        void OnZoomScaleChanged(object sender, EventArgs e)
        {
            //--------------------------------------2017-08-21 未完善----------------------------------------
            // PageLevel-2 与 PageLevel-3之间切换过程中会出现错误           

            if (imageCanvas.ExtentWidth < ScrollImageViewer.ViewportWidth)
            {
                gridContainer.Width = imageCanvas.ExtentWidth;
                imageCanvas.ModifyHorizontalOffset(0);         

                // 2017-08-24 wkj
                gridContainer_xoffset = (ScrollImageViewer.ViewportWidth - gridContainer.Width) / 2;
                gridContainer_yoffset = (ScrollImageViewer.ViewportHeight - gridContainer.Height) / 2;      
            }
            else
                gridContainer.Width = ScrollImageViewer.ViewportWidth;

            if (imageCanvas.ExtentHeight < ScrollImageViewer.ViewportHeight)
            {
                gridContainer.Height = imageCanvas.ExtentHeight;
                imageCanvas.ModifyVerticalOffset(0);
            }
            else
                gridContainer.Height = ScrollImageViewer.ViewportHeight;

        }

        /// <summary>
        ///   Gets or sets the zoom.
        /// </summary>
        /// <value>The zoom.</value>
        [DefaultValue(1.0)]
        [Category("Appearance")]
        public virtual double FullScaleZoom
        {
            get { return _fszoom; }
            set
            {
                if (value > 1)
                    _fszoom = 1;
                else if (value < 0.00001)
                    _fszoom = 0.00001;
                else
                    _fszoom = value;
            }
        }

        /// <summary>
        ///   Gets the Current Position On Target (Loaded Image).
        /// </summary>
        /// <returns></returns>
        public Point CurrentPositionOnTarget
        {
            get
            {
                return _currentPositionOnTarget;
            }
        }

        /// <summary>
        ///   Judge the loaded state of slide image.
        /// </summary>
        /// <returns></returns>
        public bool IsSlideImageLoaded
        {
            get
            {
                return _isSlideImageLoaded;
            }
            set
            {
                _isSlideImageLoaded = value;
            }
        }

        /// <summary>
        ///   Get or set the current page of slide image in tiff file.
        /// </summary>
        /// <returns></returns>
        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                // 仅允许 Page = 0,1,2,3
                if (value < 0)
                    _currentPage = 0;
                else if (value > 3)
                    _currentPage = 3;
                else
                    _currentPage = value;
            }
        }

        /// <summary>
        ///   Get or set the state when mouse moving.
        /// </summary>
        /// <returns></returns>
        public bool Dragging
        {
            get
            {
                return _dragging;
            }
            set
            {
                _dragging = value;
            }
        }

        /// <summary>
        /// Updates the current zoom.
        /// </summary>
        /// <param name="value">The new zoom value.</param>
        /// <param name="actions">The zoom actions that caused the value to be updated.</param>        
        private void SetZoom(double value, ImageBoxZoomActions actions)
        {
            double previousZoom;

            previousZoom = this.FullScaleZoom;

            if (value < MinZoom)
            {
                value = MinZoom;
            }
            else if (value > MaxZoom)
            {
                value = MaxZoom;
            }

            if (_fszoom != value)
            {
                _fszoom = value;
            }
        }

        #region Override Methods    
        private bool IsMouseMoveOnAnnotation(Point p)
        {
            // DrawingCanves 鼠标处理
            // Tool = Pointer, 如果已有标注被选择，返回
            if (annotationCanvas.Count > 0 && annotationCanvas.Tool == ToolType.Pointer)
            {
                foreach (GraphicsBase g in annotationCanvas.GraphicsList)
                {
                    if (g.Contains(p))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
                return false;
        }

        private int MouseMoveOnAnnotation(Point p)
        {
            // DrawingCanves 鼠标处理
            // Tool = Pointer, 如果已有标注被选择，返回
            if (annotationCanvas.Count > 0 && annotationCanvas.Tool == ToolType.Pointer)
            {
                int index = 1;
                foreach (GraphicsBase g in annotationCanvas.GraphicsList)
                {
                    Point _p = new Point();

                    switch (g.Page)
                    {
                        case (int)TiffPage.Page0:
                            _p = new Point(p.X, p.Y);
                            break;
                        case (int)TiffPage.Page1:
                            _p = new Point(p.X * 0.25, p.Y * 0.25);
                            break;
                        case (int)TiffPage.Page2:
                            _p = new Point(p.X * 0.0625, p.Y * 0.0625);
                            break;
                        default:

                            break;
                    }

                    if (g.Contains(_p))
                    {
                        return index;
                    }
                    index++;
                }
                return -1;
            }
            else
                return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Point posNow = new Point();
            posNow = e.GetPosition(ScrollImageViewer);
            if (_isSlideImageLoaded)
            {
                if (gridContainer.IsMouseOver)
                {
                    // 2017-08-24, gridContainer 居中缩小后，需要减去 gridContainer_offset 
                    double x = (posNow.X - gridContainer_xoffset + imageCanvas.HorizontalOffset) / _fszoom;
                    double y = (posNow.Y - gridContainer_yoffset + imageCanvas.VerticalOffset) / _fszoom;

                    _currentPositionOnTarget = new Point(x, y);
                }

                if ((posNow.X > 5 && posNow.X < 85) && (posNow.Y > 160 && posNow.Y < 310))
                {
                    zoombarCanvas.IsHitTestVisible = true;
                }
                else
                    zoombarCanvas.IsHitTestVisible = false;

                if ((posNow.X > gridViewer.Width - 5 - thumbnailCanvas.Width && posNow.X < gridViewer.Width - 5) && (posNow.Y > 5 && posNow.Y < 5 + thumbnailCanvas.Height))
                {
                    thumbnailCanvas.IsHitTestVisible = true;
                    thumbnailZoomRectThumb.IsHitTestVisible = true;
                }
                else
                {
                    thumbnailCanvas.IsHitTestVisible = false;
                    thumbnailZoomRectThumb.IsHitTestVisible = false;
                }
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            // 鼠标在标注上，才能显示右键，进行标注操作
            annotationCanvas.IsMouseMoveOnGraphics = IsMouseMoveOnAnnotation(_currentPositionOnTarget);

            if (annotationCanvas.IsHitTestVisible)
                annotationCanvas.IsHitTestVisible = false;

            if (annotationCanvas.Tool != ToolType.Pointer)
                annotationCanvas.Tool = ToolType.Pointer;
        }

        // 重点
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsSlideImageLoaded == false)
            {
                annotationCanvas.Tool = ToolType.None;
                return;
            }

            int index = MouseMoveOnAnnotation(_currentPositionOnTarget);
            if (index >= 0)
            {
                annotationCanvas.UnselectAll();
                GraphicsBase gb = (GraphicsBase)annotationCanvas.GraphicsList[index - 1];
                gb.IsSelected = true;
                annotationCanvas.IsHitTestVisible = true;
            }
            else
            {
                annotationCanvas.UnselectAll();
                annotationCanvas.IsHitTestVisible = false;
                imageCanvas.Focus(); 
            }
        }
     

        /// <summary>
        ///   Scrolls the control to the given point in the image, offset at the specified display point
        /// </summary>
        /// <param name="imageLocation">The point of the image to attempt to scroll to (Center Location).</param>
        /// <param name="relativeDisplayPoint">The relative display point to offset scrolling by.</param>
        public virtual void ScrollToImageCenter(Point imageLocation, Point relativeDisplayPoint)
        {
            double x = (imageLocation.X * this.FullScaleZoom) - relativeDisplayPoint.X;
            double y = (imageLocation.Y * this.FullScaleZoom) - relativeDisplayPoint.Y;

            imageCanvas.SetHorizontalOffset(x);
            imageCanvas.SetVerticalOffset(y);
        }

        private void ScrollImageViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (annotationCanvas.SelectionCount == 0)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        imageCanvas.LineUp();
                        break;
                    case Key.Down:
                        imageCanvas.LineDown();
                        break;
                    case Key.Left:
                        imageCanvas.LineLeft();
                        break;
                    case Key.Right:
                        imageCanvas.LineRight();
                        break;
                    case Key.PageDown:
                        imageCanvas.PageDown();
                        break;
                    case Key.PageUp:
                        imageCanvas.PageUp();
                        break;
                    default:
                        break;
                }
            }

            double offset = imageCanvas.VerticalOffset;
        }
        #endregion

        #region ImageMagnifier
        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isSlideImageLoaded)
            {
                Point rate = new Point(2, 2);
                Point pos = e.MouseDevice.GetPosition(gridViewer);  // 相对于outsideGrid获取鼠标的坐标    
                Rect viewBox = vbMagnifier.Viewbox;   // 这里的Viewbox和前台的一样, 这里就是获取前台Viewbox的值  

                // 因为鼠标要让它在矩形(放大镜)的中间  那么我们就要让矩形的左上角重新移动位置  
                double xoffset = magnifierEllipse.ActualWidth / 2;
                double yoffset = magnifierEllipse.ActualHeight / 2;

                viewBox.X = pos.X - xoffset + (magnifierEllipse.ActualWidth - magnifierEllipse.ActualWidth / rate.X) / 2;
                viewBox.Y = pos.Y - yoffset + (magnifierEllipse.ActualHeight - magnifierEllipse.ActualHeight / rate.Y) / 2;
                vbMagnifier.Viewbox = viewBox;

                Canvas.SetLeft(magnifierCanvas, pos.X - xoffset);  // 同理重新定位magnifierCanvas的坐标    
                Canvas.SetTop(magnifierCanvas, pos.Y - yoffset);
            }
        }
        #endregion

        #region ZoomBar

        /// <summary>
        /// This event is raised when the ZoomBar 1X Button is clicked.
        /// </summary>        
        public event EventHandler ZoomSliderClicked;

        /// <summary>
        /// This event is raised when the ZoomBar Fit Button is clicked.
        /// </summary>        
        public event EventHandler ZoomFitClicked;

        /// <summary>
        /// This event is raised when the ZoomBar 1X Button is clicked.
        /// </summary>        
        public event EventHandler Zoom1XClicked;

        /// <summary>
        /// This event is raised when the ZoomBar 2X Button is clicked.
        /// </summary>        
        public event EventHandler Zoom2XClicked;

        /// <summary>
        /// This event is raised when the ZoomBar 4X Button is clicked.
        /// </summary>        
        public event EventHandler Zoom4XClicked;

        /// <summary>
        /// This event is raised when the ZoomBar 5X Button is clicked.
        /// </summary>        
        public event EventHandler Zoom5XClicked;

        /// <summary>
        /// This event is raised when the ZoomBar 10X Button is clicked.
        /// </summary>        
        public event EventHandler Zoom10XClicked;

        /// <summary>
        /// This event is raised when the ZoomBar 20X Button is clicked.
        /// </summary>        
        public event EventHandler Zoom20XClicked;


        private void btnZoomFit_Click(object sender, RoutedEventArgs e)
        {
            if (ZoomFitClicked != null) ZoomFitClicked(this, EventArgs.Empty);
        }
        private void btnZoom1x_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom1XClicked != null) Zoom1XClicked(this, EventArgs.Empty);
        }

        private void btnZoom2x_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom2XClicked != null) Zoom2XClicked(this, EventArgs.Empty);
        }

        private void btnZoom4x_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom4XClicked != null) Zoom4XClicked(this, EventArgs.Empty);
        }

        private void btnZoom5x_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom5XClicked != null) Zoom5XClicked(this, EventArgs.Empty);
        }

        private void btnZoom10x_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom10XClicked != null) Zoom10XClicked(this, EventArgs.Empty);
        }

        private void btnZoom20x_Click(object sender, RoutedEventArgs e)
        {
            if (Zoom20XClicked != null) Zoom20XClicked(this, EventArgs.Empty);
        }

        private void zoomSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ZoomSliderClicked != null) ZoomSliderClicked(this, EventArgs.Empty);
        }

        #endregion

        #region LabelImage

        public void UpdateLabelImage(BitmapImage bmp)
        {
            // Test for label image         
            bmp = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/LabelImage.png"));

            Image img = new Image();
            img.Source = bmp;
            if (bmp.Width <= 166 && bmp.Height <= 160)
                labelImageCanvas.Children.Add(img);
        }

        #endregion

        #region Thumbnail Canvas

        private const int _thumbnailWindowsWidth = 250;
        private const int _thumbnailWindowsHeight = 250;

        public double _thumbnailWidth;
        public double _thumbnailHeight;

        public double ThumbnailScale { get; set; }
        // ZoomRect

        public Point ZoomRectThumbOffset;
        public double PageRate { get; set; }

        private double _pageImageWidth;
        private double _pageImageHeight;

        private ImageSource _thumbnailImage;

        public double PageImageWidth
        {
            set
            {
                _pageImageWidth = value;
            }
            get
            {
                return _pageImageWidth;
            }
        }
     
        public double PageImageHeight
        {
            set
            {
                _pageImageHeight = value;
            }
            get
            {
                return _pageImageHeight;
            }
        }

        public ImageSource ThumbnailImage
        {
            get
            {
                return _thumbnailImage;
            }
            set
            {
                if (value.Width <= _thumbnailWindowsWidth && value.Height <= _thumbnailWindowsHeight)
                {
                    _thumbnailImage = value;
                    ThumbnailImage_Brush.ImageSource = value;
                    _thumbnailWidth = _thumbnailImage.Width;
                    thumbnailCanvas.Width = _thumbnailImage.Width;
                    _thumbnailHeight = _thumbnailImage.Height;
                    thumbnailCanvas.Height = _thumbnailImage.Height;
                }
            }
        }

        public void ModifyThumbDisplay(double Horizontal, double Vertical, double width, double height)
        {     
            ZoomRectThumbOffset.X = Horizontal - thumbnailZoomRectThumb.Width / 2;
            ZoomRectThumbOffset.Y = Vertical - thumbnailZoomRectThumb.Height / 2;

            if (ZoomRectThumbOffset.X < 0)
            {
                thumbnailZoomRectThumb.Width = ZoomRectThumbOffset.X + thumbnailZoomRectThumb.Width;
                ZoomRectThumbOffset.X = 0;
            }

            if (ZoomRectThumbOffset.Y < 0)
            {
                thumbnailZoomRectThumb.Height = ZoomRectThumbOffset.Y + thumbnailZoomRectThumb.Height;
                ZoomRectThumbOffset.Y = 0;
            }

            if (width > 0 && height > 0)
            {
                if ((ZoomRectThumbOffset.X + width) > thumbnailCanvas.Width)
                    thumbnailZoomRectThumb.Width = (thumbnailCanvas.Width - ZoomRectThumbOffset.X) < 0 ? 0 : (thumbnailCanvas.Width - ZoomRectThumbOffset.X);
                else
                    thumbnailZoomRectThumb.Width = width;

                if ((ZoomRectThumbOffset.Y + height) > thumbnailCanvas.Height)
                {
                    thumbnailZoomRectThumb.Height = (thumbnailCanvas.Height - ZoomRectThumbOffset.Y) < 0 ? 0 : (thumbnailCanvas.Height - ZoomRectThumbOffset.Y);
                }
                else
                    thumbnailZoomRectThumb.Height = height;
            }

            Canvas.SetLeft(thumbnailZoomRectThumb, ZoomRectThumbOffset.X);
            Canvas.SetTop(thumbnailZoomRectThumb, ZoomRectThumbOffset.Y);
        }

        public void RectThumbInit(double scale)
        {
            Canvas.SetLeft(thumbnailZoomRectThumb, 0);
            Canvas.SetTop(thumbnailZoomRectThumb, 0);
            ZoomRectThumbOffset.X = 0;
            ZoomRectThumbOffset.Y = 0;
            ThumbnailScale = scale;
        }

        public void RectThumbChangeWithZoom(double zoom)
        {
            thumbnailZoomRectThumb.Width = thumbnailZoomRectThumb.Width / PageRate;
            thumbnailZoomRectThumb.Height = thumbnailZoomRectThumb.Height / PageRate;
        }

        private void thumbnailZoomRectThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            //
            // Update the position of the overview rect as the user drags it around.
            //     
             ZoomRectThumbOffset.X = Canvas.GetLeft(thumbnailZoomRectThumb) + e.HorizontalChange;

            if (ZoomRectThumbOffset.X > _thumbnailWindowsWidth)
                ZoomRectThumbOffset.X = _thumbnailWindowsWidth;

            if (ZoomRectThumbOffset.X<0)
                ZoomRectThumbOffset.X = 0;

            Canvas.SetLeft(thumbnailZoomRectThumb, ZoomRectThumbOffset.X);

            ZoomRectThumbOffset.Y = Canvas.GetTop(thumbnailZoomRectThumb) + e.VerticalChange;

            if (ZoomRectThumbOffset.Y > _thumbnailWindowsHeight)
                ZoomRectThumbOffset.Y = _thumbnailWindowsHeight;

            if (ZoomRectThumbOffset.Y < 0)
                ZoomRectThumbOffset.Y = 0;

            Canvas.SetTop(thumbnailZoomRectThumb, ZoomRectThumbOffset.Y);
           
            if (ThumbnailScale > 0)
            {
                switch (_currentPage)
                {
                    case (int)TiffPage.Page0:
                        imageCanvas.SetScrollOffset(ZoomRectThumbOffset.X / ThumbnailScale, ZoomRectThumbOffset.Y / ThumbnailScale);
                        break;
                    case (int)TiffPage.Page1:
                        imageCanvas.SetScrollOffset(ZoomRectThumbOffset.X * 0.25 / ThumbnailScale, ZoomRectThumbOffset.Y * 0.25 / ThumbnailScale);
                        break;
                    case (int)TiffPage.Page2:
                        imageCanvas.SetScrollOffset(ZoomRectThumbOffset.X * 0.0625 / ThumbnailScale, ZoomRectThumbOffset.Y * 0.0625 / ThumbnailScale);
                        break;
                    default:

                        break;
                }
            }
        }

        private void thumbnailCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 中心点
            Point p = e.GetPosition(thumbnailCanvas);

            ZoomRectThumbOffset.X = p.X - thumbnailZoomRectThumb.Width / 2;
            ZoomRectThumbOffset.Y = p.Y - thumbnailZoomRectThumb.Height / 2;                    

            if (ZoomRectThumbOffset.X < 0)
            {
                thumbnailZoomRectThumb.Width = thumbnailZoomRectThumb.Width + ZoomRectThumbOffset.X;
                ZoomRectThumbOffset.X = 0;
            }

            if (ZoomRectThumbOffset.Y < 0)
            {
                thumbnailZoomRectThumb.Height = thumbnailZoomRectThumb.Height + ZoomRectThumbOffset.Y;
                ZoomRectThumbOffset.Y = 0;
            }

            if ((ZoomRectThumbOffset.X + thumbnailZoomRectThumb.Width) >= thumbnailCanvas.Width)
                thumbnailZoomRectThumb.Width = (thumbnailCanvas.Width - ZoomRectThumbOffset.X);

            if ((ZoomRectThumbOffset.Y + thumbnailZoomRectThumb.Height) >= thumbnailCanvas.Height)
                thumbnailZoomRectThumb.Height = (thumbnailCanvas.Height - ZoomRectThumbOffset.Y);

            if (ThumbnailScale > 0)
            {
                ScrollToImageCenter(new Point(p.X / ThumbnailScale, p.Y / ThumbnailScale), new Point(ScrollImageViewer.ViewportWidth / 2, ScrollImageViewer.ViewportHeight / 2));

                Canvas.SetLeft(thumbnailZoomRectThumb, ZoomRectThumbOffset.X);
                Canvas.SetTop(thumbnailZoomRectThumb, ZoomRectThumbOffset.Y);
            }

        }
        public void UpdateThumbnailImage(BitmapImage bmp)
        {
            // Test for label image         
            bmp = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/OverView.png"));

            ThumbnailImage_Brush.ImageSource = bmp;
            _thumbnailWidth = bmp.Width;
            thumbnailBorder.Width = thumbnailCanvas.Width = bmp.Width;
            _thumbnailHeight = bmp.Height;
            thumbnailBorder.Height = thumbnailCanvas.Height = bmp.Height;
        }
        #endregion

    }
}
