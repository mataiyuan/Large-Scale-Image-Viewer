using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Globalization;
using System.Collections.ObjectModel;

using BitMiracle.LibTiff.Classic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;

using DrawToolsLib;
using VirtualCanvasLib;
using Utilities;
using System.Xml;


namespace mSlideViewer
{
    /// <summary>
    /// Context menu command types
    /// </summary>
    internal enum RisCaptureContextMenuCommand
    {
        SaveAs,
        SaveToSnapShot,
        Exit,
    };

    /// <summary>
    /// Microscope Magnifier
    /// </summary>
    public enum Magnifier
    {
        None,
        X2,
        X10,
        X20,
        X40
    };


    /// <summary>
    /// ImageFileType
    /// </summary>
    public enum ImageFileType
    {
        mSlideImageFile,
        AperioImageFile
    };

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region RisCapture
        private readonly RisCaptureLib.ScreenCaputre screenCaputre = new RisCaptureLib.ScreenCaputre();
        private Size? lastSize;
        #endregion

        #region DrawTool Members
        WindowStateManager windowStateManager;
        MruManager mruManager;
        string anotFileName;    // name of currently opened annotation file

        public DrawingCanvas annotationCanvas;
        #endregion

        #region ImageViewer Members

        int _tileWidth = 256;
        int _tileHeight = 256;

        double previousZoom = 0;

        public TileTiffRead tileTiffReader;
        public Tiff inputTiffFile;
        public AperioTileTiffRead aperioTileTiffRead;
        VirtualSlideInfo vsInfo;
        string tiffFileName;

        bool ivCameraPreviewMouseLeftButtonDown = false;
        Point mouseDownPoint;

        double _increment = 10;
        MapZoom mapZoom;
        Pan pan;
        AutoScroll autoScroll;
        RectangleSelectionGesture rectZoom;
        ImageFileType imagefileType;
        VirtualCanvas imageCanvas;
        public Magnifier magnifier;
        int index_recentFiles;

        double curren_zoomslide_value;
        string pathinfo = "D:/大创/3层数据/test/";
        int originImageWidth;
        int originImageHeight;
        #endregion

        public MainWindow()
        {
            // Create WindowStateManager and associate is with ApplicationSettings.MainWindowStateInfo.
            // This allows to set initial window state and track state changes in
            // the Settings.MainWindowStateInfo instance.
            // When application is closed, ApplicationSettings is saved with new window state
            // information. Next time this information is loaded from XML file.
            windowStateManager = new WindowStateManager(SettingsManager.ApplicationSettings.MainWindowStateInfo, this);

            InitializeComponent();

            imagefileType = ImageFileType.mSlideImageFile;


            // App Configuration Loaded
            this.WindowState = WindowState.Maximized;

            curren_zoomslide_value = 0;

            string settingFileName = SettingsManager.SettingsFileName;
            if (!File.Exists(settingFileName))
            {
                // 创建 Settings.xml
                SettingsManager.OnExit();
            }
            else
                SettingsManager.OnStartup();

            // ImageViewer
            ivCamera.CurrentPage = 0;

            // Localization
            tileTiffReader = new TileTiffRead(_tileWidth, _tileHeight);
            ivCamera.IsSlideImageLoaded = false;

            VirtualCanvasInitialize();
            DrawToolsInitialize();

            // RisCaptureLib
            screenCaputre.ScreenCaputred += OnScreenCaputred;
            screenCaputre.ScreenCaputreCancelled += OnScreenCaputreCancelled;

            // 远程视频会议(Conference Meeting)


            // 显微放大倍数
            magnifier = Magnifier.X20;

            // 界面相关         
            txDrawToolsTextColor.Background = new SolidColorBrush(annotationCanvas.ObjectColor);

            menuAnnotationTools.IsEnabled = false;
            menuCloseImage.IsEnabled = false;
            menuSlideImage.IsEnabled = false;

            buttonToolPointer.IsEnabled = false;
            buttonToolRectangle.IsEnabled = false;
            buttonToolEllipse.IsEnabled = false;
            buttonToolLine.IsEnabled = false;
            buttonToolArrow.IsEnabled = false;
            buttonToolRuler.IsEnabled = false;
            buttonToolPencil.IsEnabled = false;
            buttonToolText.IsEnabled = false;

            this.ivCamera.imageCanvas.VisualRegionChanged += new RoutedEventHandler(ImageView_VisualRegionChange);

            this.ivCamera.ZoomFitClicked += new EventHandler(ImageView_ZoomFit);
            this.ivCamera.Zoom1XClicked += new EventHandler(ImageView_Zoom1X);
            this.ivCamera.Zoom2XClicked += new EventHandler(ImageView_Zoom2X);
            this.ivCamera.Zoom4XClicked += new EventHandler(ImageView_Zoom4X);
            this.ivCamera.Zoom5XClicked += new EventHandler(ImageView_Zoom5X);
            this.ivCamera.Zoom10XClicked += new EventHandler(ImageView_Zoom10X);
            this.ivCamera.Zoom20XClicked += new EventHandler(ImageView_Zoom20X);

            this.ivCamera.ZoomSliderClicked += new EventHandler(ImageView_ZoomSlider);
        }

        int countq = 0;
        private void ImageView_VisualRegionChange(object sender, EventArgs e)
        {
            countq++;
            tbMsg.Text = countq.ToString();
        }

        #region ImageView_ZoomBar
        private void ImageView_ZoomFit(object sender, EventArgs e)
        {
            MessageBox.Show("Hi,Fit");
        }


        private void ImageView_Zoom1X(object sender, EventArgs e)
        {
            if (ivCamera.IsSlideImageLoaded == true)
            {
                // 获取The Offset of Viewer      
                double hcp = (ivCamera.imageCanvas.HorizontalOffset + ivCamera.ScrollImageViewer.ViewportWidth / 2) /  ivCamera.FullScaleZoom;
                double vcp = (ivCamera.imageCanvas.VerticalOffset + ivCamera.ScrollImageViewer.ViewportHeight / 2) / ivCamera.FullScaleZoom;


                if (ivCamera.FullScaleZoom > 0.0625)
                {
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, 2);
                }

                ivCamera.CurrentPage = 2;
                ivCamera.FullScaleZoom = 0.0625;
                mapZoom.Zoom = 1;

                ivCamera.zoomSlider.Value = 12;

                annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                annotationCanvas.Page = 2;

                ivCamera.lbZoom.Content = "0.062500";

                AdjustViewToPosition(new Point(hcp, vcp));
            }
        }

        private void ImageView_Zoom2X(object sender, EventArgs e)
        {
            if (ivCamera.IsSlideImageLoaded == true)
            {
                // 获取The center of Viewer      
                double hcp = (ivCamera.imageCanvas.HorizontalOffset + ivCamera.ScrollImageViewer.ViewportWidth / 2) / ivCamera.FullScaleZoom;
                double vcp = (ivCamera.imageCanvas.VerticalOffset + ivCamera.ScrollImageViewer.ViewportHeight / 2) / ivCamera.FullScaleZoom;

                if (ivCamera.FullScaleZoom <= 1 && ivCamera.FullScaleZoom > 0.25)
                {
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, 1);
                }
                else if (ivCamera.FullScaleZoom <= 0.0625)
                {
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, 1);
                }

                ivCamera.CurrentPage = 1;
                ivCamera.FullScaleZoom = 0.1171875;
                mapZoom.Zoom = 0.46875;  //  ivCamera.FullScaleZoom * 4;  

                ivCamera.zoomSlider.Value = 10;

                annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                annotationCanvas.Page = 1;

                ivCamera.lbZoom.Content = "0.125000";

                AdjustViewToPosition(new Point(hcp, vcp));
            }
        }

        private void ImageView_Zoom4X(object sender, EventArgs e)
        {
            if (ivCamera.IsSlideImageLoaded == true)
            {
                // 获取The Offset of Viewer      
                double hcp = (ivCamera.imageCanvas.HorizontalOffset + ivCamera.ScrollImageViewer.ViewportWidth / 2) / ivCamera.FullScaleZoom;
                double vcp = (ivCamera.imageCanvas.VerticalOffset + ivCamera.ScrollImageViewer.ViewportHeight / 2) / ivCamera.FullScaleZoom;

                if (ivCamera.FullScaleZoom <= 1 && ivCamera.FullScaleZoom > 0.25)
                {
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, 1);
                }
                else if (ivCamera.FullScaleZoom <= 0.0625)
                {
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, 1);
                }

                ivCamera.CurrentPage = 1;
                ivCamera.FullScaleZoom = 0.1875;
                mapZoom.Zoom = 0.75; // ivCamera.FullScaleZoom * 4;                

                ivCamera.zoomSlider.Value = 8;

                annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                annotationCanvas.Page = 1;

                ivCamera.lbZoom.Content = "0.187500";

                AdjustViewToPosition(new Point(hcp, vcp));

                /*
                switch (magnifier)
                {
                    case Magnifier.X20:
                        {
                            ivCamera.lbZoom.Content = "3.75x";
                            break;
                        }
                    case Magnifier.X40:
                        {
                            ivCamera.lbZoom.Content = "7.5x";
                            break;
                        }
                }
                */
            }
        }

        private void ImageView_Zoom5X(object sender, EventArgs e)
        {
            if (ivCamera.IsSlideImageLoaded == true)
            {
                // 获取The center of Viewer      
                double hcp = (ivCamera.imageCanvas.HorizontalOffset + ivCamera.ScrollImageViewer.ViewportWidth / 2) / ivCamera.FullScaleZoom;
                double vcp = (ivCamera.imageCanvas.VerticalOffset + ivCamera.ScrollImageViewer.ViewportHeight / 2) / ivCamera.FullScaleZoom;

                if (ivCamera.FullScaleZoom <= 1 && ivCamera.FullScaleZoom > 0.25)
                {
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, 1);
                }
                else if (ivCamera.FullScaleZoom <= 0.0625)
                {
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, 1);
                }

                ivCamera.CurrentPage = 1;
                ivCamera.FullScaleZoom = 0.25;
                mapZoom.Zoom = 1.0;

                ivCamera.zoomSlider.Value = 6;

                annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                annotationCanvas.Page = 1;

                ivCamera.lbZoom.Content = "0.250000";

                AdjustViewToPosition(new Point(hcp, vcp));
            }
        }
        private void ImageView_Zoom10X(object sender, EventArgs e)
        {
            if (ivCamera.IsSlideImageLoaded == true)
            {
                double hcp = (ivCamera.imageCanvas.HorizontalOffset + ivCamera.ScrollImageViewer.ViewportWidth / 2) / ivCamera.FullScaleZoom;
                double vcp = (ivCamera.imageCanvas.VerticalOffset + ivCamera.ScrollImageViewer.ViewportHeight / 2) / ivCamera.FullScaleZoom;

                if (ivCamera.FullScaleZoom <= 0.25)
                {
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, 0);
                }

                ivCamera.CurrentPage = 0;
                ivCamera.FullScaleZoom = 0.5;
                mapZoom.Zoom = 0.5;

                ivCamera.zoomSlider.Value = 4;

                annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                annotationCanvas.Page = 0;

                ivCamera.lbZoom.Content = "0.500000";

                AdjustViewToPosition(new Point(hcp, vcp));
            }
        }

        private void ImageView_Zoom20X(object sender, EventArgs e)
        {
            if (ivCamera.IsSlideImageLoaded == true)
            {
                // 获取The center of Viewer      
                double hcp = (ivCamera.imageCanvas.HorizontalOffset + ivCamera.ScrollImageViewer.ViewportWidth / 2) / ivCamera.FullScaleZoom;
                double vcp = (ivCamera.imageCanvas.VerticalOffset + ivCamera.ScrollImageViewer.ViewportHeight / 2) / ivCamera.FullScaleZoom;

                if (ivCamera.FullScaleZoom <= 0.25)
                {
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, 0);
                }

                ivCamera.CurrentPage = 0;
                ivCamera.FullScaleZoom = 1.0;
                mapZoom.Zoom = 1.0;
                ivCamera.zoomSlider.Value = 0;

                annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                annotationCanvas.Page = 0;

                ivCamera.lbZoom.Content = "1.000000";

                AdjustViewToPosition(new Point(hcp, vcp));
            }
        }

        private void ImageView_ZoomSlider(object sender, EventArgs e)
        {
            if (ivCamera.IsSlideImageLoaded == true)
            {
                double hcp = (ivCamera.imageCanvas.HorizontalOffset + ivCamera.ScrollImageViewer.ViewportWidth / 2) / ivCamera.FullScaleZoom;
                double vcp = (ivCamera.imageCanvas.VerticalOffset + ivCamera.ScrollImageViewer.ViewportHeight / 2) / ivCamera.FullScaleZoom;

                if (curren_zoomslide_value < ivCamera.zoomSlider.Value)
                {
                    ZoomOut(new Point(hcp,vcp));  // 缩小
                }
                else
                {
                    ZoomIn(new Point(hcp, vcp));   // 放大
                }

                ivCamera.lbZoom.Content = ivCamera.FullScaleZoom.ToString();
                curren_zoomslide_value = ivCamera.zoomSlider.Value;
            }
        }

        private void ChangeZoomBarDisplay()
        {
            //switch (ivCamera.FullScaleZoom)
            //{
            //    //Page 0
            //    case 1.0: ivCamera.zoomSlider.Value = 0; break;
            //    case 0.875000: ivCamera.zoomSlider.Value = 1; break;
            //    case 0.750000: ivCamera.zoomSlider.Value = 2; break;
            //    case 0.625000: ivCamera.zoomSlider.Value = 3; break;
            //    case 0.500000: ivCamera.zoomSlider.Value = 4; break;
            //    case 0.375000: ivCamera.zoomSlider.Value = 5; break;
            //    // Page 1
            //    case 0.250000: ivCamera.zoomSlider.Value = 6; break;
            //    case 0.218750: ivCamera.zoomSlider.Value = 7; break;
            //    case 0.187500: ivCamera.zoomSlider.Value = 8; break;
            //    case 0.156250: ivCamera.zoomSlider.Value = 9; break;
            //    case 0.125000: ivCamera.zoomSlider.Value = 10; break;
            //    case 0.093750: ivCamera.zoomSlider.Value = 11; break;
            //    // Page 2       
            //    case 0.062500: ivCamera.zoomSlider.Value = 12; break;
            //    case 0.0546875: ivCamera.zoomSlider.Value = 13; break;
            //    case 0.046875: ivCamera.zoomSlider.Value = 14; break;
            //    case 0.0390625: ivCamera.zoomSlider.Value = 15; break;
            //    case 0.031250: ivCamera.zoomSlider.Value = 16; break;
            //    case 0.0234375: ivCamera.zoomSlider.Value = 17; break;
            //}
        }

        #endregion
        private void VirtualCanvasInitialize()
        {
            // VirtualCanvas
            imageCanvas = ivCamera.imageCanvas;
            imageCanvas.ScrollOwner = ivCamera.ScrollImageViewer;
            imageCanvas.SmallScrollIncrement = new Size(_increment, _increment);

            //Scroller.Content = grid;
            object v = ivCamera.ScrollImageViewer.GetValue(ScrollViewer.CanContentScrollProperty);

            Canvas targetImage = imageCanvas.ContentCanvas;

            mapZoom = new MapZoom(targetImage);
            pan = new Pan(targetImage, imageCanvas, mapZoom);

            // 鼠标中键拖动图像
            autoScroll = new AutoScroll(targetImage, mapZoom);

            // CTRL + Rectangle (Ctrl+鼠标选择区域放大)
            rectZoom = new RectangleSelectionGesture(targetImage, mapZoom, ModifierKeys.Control);
            rectZoom.ZoomSelection = true;

            mapZoom.ZoomChanged += new EventHandler(OnZoomChanged);
            imageCanvas.Scale.Changed += new EventHandler(OnScaleChanged);

            imageCanvas.Translate.Changed += new EventHandler(OnTranslateChanged);

            // 改变背景色，彻底取消网格线
        //      imageCanvas.Background = new SolidColorBrush(Color.FromRgb(0xd0, 0xd0, 0xd0));             
        //      imageCanvas.ContentCanvas.Background = Brushes.White; 

              imageCanvas.Background = Brushes.Transparent;
              imageCanvas.ContentCanvas.Background = Brushes.Transparent;


            //   imageCanvas.Background = Brushes.Transparent;
            // imageCanvas.ContentCanvas.Background = Brushes.Transparent;


        }

        private void DrawToolsInitialize()
        {
            // Draw Tool
            annotationCanvas = ivCamera.annotationCanvas;

            SubscribeToEvents();
            UpdateTitle(tiffFileName, anotFileName);
            InitializedrawingCanvas();
            InitializePropertiesControls();
            InitializeMruList();
            annotationCanvas.ScrollOwner = ivCamera.ScrollImageViewer;

            annotationCanvas.GraphicListChanged += OnGraphicListChanged;
            annotationCanvas.SmallScrollIncrement = new Size(_increment, _increment);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            btThumbnail.IsChecked = true;
            menuThumbnail.IsChecked = true;

            btZoomBar.IsChecked = true;
            menuZoomBar.IsChecked = true;

            btMagnifier.IsChecked = true;
            menuMagnifier.IsChecked = true;
            menuStatusBar.IsChecked = true;

            btSlideLabel.IsChecked = true;
            menuSlideLabel.IsChecked = true;

            // Expand Tree View(tvICDO);
            foreach (object item in tvICDO.Items)
            {
                TreeViewItem treeItem = tvICDO.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null)
                    ExpandAll(treeItem, true);
                treeItem.IsExpanded = true;
            }
        }

        private void ivCamera_ScrollChanged(object sender, EventArgs e)
        {


        }

        #region VirtualCanvas for whole slide image

        IList<TissueSlideTile> Page0_Shapes = new List<TissueSlideTile>();
        IList<TissueSlideTile> Page1_Shapes = new List<TissueSlideTile>();
        IList<TissueSlideTile> Page2_Shapes = new List<TissueSlideTile>();
        ObservableCollection<IVirtualChild> vc_children0 = new ObservableCollection<IVirtualChild>();
        ObservableCollection<IVirtualChild> vc_children1 = new ObservableCollection<IVirtualChild>();
        ObservableCollection<IVirtualChild> vc_children2 = new ObservableCollection<IVirtualChild>();
        // 2017-08-11 吴开杰，提前加载 Shap 和 Node’s Children
        private void AllocateShaps(TileTiffRead tiffReader, Tiff tiffFile, byte[] tileTiffBuffer)
        {
            if (tiffFile != null && tiffReader != null)
            {
                for (int page = 0; page < 3; page++)
                {
                    int rows = tiffReader.pageLevel[page].Tile_YCount;
                    int cols = tiffReader.pageLevel[page].Tile_XCount;
                    Size s = new Size(_tileWidth, _tileHeight);
                    Point pos;
                    TissueSlideTile shape;

                    for (int x = 0; x < cols; x++)
                    {
                        for (int y = 0; y < rows; y++)
                        {
                            pos = new Point(x * _tileWidth, y * _tileHeight);
                            shape = new TissueSlideTile(new Rect(pos, s), _tileWidth, _tileHeight,pathinfo);

                            shape.TileTiffBuffer = tileTiffBuffer;
                            shape.CurrentPage = page;

                            switch (page)
                            {
                                case (int)TiffPage.Page0:
                                    Page0_Shapes.Add(shape);
                                    break;
                                case (int)TiffPage.Page1:
                                    Page1_Shapes.Add(shape);
                                    break;
                                case (int)TiffPage.Page2:
                                    Page2_Shapes.Add(shape);
                                    break;
                            }
                        }
                    }
                    // T400 耗时 Page0_Shapes 70ms [48×48 Tiles]; Page1_Shapes 1.5ms [12×12 Tiles]; Page2_Shapes 96us [3×3 Tiles];                 
                }
            }
            else
            {
                Page0_Shapes = null;
                Page1_Shapes = null;
                Page2_Shapes = null;
            }

            for (int s = 0; s < Page0_Shapes.Count; s++)
                vc_children0.Add(Page0_Shapes[s]);

            for (int s = 0; s < Page1_Shapes.Count; s++)
                vc_children1.Add(Page1_Shapes[s]);


            for (int s = 0; s < Page2_Shapes.Count; s++)
                vc_children2.Add(Page2_Shapes[s]);
        }

        public void AllocateNodes(ref VirtualCanvas vCanvas, TileTiffRead tiffReader, Tiff tiffFile, int page)
        {
            double Xoffset = imageCanvas.HorizontalOffset;
            double Yoffset = imageCanvas.VerticalOffset;

            if (vCanvas != null && tiffFile != null && tiffReader != null && page <= 2 && page >= 0)
            {
                switch (page)
                {
                    case (int)TiffPage.Page0:
                        {
                            vCanvas.VirtualChildren = vc_children0;
                            vCanvas.CalculateExtent();
                            annotationCanvas.SetValue(DrawingCanvas.ActualScaleProperty, ivCamera.FullScaleZoom);
                            annotationCanvas.SetValue(DrawingCanvas.PageProperty, 0);
                            break;
                        }
                    case (int)TiffPage.Page1:
                        {
                            vCanvas.VirtualChildren = vc_children1;
                            vCanvas.CalculateExtent();

                            annotationCanvas.SetValue(DrawingCanvas.ActualScaleProperty, ivCamera.FullScaleZoom);
                            annotationCanvas.SetValue(DrawingCanvas.PageProperty, 1);
                            break;
                        }
                    case (int)TiffPage.Page2:
                        {
                            vCanvas.VirtualChildren = vc_children2;
                            vCanvas.CalculateExtent();
                            annotationCanvas.SetValue(DrawingCanvas.ActualScaleProperty, ivCamera.FullScaleZoom);
                            annotationCanvas.SetValue(DrawingCanvas.PageProperty, 2);
                            break;
                        }
                }

                // MaTaiyuan
                imageCanvas.SetHorizontalOffset(Xoffset);
                imageCanvas.SetVerticalOffset(Yoffset + 30);

                // T400 耗时 vCanvas.AddVirtualChild Page0 125ms [48×48 Tiles],加载每个Shap约 55us.
            }
        }

        void OnZoomChanged(object sender, EventArgs e)
        {
            mesZoom.Content = "Zoom: " + ivCamera.CurrentPage.ToString() + " | " + ivCamera.FullScaleZoom.ToString("F6") + " | " + mapZoom.Zoom.ToString("F6");
        }

        void OnScaleChanged(object sender, EventArgs e)
        {            
            double scale2 = ivCamera.ScrollImageViewer.ViewportWidth / (ivCamera.ScrollImageViewer.ViewportWidth + ivCamera.ScrollImageViewer.ViewportHeight);

            ivCamera.ModifyThumbDisplay(imageCanvas.HorizontalOffset * ivCamera.ThumbnailScale / ivCamera.FullScaleZoom,
                imageCanvas.VerticalOffset * ivCamera.ThumbnailScale / ivCamera.FullScaleZoom, 40 * scale2 / ivCamera.FullScaleZoom, 40 * (1 - scale2) / ivCamera.FullScaleZoom);
        }

        void OnTranslateChanged(object sender, EventArgs e)
        {            
            double scale2 = ivCamera.ScrollImageViewer.ViewportWidth / (ivCamera.ScrollImageViewer.ViewportWidth + ivCamera.ScrollImageViewer.ViewportHeight);           

            ivCamera.ModifyThumbDisplay(imageCanvas.HorizontalOffset * ivCamera.ThumbnailScale / ivCamera.FullScaleZoom,
                imageCanvas.VerticalOffset* ivCamera.ThumbnailScale / ivCamera.FullScaleZoom, 40 * scale2 / ivCamera.FullScaleZoom, 40 * (1 - scale2) / ivCamera.FullScaleZoom);
        }


        delegate void BooleanEventHandler(bool arg);

        protected string FormatPoint(Point point)
        {
            return string.Format("X:{0:N1}, Y:{1:N1}", point.X, point.Y);
        }


        private void ivCamera_MouseMove(object sender, MouseEventArgs e)
        {
            // Update Cursor Position On Target
            if (ivCamera.gridContainer.IsMouseOver)
            {
                Point CurrentPosition = ivCamera.CurrentPositionOnTarget;
                Point mousePosition = e.GetPosition(imageCanvas);
                mesOffset.Content = "Offset: [ " + this.FormatPoint(new Point(imageCanvas.HorizontalOffset, imageCanvas.VerticalOffset)) + " ]";
                mesPosition.Content = "Position: [ " + this.FormatPoint(CurrentPosition) + " ]";
                ivCamera.PageRate = ivCamera.FullScaleZoom;

                /*
                if (ivCamera.IsSlideImageLoaded)
                {
                    if (ivCameraPreviewMouseLeftButtonDown == true && !ivCamera.thumbnailCanvas.IsMouseOver)
                    {
                       // ivCamera.RectThumbMoveWithMouse((imageCanvas.HorizontalOffset) * ivCamera.ThumbnailScale / ivCamera.PageRate,
                       //    (imageCanvas.VerticalOffset) * ivCamera.ThumbnailScale / ivCamera.PageRate);

               //        ivCamera.RectThumbMoveWithMouse(CurrentPosition.X * ivCamera.ThumbnailScale,
               //            CurrentPosition.Y * ivCamera.ThumbnailScale);

                    }
                }
                */

            }
        }

        TiffStream ts = new TiffStream();

        private void ImageFileOpen_Click(object sender, RoutedEventArgs e)
        {
           // pathinfo = "D:/大创/3层数据/test/";
            // Create OpenFileDialog
            OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // dlg.InitialDirectory = @"E:\Projects\Pictures\WPF-SaperaImageBox-6\";
            // Set filter for file extension and default file extension      
            dlg.Filter = "Aperio Virtual Slide Tiff (*.svs)|*.svs|mSlide Virtual Slide Tiff (*.tif;*.tiff)|*.tif;*.tiff";

            dlg.FilterIndex = 2;

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                if (ivCamera.IsSlideImageLoaded)
                {
                    //可以不关闭
                    //  inputTiffFile.Close();
                    //  ts.Close(inputTiffFile);

                    if (anotFileName != null)
                    {
                        mruManager.Delete(anotFileName);
                    }
                    annotationCanvas.Clear();
                }

                tiffFileName = dlg.FileName;
                UpdateTitle(tiffFileName, "");
                FileInfo fi = new FileInfo(tiffFileName);

                inputTiffFile = Tiff.Open(tiffFileName, "r");

                //检测是否是Aperio图像
                aperioTileTiffRead = new AperioTileTiffRead(_tileWidth, _tileHeight, inputTiffFile);
                if (aperioTileTiffRead.IsAperioImage())
                {
                    imagefileType = ImageFileType.AperioImageFile;
                }
                else
                {
                    imagefileType = ImageFileType.mSlideImageFile;
                }

                _tileWidth = inputTiffFile.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                _tileHeight = inputTiffFile.GetField(TiffTag.TILELENGTH)[0].ToInt();

                // Open documen//      
                tileTiffReader = new TileTiffRead(_tileWidth, _tileHeight);
                byte[] buffer = File.ReadAllBytes(tiffFileName);

                /*                             
                    MemoryStream sourceStream = new MemoryStream(buffer);
                    inputTiffFile = Tiff.ClientOpen("In-Memory-Target", "r", sourceStream, ts);
                */

                if (inputTiffFile != null && inputTiffFile.IsTiled() == true)
                {
                    int dircount = 0;

                    do
                    {
                        dircount++;
                    } while (inputTiffFile.ReadDirectory());

                    switch (imagefileType)
                    {
                        case ImageFileType.AperioImageFile:

                            for (short page = 0; page < 3; page++)
                            {
                                if (page == 1 || page == 2)
                                    page = page++;

                                inputTiffFile.SetDirectory(page);
                                // Page，图像高度方向，Tile总数
                                int tile_height = inputTiffFile.GetField(TiffTag.TILELENGTH)[0].ToInt();
                                int image_height = inputTiffFile.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                                tileTiffReader.pageLevel[page].Page_Height = image_height;
                                tileTiffReader.pageLevel[page].Tile_YCount = image_height / tile_height;

                                // Page，图像宽带方向，Tile总数 
                                int tile_width = inputTiffFile.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                                int image_width = inputTiffFile.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                                tileTiffReader.pageLevel[page].Page_Width = image_width;
                                tileTiffReader.pageLevel[page].Tile_XCount = image_width / tile_width;

                                // 2017-03-21
                                ivCamera.PageImageWidth = image_width;
                                ivCamera.PageImageHeight = image_height;
                            }

                            break;
                        case ImageFileType.mSlideImageFile:

                            for (short page = 0; page < 3; page++)
                            {
                                inputTiffFile.SetDirectory(page);
                                // Page，图像高度方向，Tile总数
                                int tile_height = inputTiffFile.GetField(TiffTag.TILELENGTH)[0].ToInt();
                                int image_height = inputTiffFile.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                                tileTiffReader.pageLevel[page].Page_Height = image_height;
                                tileTiffReader.pageLevel[page].Tile_YCount = image_height / tile_height;

                                // Page，图像宽带方向，Tile总数 
                                int tile_width = inputTiffFile.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                                int image_width = inputTiffFile.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                                tileTiffReader.pageLevel[page].Page_Width = image_width;
                                tileTiffReader.pageLevel[page].Tile_XCount = image_width / tile_width;

                                ivCamera.PageImageWidth = image_width;
                                ivCamera.PageImageHeight = image_height;

                                if (page == 0)
                                {
                                    originImageWidth = image_width;
                                    originImageHeight = image_height;
                                }
                            }

                            break;
                        default:
                            break;
                    }

                    string slideFileName = System.IO.Path.GetFileName(tiffFileName);

                    vsInfo = new VirtualSlideInfo();
                    vsInfo.AuthorOfAnnotation = "Kaijie Wu";
                    vsInfo.DateOfAnnotation = DateTime.Now.ToString("yyyy-MM-dd");
                    vsInfo.ImageHeight = tileTiffReader.pageLevel[0].Page_Height;
                    vsInfo.ImageWidth = tileTiffReader.pageLevel[0].Page_Width;
                    // GetMD5Hash is not suit for super large tiff (>1GByte).

                    vsInfo.FileSize = fi.Length;
                    vsInfo.ImageFileName = slideFileName;

                    // 特殊处理 仪器放大 40倍，按10倍查看扫描图像
                    ivCamera.CurrentPage = 0;
                    ivCamera.FullScaleZoom = 1;

                    previousZoom = 1;

                    annotationCanvas.Page = 0;

                    // 加载显示图像节点
                    AllocateShaps(tileTiffReader, inputTiffFile, buffer);

                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);
                    mapZoom.Zoom = 1;
                    mapZoom.Offset = new Point(0, 0);

                    ivCamera.IsSlideImageLoaded = true;

                    ivCamera.zoomSlider.Value = 0;

                    // 首先允许 ScrollViewer 拖放操作
                    annotationCanvas.IsHitTestVisible = false;

                    // 缩略图显示
               
                  
                   // GenerateOverview();
                    btThumbnail.IsChecked = true;
                    menuThumbnail.IsChecked = true;

                    // 设置AnnotationCanvas 的Size
                    imageCanvas.CalculateExtent();
                    annotationCanvas.Width = imageCanvas.Extent.Width;
                    annotationCanvas.Height = imageCanvas.Extent.Height;

                    annotationCanvas.ExtentWidth = imageCanvas.ExtentWidth;
                    annotationCanvas.ExtentHeight = imageCanvas.ExtentHeight;

                    // 界面相关
                    menuAnnotationTools.IsEnabled = true;
                    menuCloseImage.IsEnabled = true;
                    menuSlideImage.IsEnabled = true;

                    buttonToolPointer.IsEnabled = true;
                    buttonToolRectangle.IsEnabled = true;
                    buttonToolEllipse.IsEnabled = true;
                    buttonToolLine.IsEnabled = true;
                    buttonToolArrow.IsEnabled = true;
                    buttonToolRuler.IsEnabled = true;
                    buttonToolPencil.IsEnabled = true;
                    buttonToolText.IsEnabled = true;

                    // ImageViewer ZoomBar Display
                    if (magnifier == Magnifier.X20)
                    {
                        ivCamera.btnZoom1x.Content = "1.25X";
                        ivCamera.btnZoom2x.Content = "2.5X";
                        ivCamera.btnZoom4x.Content = "3.75X";
                        ivCamera.btnZoom5x.Content = "5.0X";
                        ivCamera.btnZoom10x.Content = "10.0X";
                        ivCamera.btnZoom20x.Content = "20.0X";
                    }

                    if (magnifier == Magnifier.X40)
                    {
                        ivCamera.btnZoom1x.Content = "2.5X";
                        ivCamera.btnZoom2x.Content = "5.0X";
                        ivCamera.btnZoom4x.Content = "7.5X";
                        ivCamera.btnZoom5x.Content = "10.0X";
                        ivCamera.btnZoom10x.Content = "20.0X";
                        ivCamera.btnZoom20x.Content = "40.0X";
                    }                

                    // 待完善 Label Image 读取
                    BitmapImage bmp = new BitmapImage();
                    //      ivCamera.UpdateThumbnailImage(bmp);
                    ivCamera.UpdateLabelImage(bmp);

                    // Favorite Items
                    if (SettingsManager.ApplicationSettings.FavoriteItemsLoadOnStartup)
                        LoadFavoriteItems();


                    // Recent Files Listbox
                    // lbRecentFiles.Items.Insert(index_recentFiles, new { ImgPath = ThumbnailImage, ImgTxt = Path.GetFileName(dlg.FileName)});
                    index_recentFiles++;
                }
            }
        }


        private void ImageFileClose_Click(object sender, RoutedEventArgs e)
        {
            if (inputTiffFile != null)
                inputTiffFile.Close();

            if (annotationCanvas.GraphicsList.Count > 0)
            {
                annotationCanvas.Clear();
                annotationCanvas.OnGraphicListChanged(null);
            }

            if (imageCanvas.VirtualChildren.Count > 0)
            {
                imageCanvas.VirtualChildren.Clear();
            }

            ivCamera.IsSlideImageLoaded = false;

            // Recent Files Listbox
            if (lbRecentFiles.SelectedIndex >= 0)
                lbRecentFiles.Items.RemoveAt(lbRecentFiles.SelectedIndex);

            index_recentFiles--;

            // 界面相关
            menuAnnotationTools.IsEnabled = false;
            menuCloseImage.IsEnabled = false;
            menuSlideImage.IsEnabled = false;

            buttonToolPointer.IsEnabled = false;
            buttonToolRectangle.IsEnabled = false;
            buttonToolEllipse.IsEnabled = false;
            buttonToolLine.IsEnabled = false;
            buttonToolArrow.IsEnabled = false;
            buttonToolRuler.IsEnabled = false;
            buttonToolPencil.IsEnabled = false;
            buttonToolText.IsEnabled = false;
        }

        // 图像缩小
        private void ZoomOut(Point cp)
        {
            // 2017-08-06 改为 18level
            switch (ivCamera.CurrentPage)
            {
                case (int)TiffPage.Page0:
                    {
                        ivCamera.FullScaleZoom -= 0.125;

                        if (ivCamera.FullScaleZoom < 0.375)
                        {
                            // Page 0 -> Page 1                            
                            ivCamera.CurrentPage = 1;
                            ivCamera.FullScaleZoom = 0.25;

                            AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);

                            mapZoom.Zoom = 1.0;
                            annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                            annotationCanvas.Page = 1;

                        }
                        else
                        {
                            mapZoom.Zoom = ivCamera.FullScaleZoom;

                            annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                            annotationCanvas.Page = 0;
                        }
                        break;
                    }
                case (int)TiffPage.Page1:
                    {
                        ivCamera.FullScaleZoom -= 0.03125;

                        if (ivCamera.FullScaleZoom < 0.09375)
                        {
                            // Page 1 -> Page 2
                            ivCamera.CurrentPage = 2;
                            ivCamera.FullScaleZoom = 0.0625;

                            AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);

                            mapZoom.Zoom = 1.0;

                            annotationCanvas.ActualScale = 1 / mapZoom.Zoom;  // XXX 线宽存在问题 2017-08-12
                            annotationCanvas.Page = 2;
                        }
                        else
                        {
                            mapZoom.Zoom = 4 * ivCamera.FullScaleZoom;

                            annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                            annotationCanvas.Page = 1;
                        }

                        break;
                    }
                case (int)TiffPage.Page2:
                    {
                        ivCamera.FullScaleZoom -= 0.0078125;

                        if (ivCamera.FullScaleZoom < 0.023438)
                        {
                            ivCamera.CurrentPage = 3;  // 暂时不支持 缩略图显示    
                            break;
                        }
                        else
                        {
                            mapZoom.Zoom = 16 * ivCamera.FullScaleZoom;

                            annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                            annotationCanvas.Page = 2;
                        }
                        break;
                    }

                default:
                    break;
            }

            ivCamera.lbZoom.Content = ivCamera.FullScaleZoom;
            ChangeZoomBarDisplay();

            AdjustViewToPosition(cp);
        }

        // 图像放大
        private void ZoomIn(Point cp)
        {
           
            // 2017-08-06 改为 18level
            switch (ivCamera.CurrentPage)
            {
                case (int)TiffPage.Page0:
                    {
                        ivCamera.FullScaleZoom += 0.125;
                        mapZoom.Zoom = ivCamera.FullScaleZoom;

                        annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                        annotationCanvas.Page = 0;

                        break;
                    }
                case (int)TiffPage.Page1:
                    {
                        ivCamera.FullScaleZoom += 0.03125;


                        if (ivCamera.FullScaleZoom > 0.25)
                        {
                            // Page 1 -> Page 0
                            ivCamera.CurrentPage = 0;
                            ivCamera.FullScaleZoom = 0.375;

                            AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);

                            mapZoom.Zoom = 0.375;
                            annotationCanvas.ActualScale = 1 / mapZoom.Zoom;  // XXX 线宽存在问题 2017-08-12
                            annotationCanvas.Page = 0;

                        }
                        else
                        {
                            mapZoom.Zoom = 4 * ivCamera.FullScaleZoom;

                            annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                            annotationCanvas.Page = 1;
                        }

                        break;
                    }
                case (int)TiffPage.Page2:
                    {
                        ivCamera.FullScaleZoom += 0.0078125;

                        if (ivCamera.FullScaleZoom > 0.0625)
                        {
                            // Page 2 -> Page 1
                            ivCamera.CurrentPage = 1;  // 切片缩略图
                            ivCamera.FullScaleZoom = 0.09375;

                            AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);

                            mapZoom.Zoom = 0.375;

                            annotationCanvas.ActualScale = 1 / mapZoom.Zoom;  // XXX 线宽存在问题 2017-08-12
                            annotationCanvas.Page = 1;
                        }
                        else
                        {
                            mapZoom.Zoom = 16 * ivCamera.FullScaleZoom;

                            annotationCanvas.ActualScale = 1 / mapZoom.Zoom;
                            annotationCanvas.Page = 2;
                        }
                        break;
                    }
                case (int)TiffPage.Thumbnail:
                    {
                        ivCamera.CurrentPage = 2;  // 切片缩略图     
                        AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);
                        ivCamera.FullScaleZoom = 0.0234375;
                        mapZoom.Zoom = 0.375;
                        break;
                    }
                default:
                    break;
            }

            ivCamera.lbZoom.Content = ivCamera.FullScaleZoom;
            ChangeZoomBarDisplay();
            previousZoom = ivCamera.FullScaleZoom;

            AdjustViewToPosition(cp);
        }

        // cp is the center of viewer.
        private void AdjustViewToPosition(Point cp)
        {
            if (ivCamera.imageCanvas.ExtentWidth < ivCamera.ScrollImageViewer.ViewportHeight)
                //   ivCamera.ScrollToImageCenter(new Point(cp.X, cp.Y), new Point(ivCamera.ScrollImageViewer.ViewportWidth / 2, ivCamera.ScrollImageViewer.ViewportHeight / 2));
                ivCamera.ScrollToImageCenter(new Point(cp.X, cp.Y), new Point(0, 0));
            else
                ivCamera.ScrollToImageCenter(new Point(cp.X, cp.Y), new Point(ivCamera.ScrollImageViewer.ViewportWidth / 2, ivCamera.ScrollImageViewer.ViewportHeight / 2));

       //     ivCamera.RectThumbMoveWithMouse((imageCanvas.HorizontalOffset) * ivCamera.ThumbnailScale / ivCamera.PageRate,
      //          (imageCanvas.VerticalOffset) * ivCamera.ThumbnailScale / ivCamera.PageRate);
        }

        // 图像放大 
        private void ImageZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera.IsSlideImageLoaded == true)
            {
                double hcp = (ivCamera.imageCanvas.HorizontalOffset + ivCamera.ScrollImageViewer.ViewportWidth / 2) / ivCamera.FullScaleZoom;
                double vcp = (ivCamera.imageCanvas.VerticalOffset + ivCamera.ScrollImageViewer.ViewportHeight / 2) / ivCamera.FullScaleZoom;

                ZoomIn(new Point(hcp, vcp));    
            }
            else
                MessageBox.Show("Please make sure that one slide image is loaded!", "Zoom In - Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // 图像缩小
        private void ImageZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera.IsSlideImageLoaded == true)
            {
                double hcp = (ivCamera.imageCanvas.HorizontalOffset + ivCamera.ScrollImageViewer.ViewportWidth / 2) / ivCamera.FullScaleZoom;
                double vcp = (ivCamera.imageCanvas.VerticalOffset + ivCamera.ScrollImageViewer.ViewportHeight / 2) / ivCamera.FullScaleZoom;

                ZoomOut(new Point(hcp, vcp));
            }
            else
                MessageBox.Show("Please make sure that one slide image is loaded!", "Zoom Out - Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ivCamera_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            pan.Dragging = false;

            ivCameraPreviewMouseLeftButtonDown = false;
        }

        private void ivCamera_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            ivCameraPreviewMouseLeftButtonDown = true;
            mouseDownPoint = e.GetPosition(imageCanvas);
        }

        private void ivCamera_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && ivCamera.IsMouseOver)
            {
                ivCamera.Focus();    // 鼠标滚轮事件(缩放时),需要有焦点
            }
        }

        private void ImageSelection_Click(object sender, RoutedEventArgs e)
        {
           

        }
       public void ZoomWithMouseWheel()
        {
            if (mapZoom.IsZoomIn)
            {
                Point cp = ivCamera.CurrentPositionOnTarget;
                ZoomIn(cp);
                ivCamera.zoomSlider.Value++;
            }
            else
            {
                Point cp = ivCamera.CurrentPositionOnTarget;
                ZoomOut(cp);
                ivCamera.zoomSlider.Value--;
            }
        }

        private void ZoomOut_Version2()
        {
            switch (ivCamera.CurrentPage)
            {
                case 0:
                    {
                        annotationCanvas.Page = 0;


                        if (mapZoom.Zoom <= 0.251)
                        {
                            // 为了 Page 0 与 Page 1 衔接
                            ivCamera.FullScaleZoom = mapZoom.Zoom;
                            ivCamera.CurrentPage = 1;
                            AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);
                            mapZoom.Zoom = 1.0;
                        }
                        else
                        {
                            // mapZoom.Zoom = ivCamera.FullScaleZoom;
                        }

                        // 【测试-WKJ】
                        //     VisualCollection GL2;
                        Visual[] GL2 = new Visual[annotationCanvas.GraphicsList.Count];
                        annotationCanvas.GraphicsList.CopyTo(GL2, 0);

                        break;
                    }
                case 1:
                    {
                        annotationCanvas.Page = 1;

                        ivCamera.FullScaleZoom = mapZoom.Zoom*0.25;
                        if (mapZoom.Zoom < 0.2501)
                        {
                            // 为了 Page 1 与 Page 2 衔接

                            ivCamera.CurrentPage = 2;
                            AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);
                            mapZoom.Zoom = 1.0;
                        }
                        else
                        {
                            //mapZoom.Zoom = 4 * ivCamera.FullScaleZoom;
                        }

                        break;
                    }
                case 2:
                    {
                        annotationCanvas.Page = 2;
                        ivCamera.FullScaleZoom = mapZoom.Zoom * 0.25*0.25;
                        if (ivCamera.FullScaleZoom < 0.015629)
                        {
                            ivCamera.CurrentPage = 3;  // 切片缩略图     
                                                       // 暂时不支持 缩略图显示                                

                            break;
                        }
                        else
                        {
                            // mapZoom.Zoom = 16 * ivCamera.FullScaleZoom;
                        }
                        break;
                    }

                default:
                    break;
            }
        }
        private void ZoomIn_Version2()
        {
            switch (ivCamera.CurrentPage)
            {
                case 0:
                    {
                        annotationCanvas.Page = 0;
                        ivCamera.FullScaleZoom = mapZoom.Zoom ;
                        break;
                    }
                case 1:
                    {
                        annotationCanvas.Page = 1;

                    ivCamera.FullScaleZoom = mapZoom.Zoom*0.25;
                        if (mapZoom.Zoom >= 1.00)
                        {
                            // 为了 Page 1 与 Page 0 衔接
                            ivCamera.CurrentPage = 0;
                          
                            AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);
                            mapZoom.Zoom = 0.25;
                        }
                        else
                        {
                            //mapZoom.Zoom = 4 * ivCamera.FullScaleZoom;
                        }

                        break;
                    }
                case 2:
                    {
                        annotationCanvas.Page = 2;

                        ivCamera.FullScaleZoom = mapZoom.Zoom * 0.25*0.25;
                        if (mapZoom.Zoom >= 1.000)
                        {
                            // 为了 Page 2 与 Page 1 衔接
                            ivCamera.CurrentPage = 1;  // 切片缩略图

                            AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);
                            mapZoom.Zoom = 0.25;
                        }
                        else
                        {
                            // mapZoom.Zoom = 16 * ivCamera.FullScaleZoom;
                        }
                        break;
                    }
                case 3:
                    {
                        ivCamera.CurrentPage = 2;  // 切片缩略图     
                        AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);
                        ivCamera.FullScaleZoom = 0.01953125;
                        mapZoom.Zoom = 0.3125;
                        break;
                    }
                default:
                    break;
            }

            previousZoom = ivCamera.FullScaleZoom;
        }
        private void ivCamera_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {


            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                if (mapZoom.IsZoomIn)
                {
                    ZoomIn_Version2();
                }
                else
                {
                    ZoomOut_Version2();
                }
            }

            //if (SettingsManager.ApplicationSettings.CTRLforScale)
            //{
            //    if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            //    {
            //        ZoomWithMouseWheel();
            //    }
            //}
            //else
            //{
            //    ZoomWithMouseWheel();
            //}

            //ChangeZoomBarDisplay();


        }

        #endregion

        #region DrawTools
        #region Application Commands
        /// <summary>
        /// Annotation New Command
        /// </summary>
        private void menuNewAnnotation_Click(object sender, RoutedEventArgs e)
        {
            if (!PromptToSave())
            {
                return;
            }

            annotationCanvas.Clear();

            anotFileName = "";
            UpdateTitle(tiffFileName, anotFileName);
        }

        /// <summary>
        /// Annotation Print command
        /// </summary>

        private void menuPrintAnnotation_Click(object sender, RoutedEventArgs e)
        {
            if (annotationCanvas.GraphicsList.Count == 0)
            {
                MessageBox.Show("There is no any annotion! Save is canceled!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Depending on XAML version, call required Print function.
            Object o = FindName("imageBackground");

            if (o != null)
            {
                Image image = o as Image;

                if (image != null)
                {
                    PrintWithBackgroundImage(image);
                    return;
                }
            }

            PrintWithoutBackground();
        }

        /// <summary>
        /// Annotation Close Command
        /// </summary>
        private void menuCloseAnnotation_Click(object sender, RoutedEventArgs e)
        {

            MruInfo mInfo = SettingsManager.ApplicationSettings.RecentFilesList;

            if (anotFileName != null)
            {
                mruManager.Delete(anotFileName);
                annotationCanvas.DeleteAll();
            }
        }

        /// <summary>
        /// Annotation Open Command
        /// </summary>
        private void menuOpenAnnotation_Click(object sender, RoutedEventArgs e)
        {

            if (vsInfo == null)
            {
                MessageBox.Show("Please make sure that one virtual slide image is loaded!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!PromptToSave())
            {
                return;
            }

            // Show Open File dialog
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "XML files (*.xml)|*.xml|All Files|*.*";
            dlg.DefaultExt = "xml";
            dlg.InitialDirectory = SettingsManager.ApplicationSettings.InitialDirectory;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog().GetValueOrDefault() != true)
            {
                return;
            }

            try
            {
                VirtualSlideInfo info = annotationCanvas.CheckMatchSlide(dlg.FileName);
                if ((vsInfo.FileSize == info.FileSize) && (vsInfo.ImageWidth == info.ImageWidth)
                    && (vsInfo.ImageHeight == info.ImageHeight))
                {
                    // Load file
                    annotationCanvas.Load(dlg.FileName);
                }
                else
                {
                    ShowError("The annotation is not match current virtual slide image!");
                }
            }
            catch (DrawingCanvasException ee)
            {
                ShowError(ee.Message);
                mruManager.Delete(dlg.FileName);
                return;
            }

            anotFileName = dlg.FileName;
            UpdateTitle(tiffFileName, anotFileName);
            mruManager.Add(anotFileName);

            // Remember initial directory
            SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(dlg.FileName);
        }

        /// <summary>
        /// Annotation Save Command
        /// </summary>
        private void menuSaveAnnotation_Click(object sender, RoutedEventArgs e)
        {
            if (annotationCanvas.GraphicsList.Count == 0)
            {
                MessageBox.Show("There is no any annotion! Save is canceled!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(anotFileName))
            {
                menuSaveAsAnnotation_Click(sender, e);
                return;
            }

            if (vsInfo != null)
            {
                Save(anotFileName, vsInfo);   // 不做坐标映射           
            }
        }

        /// <summary>
        /// Save As Command
        /// </summary>
        private void menuSaveAsAnnotation_Click(object sender, RoutedEventArgs e)
        {
            if (annotationCanvas.GraphicsList.Count == 0)
            {
                MessageBox.Show("There is no any annotion! Save is canceled!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show Save File dialog
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Filter = "XML files (*.xml)|*.xml|All Files|*.*";
            dlg.OverwritePrompt = true;
            dlg.DefaultExt = "xml";
            dlg.InitialDirectory = SettingsManager.ApplicationSettings.InitialDirectory;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog().GetValueOrDefault() != true)
            {
                return;
            }

            if (vsInfo != null)
            {
                // Save
                if (!Save(dlg.FileName, vsInfo))
                {
                    return;
                }
            }
            // Remember initial directory
            SettingsManager.ApplicationSettings.InitialDirectory = System.IO.Path.GetDirectoryName(dlg.FileName);
        }

        /// <summary>
        /// Undo command
        /// </summary>
        private void btAnnotationUndo_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.Undo();
        }

        /// <summary>
        /// Redo command
        /// </summary>
        private void btAnnotationRedo_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.Redo();
        }

        /// <summary>
        /// Delete command
        /// </summary>
        private void btAnnotationDelete_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.Delete();
        }

        /// <summary>
        /// DeleteAll command
        /// </summary>
        private void btAnnotationDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.DeleteAll();
        }
        /// <summary>
        /// SelectAll command
        /// </summary>
        private void btAnnotationSelectAll_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.SelectAll();
        }

        private void FillAnnotationDataView(VisualCollection graphicList)
        {
            if (!ivCamera.IsSlideImageLoaded)
                return;

            if (graphicList.Count >= 0)
            {
                // ListView 绑定 DataTable
                DataTable annotDataTable = new DataTable("AnnotDataTable");

                DataColumn ID = new DataColumn("ID");//第一列
                ID.DataType = System.Type.GetType("System.Int32");
                //ID.AutoIncrement = true; //自动递增ID号 
                annotDataTable.Columns.Add(ID);

                //设置主键
                DataColumn[] keys = new DataColumn[1];
                keys[0] = ID;
                annotDataTable.PrimaryKey = keys;

                annotDataTable.Columns.Add(new DataColumn("Type", typeof(string)));//第二列
                annotDataTable.Columns.Add(new DataColumn("Position", typeof(string)));//第三列
                annotDataTable.Columns.Add(new DataColumn("ReportDiscription", typeof(string)));//第四列
                annotDataTable.Columns.Add(new DataColumn("Reporter", typeof(string)));//第五列
                annotDataTable.Columns.Add(new DataColumn("Date", typeof(string)));//第六列

                GraphicsLine gl;
                GraphicsEllipse ge;
                GraphicsPolyLine gpl;
                GraphicsRectangle gr;
                GraphicsText gt;
                GraphicsArrow ga;
                GraphicsRuler grl;

                int i = 0;
                foreach (GraphicsBase g in graphicList)
                {
                    String typeName = g.GetType().Name;
                    string _typeName = "";
                    string _position = "";
                    string _report = "";
                    string _reporter = "";
                    string _reportDate = "";
                    if (typeName == "GraphicsLine")
                    {
                        gl = (GraphicsLine)g;

                        _typeName = "Line";
                        _position = FormatPoint(gl.Start) + " (" + gl.Page.ToString() + ")";
                        _report = gl.ReportContent;
                        _reporter = gl.Reporter;
                        _reportDate = gl.ReportDate;
                    }

                    if (typeName == "GraphicsArrow")
                    {
                        ga = (GraphicsArrow)g;

                        _typeName = "Arrow";
                        _position = FormatPoint(ga.Start) + " (" + ga.Page.ToString() + ")";
                        _report = ga.ReportContent;
                        _reporter = ga.Reporter;
                        _reportDate = ga.ReportDate;
                    }

                    if (typeName == "GraphicsRuler")
                    {
                        grl = (GraphicsRuler)g;

                        _typeName = "Ruler";
                        _position = FormatPoint(grl.Start) + " (" + grl.Page.ToString() + ")";
                        _report = grl.ReportContent;
                        _reporter = grl.Reporter;
                        _reportDate = grl.ReportDate;
                    }

                    if (typeName == "GraphicsPolyLine")
                    {
                        gpl = (GraphicsPolyLine)g;

                        _typeName = "PolyLine";
                        _position = FormatPoint(gpl.Start) + " (" + gpl.Page.ToString() + ")";
                        _report = gpl.ReportContent;
                        _reporter = gpl.Reporter;
                        _reportDate = gpl.ReportDate;
                    }

                    if (typeName == "GraphicsRectangle")
                    {
                        gr = (GraphicsRectangle)g;

                        _typeName = "Rectangle";
                        _position = FormatPoint(gr.Start) + " (" + gr.Page.ToString() + ")";
                        _report = gr.ReportContent;
                        _reporter = gr.Reporter;
                        _reportDate = gr.ReportDate;
                    }

                    if (typeName == "GraphicsEllipse")
                    {
                        ge = (GraphicsEllipse)g;

                        _typeName = "Ellipse";
                        _position = FormatPoint(ge.Start) + " (" + ge.Page.ToString() + ")";
                        _report = ge.ReportContent;
                        _reporter = ge.Reporter;
                        _reportDate = ge.ReportDate;
                    }

                    if (typeName == "GraphicsText")
                    {
                        gt = (GraphicsText)g;

                        _typeName = "Text";
                        _position = FormatPoint(gt.Start) + " (" + gt.Page.ToString() + ")";
                        _report = gt.ReportContent;
                        _reporter = gt.Reporter;
                        _reportDate = gt.ReportDate;
                    }

                    annotDataTable.Rows.Add(i, _typeName, _position, _report, _reporter, _reportDate);

                    i++;
                }

                // Setup the GridView Columns
                graphicListView.ItemsSource = annotDataTable.DefaultView;
            }
        }

        #endregion 

        #region Tools Event Handlers
        /// <summary>
        /// One of Tools menu items is clicked.
        /// ToolType enumeration member name is in the item tag.
        /// </summary>
        void ToolMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 确保图像已加载，才能进行标注
            if (imageCanvas.VirtualChildren.Count > 0)
            {
                annotationCanvas.IsHitTestVisible = true;
                annotationCanvas.Tool = (ToolType)Enum.Parse(typeof(ToolType), ((MenuItem)sender).Tag.ToString());
            }
        }

        /// <summary>
        /// One of Tools toolbar buttons is clicked.
        /// ToolType enumeration member name is in the button tag.
        /// 
        /// For toolbar buttons I use PreviewMouseDown event instead of Click,
        /// because IsChecked property is not handled by standard
        /// way. For example, every click on the Pointer button keeps it checked
        /// instead of changing state. IsChecked property of every button is bound 
        /// to annotationCanvas.Tool property.
        /// Using normal Click handler toggles every button, which doesn't
        /// match my requirements. So, I catch click in PreviewMouseDown handler
        /// and set Handled to true preventing standard IsChecked handling.
        /// 
        /// Other way to do the same: handle Click event and set buttons state
        /// at application idle time, without binding IsChecked property.
        /// </summary>
        void ToolButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 确保图像已加载，才能进行标注
            if (imageCanvas.VirtualChildren.Count > 0)
            {
                annotationCanvas.IsHitTestVisible = true;
                annotationCanvas.Tool = (ToolType)Enum.Parse(typeof(ToolType),
                    ((System.Windows.Controls.Primitives.ButtonBase)sender).Tag.ToString());

                e.Handled = true;
            }
        }

        #endregion Tools Event Handlers

        #region Edit Event Handlers

        void menuEditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.SelectAll();
        }

        void menuEditUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.UnselectAll();
        }

        void menuEditDelete_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.Delete();
        }

        void menuEditDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.DeleteAll();
        }

        void menuEditMoveToFront_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.MoveToFront();
        }

        void menuEditMoveToBack_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.MoveToBack();
        }

        void menuEditModifyGraphicsText_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.ModifyGraphicsText();
        }


        void menuEditSetProperties_Click(object sender, RoutedEventArgs e)
        {
            annotationCanvas.SetProperties();
        }
        #endregion Edit Event Handlers

        #region Properties Event Handlers

        /// <summary>
        /// Show Font dialog
        /// </summary>
        void PropertiesFont_Click(object sender, RoutedEventArgs e)
        {
            Petzold.ChooseFont.FontDialog dlg = new Petzold.ChooseFont.FontDialog();
            dlg.Owner = this;
            dlg.Background = SystemColors.ControlBrush;
            dlg.Title = "Select Font";

            dlg.FaceSize = annotationCanvas.TextFontSize;

            dlg.Typeface = new Typeface(
                new FontFamily(annotationCanvas.TextFontFamilyName),
                annotationCanvas.TextFontStyle,
                annotationCanvas.TextFontWeight,
                annotationCanvas.TextFontStretch);

            if (dlg.ShowDialog().GetValueOrDefault() != true)
            {
                return;
            }

            // Set new font in drawing canvas
            annotationCanvas.TextFontSize = dlg.FaceSize;
            annotationCanvas.TextFontFamilyName = dlg.Typeface.FontFamily.ToString();
            annotationCanvas.TextFontStyle = dlg.Typeface.Style;
            annotationCanvas.TextFontWeight = dlg.Typeface.Weight;
            annotationCanvas.TextFontStretch = dlg.Typeface.Stretch;

            txDrawToolsTextFontFamilyName.Text = dlg.Typeface.FontFamily.ToString();
            double d = (double)dlg.FaceSize * 0.75;
            txDrawToolsTextFontSize.Text = ((int)(d + 0.5)).ToString(CultureInfo.InvariantCulture);

            // Set new font in application settings
            SettingsManager.ApplicationSettings.TextFontSize = dlg.FaceSize;
            SettingsManager.ApplicationSettings.TextFontFamilyName = dlg.Typeface.FontFamily.ToString();
            SettingsManager.ApplicationSettings.TextFontStyle = FontConversions.FontStyleToString(dlg.Typeface.Style);
            SettingsManager.ApplicationSettings.TextFontWeight = FontConversions.FontWeightToString(dlg.Typeface.Weight);
            SettingsManager.ApplicationSettings.TextFontStretch = FontConversions.FontStretchToString(dlg.Typeface.Stretch);
        }

        /// <summary>
        /// Show Color dialog
        /// </summary>
        void PropertiesColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Owner = this;
            dlg.Color = annotationCanvas.ObjectColor;

            if (dlg.ShowDialog().GetValueOrDefault() != true)
            {
                return;
            }

            // Set selected color in drawing canvas and in application settings
            annotationCanvas.ObjectColor = dlg.Color;
            txDrawToolsTextColor.Background = new SolidColorBrush(dlg.Color);

            SettingsManager.ApplicationSettings.ObjectColor = annotationCanvas.ObjectColor;
        }

        /// <summary>
        /// Line width is selected
        /// </summary>
        void PropertiesLineWidth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double lineWidth = Double.Parse(comboPropertiesLineWidth.SelectedValue.ToString(),
                CultureInfo.InvariantCulture);

            annotationCanvas.LineWidth = lineWidth;
            SettingsManager.ApplicationSettings.LineWidth = lineWidth;
        }

        #endregion Properties Event Handlers

        #region Other Event Handlers

        /// <summary>
        /// File is selected from MRU list
        /// </summary>
        void mruManager_FileSelected(object sender, MruFileOpenEventArgs e)
        {
            if (!PromptToSave())
            {
                return;
            }

            try
            {
                // Load file
                annotationCanvas.Load(e.FileName);

            }
            catch (DrawingCanvasException ex)
            {
                ShowError(ex.Message);
                mruManager.Delete(e.FileName);

                return;
            }

            anotFileName = e.FileName;
            UpdateTitle(tiffFileName, anotFileName);
            mruManager.Add(anotFileName);
        }


        /// <summary>
        /// IsDirty is changed
        /// </summary>
        void drawingCanvas_IsDirtyChanged(object sender, RoutedEventArgs e)
        {
            UpdateTitle(tiffFileName, anotFileName);
        }

        /// <summary>
        /// Check Tools menu items according to active tool
        /// </summary>
        void menuTools_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            menuToolsPointer.IsChecked = (annotationCanvas.Tool == ToolType.Pointer);
            menuToolsRectangle.IsChecked = (annotationCanvas.Tool == ToolType.Rectangle);
            menuToolsEllipse.IsChecked = (annotationCanvas.Tool == ToolType.Ellipse);
            menuToolsLine.IsChecked = (annotationCanvas.Tool == ToolType.Line);
            menuToolsPencil.IsChecked = (annotationCanvas.Tool == ToolType.PolyLine);
            menuToolsText.IsChecked = (annotationCanvas.Tool == ToolType.Text);
        }

        /// <summary>
        /// Enable Edit menu items according to annotationCanvas stare
        /// </summary>
        void menuEdit_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            menuEditDelete.IsEnabled = annotationCanvas.CanDelete;
            menuEditDeleteAll.IsEnabled = annotationCanvas.CanDeleteAll;
            menuEditMoveToBack.IsEnabled = annotationCanvas.CanMoveToBack;
            menuEditMoveToFront.IsEnabled = annotationCanvas.CanMoveToFront;
            menuEditModifyGraphicsText.IsEnabled = annotationCanvas.CanModifyGraphicsText;
            menuEditSelectAll.IsEnabled = annotationCanvas.CanSelectAll;
            menuEditUnselectAll.IsEnabled = annotationCanvas.CanUnselectAll;
            menuEditSetProperties.IsEnabled = annotationCanvas.CanSetProperties;
            menuEditUndo.IsEnabled = annotationCanvas.CanUndo;
            menuEditRedo.IsEnabled = annotationCanvas.CanRedo;
        }

        /// <summary>
        /// Show About Dialog
        /// </summary>
        private void menuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow dlg = new AboutWindow();
            dlg.Owner = this;

            try
            {
                dlg.ShowDialog();
            }
            catch (System.Net.WebException ee)
            {
                // Click on Hyperlink without Internet connection
                if (dlg != null)
                {
                    dlg.Close();
                }

                ShowError(ee.Message);
            }
        }

        /// <summary>
        /// Form is closing - ask to save
        /// </summary>
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!PromptToSave())
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Function executes different actions depending on
        /// XAML version.
        /// Use one of them in actual program.
        /// </summary>
        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Instead of using control names directly,
            // find them dynamically. This allows to compile
            // the program with different XAML versions.
            Object o = FindName("imageBackground");

            if (o == null)
            {
                // Refresh clip area in the canvas.
                // This is required when canvas is used in standalone mode without
                // background image.
                annotationCanvas.RefreshClip();

                return;
            }

            Image image = o as Image;

            if (image == null)
            {
                return;
            }

            o = FindName("viewBoxContainer");

            if (o == null)
            {
                return;
            }

            Viewbox v = o as Viewbox;

            if (v == null)
            {
                return;     // precaution
            }

            // Compute actual scale of image drawn on the screen.
            // Image is resized by ViewBox.
            //
            // Note: when image is placed inside ScrollView with slider,
            // the same correction is done in XAML using ActualScale binding.

            double viewBoxWidth = v.ActualWidth;
            double viewBoxHeight = v.ActualHeight;

            double imageWidth = image.Source.Width;
            double imageHeight = image.Source.Height;

            double scale;

            if (viewBoxWidth / imageWidth > viewBoxHeight / imageHeight)
            {
                scale = viewBoxWidth / imageWidth;
            }
            else
            {
                scale = viewBoxHeight / imageHeight;
            }

            // Apply actual scale to the canvas to keep overlay line width constant.
            annotationCanvas.ActualScale = scale;
        }

        private void OnGraphicListChanged(object sender, System.EventArgs e)
        {
            if (annotationCanvas.GraphicsList.Count >= 0)
            {
                FillAnnotationDataView(annotationCanvas.GraphicsList);
            }
        }

        private void graphicListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectIndex = graphicListView.SelectedIndex;
            if (selectIndex >= 0 && annotationCanvas.GraphicsList != null)
            {
                double rate = 0;

                GraphicsArrow ga;
                GraphicsRuler grl;

                GraphicsLine gl;
                GraphicsEllipse ge;
                GraphicsPolyLine gpl;
                GraphicsRectangle gr;
                GraphicsText gt;

                GraphicsBase o = null;

                o = (GraphicsBase)annotationCanvas.GraphicsList[selectIndex];

                if (o != null)
                {
                    if (!o.IsSelected)
                    {
                        annotationCanvas.UnselectAll();
                    }

                    // Select clicked object
                    o.IsSelected = true;
                }
                else
                {
                    annotationCanvas.UnselectAll();
                }

                PropertiesGraphicsBase g = o.CreateSerializedObject();
                Point p = new Point();
                String typeName = g.GetType().Name;

                if (typeName == "PropertiesGraphicsArrow")
                {
                    ga = (GraphicsArrow)g.CreateGraphics();
                    rate = ivCamera.annotationCanvas.GetDrawRate(ga, ivCamera.CurrentPage);
                    p = new Point(ga.Center.X * rate, ga.Center.Y * rate);
                }

                if (typeName == "PropertiesGraphicsRuler")
                {
                    grl = (GraphicsRuler)g.CreateGraphics();
                    rate = ivCamera.annotationCanvas.GetDrawRate(grl, ivCamera.CurrentPage);
                    p = new Point(grl.Center.X * rate, grl.Center.Y * rate);
                }

                if (typeName == "PropertiesGraphicsLine")
                {
                    gl = (GraphicsLine)g.CreateGraphics();
                    rate = ivCamera.annotationCanvas.GetDrawRate(gl, ivCamera.CurrentPage);
                    p = new Point(gl.Center.X * rate, gl.Center.Y * rate);
                }

                if (typeName == "PropertiesGraphicsPolyLine")
                {
                    gpl = (GraphicsPolyLine)g.CreateGraphics();
                    rate = ivCamera.annotationCanvas.GetDrawRate(gpl, ivCamera.CurrentPage);
                    p = new Point(gpl.Center.X * rate, gpl.Center.Y * rate);
                }

                if (typeName == "PropertiesGraphicsRectangle")
                {
                    gr = (GraphicsRectangle)g.CreateGraphics();
                    rate = ivCamera.annotationCanvas.GetDrawRate(gr, ivCamera.CurrentPage);
                    p = new Point(gr.Center.X * rate, gr.Center.Y * rate);
                }

                if (typeName == "PropertiesGraphicsEllipse")
                {
                    ge = (GraphicsEllipse)g.CreateGraphics();
                    rate = ivCamera.annotationCanvas.GetDrawRate(ge, ivCamera.CurrentPage);
                    p = new Point(ge.Center.X * rate, ge.Center.Y * rate);
                }

                if (typeName == "PropertiesGraphicsText")
                {
                    gt = (GraphicsText)g.CreateGraphics();
                    rate = ivCamera.annotationCanvas.GetDrawRate(gt, ivCamera.CurrentPage);
                    p = new Point(gt.Center.X * rate, gt.Center.Y * rate);
                }


                ivCamera.imageCanvas.SetHorizontalOffset(p.X * mapZoom.Zoom - ivCamera.ScrollImageViewer.ViewportWidth / 2);
                ivCamera.imageCanvas.SetVerticalOffset(p.Y * mapZoom.Zoom - ivCamera.ScrollImageViewer.ViewportHeight / 2);

            }
        }


        #endregion Other Event Handlers

        #region Other Functions

        /// <summary>
        /// Prompt to save and make Save operation if necessary.
        /// </summary>
        /// <returns>
        /// true - caller can continue (open new file, close program etc.
        /// false - caller should cancel current operation.
        /// </returns>
        bool PromptToSave()
        {
#if NO_PROMPT_TO_SAVE
            return true;
#else

            if (!annotationCanvas.IsDirty)
            {
                return true;    // continue
            }

            MessageBoxResult result = MessageBox.Show(
                this,
                "Do you want to save changes of annotations?",
                Properties.Resources.ApplicationTitle,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);


            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Save
                    menuSaveAnnotation_Click(null, null);    // Any better way to do this?

                    // If Saved succeeded (IsDirty  false), return true
                    return (!annotationCanvas.IsDirty);

                case MessageBoxResult.No:

                    return true;        // continue without save

                case MessageBoxResult.Cancel:

                    return false;

                default:

                    return true;
            }
#endif
        }

        /// <summary>
        /// Subscribe to different events
        /// </summary>
        void SubscribeToEvents()
        {
            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);

            annotationCanvas.IsDirtyChanged += new RoutedEventHandler(drawingCanvas_IsDirtyChanged);

            // Menu opened - used to set menu items state
            menuAnnotationTools.SubmenuOpened += new RoutedEventHandler(menuTools_SubmenuOpened);
            menuEdit.SubmenuOpened += new RoutedEventHandler(menuEdit_SubmenuOpened);

            // Tools menu
            menuToolsPointer.Click += ToolMenuItem_Click;
            menuToolsRectangle.Click += ToolMenuItem_Click;
            menuToolsEllipse.Click += ToolMenuItem_Click;
            menuToolsLine.Click += ToolMenuItem_Click;
            menuToolsPencil.Click += ToolMenuItem_Click;
            menuToolsText.Click += ToolMenuItem_Click;

            // Tools buttons
            buttonToolPointer.PreviewMouseDown += new MouseButtonEventHandler(ToolButton_PreviewMouseDown);
            buttonToolRectangle.PreviewMouseDown += new MouseButtonEventHandler(ToolButton_PreviewMouseDown);
            buttonToolEllipse.PreviewMouseDown += new MouseButtonEventHandler(ToolButton_PreviewMouseDown);
            buttonToolLine.PreviewMouseDown += new MouseButtonEventHandler(ToolButton_PreviewMouseDown);
            buttonToolArrow.PreviewMouseDown += new MouseButtonEventHandler(ToolButton_PreviewMouseDown);
            buttonToolRuler.PreviewMouseDown += new MouseButtonEventHandler(ToolButton_PreviewMouseDown);
            buttonToolPencil.PreviewMouseDown += new MouseButtonEventHandler(ToolButton_PreviewMouseDown);
            buttonToolText.PreviewMouseDown += new MouseButtonEventHandler(ToolButton_PreviewMouseDown);

            // Edit menu
            menuEditSelectAll.Click += new RoutedEventHandler(menuEditSelectAll_Click);
            menuEditUnselectAll.Click += new RoutedEventHandler(menuEditUnselectAll_Click);
            menuEditDelete.Click += new RoutedEventHandler(menuEditDelete_Click);
            menuEditDeleteAll.Click += new RoutedEventHandler(menuEditDeleteAll_Click);
            menuEditMoveToFront.Click += new RoutedEventHandler(menuEditMoveToFront_Click);
            menuEditMoveToBack.Click += new RoutedEventHandler(menuEditMoveToBack_Click);
            menuEditModifyGraphicsText.Click += new RoutedEventHandler(menuEditModifyGraphicsText_Click);
            menuEditSetProperties.Click += new RoutedEventHandler(menuEditSetProperties_Click);
        }

        /// <summary>
        /// Initialize Properties controls on the toolbar
        /// </summary>
        void InitializePropertiesControls()
        {
            for (int i = 1; i <= 10; i++)
            {
                comboPropertiesLineWidth.Items.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            for (int i = 1; i <= 10; i++)
            {
                comboPropertiesLayer.Items.Add(i.ToString(CultureInfo.InvariantCulture));
            }


            // Fill line width combo and set initial selection
            int lineWidth = (int)(annotationCanvas.LineWidth + 0.5);

            if (lineWidth < 1)
            {
                lineWidth = 1;
            }

            if (lineWidth > 10)
            {
                lineWidth = 10;
            }

            comboPropertiesLineWidth.SelectedIndex = lineWidth - 1;

            buttonPropertiesFont.Click += new RoutedEventHandler(PropertiesFont_Click);
            buttonPropertiesColor.Click += new RoutedEventHandler(PropertiesColor_Click);
            comboPropertiesLineWidth.SelectionChanged += new SelectionChangedEventHandler(PropertiesLineWidth_SelectionChanged);

            comboPropertiesLayer.SelectedIndex = 0;
        }

        /// <summary>
        /// Initialize MRU list
        /// </summary>
        void InitializeMruList()
        {
            mruManager = new MruManager(SettingsManager.ApplicationSettings.RecentFilesList, menuFileRecentFiles);
            mruManager.FileSelected += new EventHandler<MruFileOpenEventArgs>(mruManager_FileSelected);
        }

        /// <summary>
        /// Set initial properties of drawing canvas
        /// </summary>
        void InitializedrawingCanvas()
        {
            annotationCanvas.LineWidth = SettingsManager.ApplicationSettings.LineWidth;
            annotationCanvas.ObjectColor = SettingsManager.ApplicationSettings.ObjectColor;

            annotationCanvas.TextFontSize = SettingsManager.ApplicationSettings.TextFontSize;
            annotationCanvas.TextFontFamilyName = SettingsManager.ApplicationSettings.TextFontFamilyName;
            annotationCanvas.TextFontStyle = FontConversions.FontStyleFromString(SettingsManager.ApplicationSettings.TextFontStyle);
            annotationCanvas.TextFontWeight = FontConversions.FontWeightFromString(SettingsManager.ApplicationSettings.TextFontWeight);
            annotationCanvas.TextFontStretch = FontConversions.FontStretchFromString(SettingsManager.ApplicationSettings.TextFontStretch);
        }

        /// <summary>
        /// Show error message
        /// </summary>
        void ShowError(string message)
        {
            MessageBox.Show(
                this,
                message,
                Properties.Resources.ApplicationTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        /// <summary>
        /// Update window title
        /// </summary>
        void UpdateTitle(string tiffFileName, string anotFileName)
        {
            string s = Properties.Resources.ApplicationTitle;

            if (string.IsNullOrEmpty(anotFileName) && string.IsNullOrEmpty(tiffFileName))
            {
                s += " - " + " [ " + Properties.Resources.Untitled + " ] ";
            }
            else
            {
                if (!string.IsNullOrEmpty(tiffFileName))
                {
                    if (string.IsNullOrEmpty(anotFileName))
                        s += " - " + " [ Virtual Slide Image: " + System.IO.Path.GetFileName(tiffFileName) + " ] ";
                    else
                    {
                        s += " - " + " [ Virtual Slide Image: " + System.IO.Path.GetFileName(tiffFileName) + " ] " + " [ Annotation: " + System.IO.Path.GetFileName(anotFileName) + " ] ";
                    }
                }
            }

            if (annotationCanvas.IsDirty)
            {
                s = "*" + s;
            }

            this.Title = s;
        }

        /// <summary>
        /// Save to file
        /// </summary>
        bool Save(string file, VirtualSlideInfo vsInfo)
        {
            try
            {
                if (!string.IsNullOrEmpty(file))
                {
                    // Save file
                    annotationCanvas.Save(file, vsInfo);
                }
            }
            catch (DrawingCanvasException e)
            {
                ShowError(e.Message);
                return false;
            }

            anotFileName = file;
            UpdateTitle(tiffFileName, anotFileName);
            mruManager.Add(anotFileName);

            return true;
        }

        /// <summary>
        /// This function prints graphics when background image doesn't exist
        /// </summary>
        void PrintWithoutBackground()
        {
            PrintDialog dlg = new PrintDialog();

            if (dlg.ShowDialog().GetValueOrDefault() != true)
            {
                return;
            }

            // Calculate rectangle for graphics
            double width = dlg.PrintableAreaWidth / 2;
            double height = width * annotationCanvas.ActualHeight / annotationCanvas.ActualWidth;

            double left = (dlg.PrintableAreaWidth - width) / 2;
            double top = (dlg.PrintableAreaHeight - height) / 2;

            Rect rect = new Rect(left, top, width, height);

            // Create DrawingVisual and get its drawing context
            DrawingVisual vs = new DrawingVisual();
            DrawingContext dc = vs.RenderOpen();

            double scale = width / annotationCanvas.ActualWidth;

            // Keep old existing actual scale and set new actual scale.
            double oldActualScale = annotationCanvas.ActualScale;
            annotationCanvas.ActualScale = scale;

            // Remove clip in the canvas - we set our own clip.
            annotationCanvas.RemoveClip();

            // Draw frame
            dc.DrawRectangle(null, new Pen(Brushes.Black, 1),
                new Rect(rect.Left - 1, rect.Top - 1, rect.Width + 2, rect.Height + 2));

            // Prepare drawing context to draw graphics
            dc.PushClip(new RectangleGeometry(rect));
            dc.PushTransform(new TranslateTransform(left, top));
            dc.PushTransform(new ScaleTransform(scale, scale));


            // Ask canvas to draw overlays
            annotationCanvas.Draw(dc);

            // Restore old actual scale.
            annotationCanvas.ActualScale = oldActualScale;

            // Restore clip
            annotationCanvas.RefreshClip();

            dc.Pop();
            dc.Pop();
            dc.Pop();

            dc.Close();

            // Print DrawVisual
            dlg.PrintVisual(vs, "Graphics");
        }

        /// <summary>
        /// This function prints graphics with background image.
        /// </summary>
        void PrintWithBackgroundImage(Image image)
        {
            PrintDialog dlg = new PrintDialog();

            if (dlg.ShowDialog().GetValueOrDefault() != true)
            {
                return;
            }

            // Calculate rectangle for image
            double width = dlg.PrintableAreaWidth / 2;
            double height = width * image.Source.Height / image.Source.Width;

            double left = (dlg.PrintableAreaWidth - width) / 2;
            double top = (dlg.PrintableAreaHeight - height) / 2;

            Rect rect = new Rect(left, top, width, height);

            // Create DrawingVisual and get its drawing context
            DrawingVisual vs = new DrawingVisual();
            DrawingContext dc = vs.RenderOpen();

            // Draw image
            dc.DrawImage(image.Source, rect);

            double scale = width / image.Source.Width;

            // Keep old existing actual scale and set new actual scale.
            double oldActualScale = annotationCanvas.ActualScale;
            annotationCanvas.ActualScale = scale;

            // Remove clip in the canvas - we set our own clip.
            annotationCanvas.RemoveClip();

            // Prepare drawing context to draw graphics
            dc.PushClip(new RectangleGeometry(rect));
            dc.PushTransform(new TranslateTransform(left, top));
            dc.PushTransform(new ScaleTransform(scale, scale));

            // Ask canvas to draw overlays
            annotationCanvas.Draw(dc);

            // Restore old actual scale.
            annotationCanvas.ActualScale = oldActualScale;

            // Restore clip
            annotationCanvas.RefreshClip();

            dc.Pop();
            dc.Pop();
            dc.Pop();

            dc.Close();

            // Print DrawVisual
            dlg.PrintVisual(vs, "Graphics");
        }

        private void ivCamera_KeyDown(object sender, KeyEventArgs e)
        {
            if (annotationCanvas.SelectionCount > 0)
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        annotationCanvas.Delete();
                        break;
                    case Key.Tab:
                        {
                            ShowAnnotationDescriptionDialog();

                            break;
                        }
                    default:
                        break;
                }
            }

            switch (e.Key)
            {
                case Key.Escape:
                    {
                        if (annotationCanvas.IsHitTestVisible)
                            annotationCanvas.IsHitTestVisible = false;
                        break;
                    }

                default:
                    break;
            }
        }

        private void ivCamera_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (annotationCanvas.SelectionCount > 0)
            {
                ShowAnnotationDescriptionDialog();
            }
        }

        private void ShowAnnotationDescriptionDialog()
        {
            GraphicsBase gbClone;
            GraphicsLine gl;
            GraphicsArrow ga;
            GraphicsRuler grl;
            GraphicsEllipse ge;
            GraphicsPolyLine gpl;
            GraphicsRectangle gr;
            GraphicsText gt;

                int count = annotationCanvas.SelectionCount;
            int index = annotationCanvas.SelectionStartIndex;

            gbClone = ivCamera.annotationCanvas.Selection.First();

            string mm = gbClone.GetType().Name;
            string annotDescription = "";

            switch (gbClone.GetType().Name)
            {
                case "GraphicsLine":
                    {
                        gl = (GraphicsLine)gbClone;
                        annotDescription = gl.ToString();

                        break;
                    }
                case "GraphicsArrow":
                    {
                        ga = (GraphicsArrow)gbClone;
                        annotDescription = ga.ToString();

                        break;
                    }
                case "GraphicsRuler":
                    {
                        grl = (GraphicsRuler)gbClone;
                        annotDescription = grl.ToString();

                        break;
                    }
                case "GraphicsPolyLine":
                    {
                        gpl = (GraphicsPolyLine)gbClone;
                        annotDescription = gpl.ToString();
                        break;
                    }
                case "GraphicsRectangle":
                    {
                        gr = (GraphicsRectangle)gbClone;
                        annotDescription = gr.ToString();
                        break;
                    }
                case "GraphicsEllipse":
                    {
                        ge = (GraphicsEllipse)gbClone;
                        annotDescription = ge.ToString();
                        break;
                    }
                case "GraphicsText":
                    {
                        gt = (GraphicsText)gbClone;
                        annotDescription = gt.ToString();
                        break;
                    }
                default:
                    break;
            }

            // 选择多个标注时，仅弹出第一个标注的注释对话框 
            AnnotationDiscription discDialog = new AnnotationDiscription();
            discDialog.tbAnnotationInfo.Text = annotDescription;
            discDialog.Topmost = true;
            discDialog.Owner = this;

            graphicListView.SelectedItem = graphicListView.Items[index];
            DataRowView row = graphicListView.SelectedItem as DataRowView;
            discDialog.txReport.Text = row[3].ToString();
            discDialog.txReporter.Text = row[4].ToString();
            if (row[5].ToString() != "")
            {
                // 增加字符串的时间验证...
                discDialog.dateReport.SelectedDate = DateTime.Parse(row[5].ToString());
            }

            Nullable<bool> result = discDialog.ShowDialog();

            if (result == true)
            {
                switch (gbClone.GetType().Name)
                {
                    case "GraphicsLine":
                        {
                            gl = (GraphicsLine)gbClone;
                            gl.ReportContent = discDialog.txReport.Text;
                            gl.Reporter = discDialog.txReporter.Text;
                            gl.ReportDate = discDialog.dateReport.ToString();

                            ivCamera.annotationCanvas.GraphicsList.RemoveAt(index);
                            ivCamera.annotationCanvas.GraphicsList.Insert(index, gl);
                            break;
                        }

                    case "GraphicsArrow":
                        {
                            ga = (GraphicsArrow)gbClone;
                            ga.ReportContent = discDialog.txReport.Text;
                            ga.Reporter = discDialog.txReporter.Text;
                            ga.ReportDate = discDialog.dateReport.ToString();

                            ivCamera.annotationCanvas.GraphicsList.RemoveAt(index);
                            ivCamera.annotationCanvas.GraphicsList.Insert(index, ga);
                            break;
                        }
                    case "GraphicsRuler":
                        {
                            grl = (GraphicsRuler)gbClone;
                            grl.ReportContent = discDialog.txReport.Text;
                            grl.Reporter = discDialog.txReporter.Text;
                            grl.ReportDate = discDialog.dateReport.ToString();

                            ivCamera.annotationCanvas.GraphicsList.RemoveAt(index);
                            ivCamera.annotationCanvas.GraphicsList.Insert(index, grl);
                            break;
                        }
                    case "GraphicsPolyLine":
                        {
                            gpl = (GraphicsPolyLine)gbClone;

                            gpl.ReportContent = discDialog.txReport.Text;
                            gpl.Reporter = discDialog.txReporter.Text;
                            gpl.ReportDate = discDialog.dateReport.ToString();

                            ivCamera.annotationCanvas.GraphicsList.RemoveAt(index);
                            ivCamera.annotationCanvas.GraphicsList.Insert(index, gpl);
                            break;
                        }
                    case "GraphicsRectangle":
                        {
                            gr = (GraphicsRectangle)gbClone;

                            gr.ReportContent = discDialog.txReport.Text;
                            gr.Reporter = discDialog.txReporter.Text;
                            gr.ReportDate = discDialog.dateReport.ToString();

                            ivCamera.annotationCanvas.GraphicsList.RemoveAt(index);
                            ivCamera.annotationCanvas.GraphicsList.Insert(index, gr);
                            break;
                        }
                    case "GraphicsEllipse":
                        {
                            ge = (GraphicsEllipse)gbClone;
                            ge.ReportContent = discDialog.txReport.Text;
                            ge.Reporter = discDialog.txReporter.Text;
                            ge.ReportDate = discDialog.dateReport.ToString();

                            ivCamera.annotationCanvas.GraphicsList.RemoveAt(index);
                            ivCamera.annotationCanvas.GraphicsList.Insert(index, ge);
                            break;
                        }
                    case "GraphicsText":
                        {
                            gt = (GraphicsText)gbClone;

                            gt.ReportContent = discDialog.txReport.Text;
                            gt.Reporter = discDialog.txReporter.Text;
                            gt.ReportDate = discDialog.dateReport.ToString();

                            ivCamera.annotationCanvas.GraphicsList.RemoveAt(index);
                            ivCamera.annotationCanvas.GraphicsList.Insert(index, gt);
                            break;
                        }
                    default:
                        break;
                }

                FillAnnotationDataView(ivCamera.annotationCanvas.GraphicsList);
            }
        }

        private void graphicListView_KeyDown(object sender, KeyEventArgs e)
        {
            int index = graphicListView.SelectedIndex;
            if (index >= 0)
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        annotationCanvas.GraphicsList.RemoveAt(index);
                        annotationCanvas.OnGraphicListChanged(null);

                        break;
                    default:
                        break;
                }
            }
        }

        #endregion Other Functions

        #endregion

        #region mSlideViewer Window
        private void ApplicationExit_Click(object sender, RoutedEventArgs e)
        {
            AppClose();
        }


        private void AppClose()
        {
            if (inputTiffFile != null)
                inputTiffFile.Close();

            if (imageCanvas.VirtualChildren.Count > 0)
                imageCanvas.VirtualChildren.Clear();

            // 是否保存截图到指定文件夹
            if (SettingsManager.ApplicationSettings.SaveSnapShotOnExit && lbSnapShot.Items.Count > 0)
            {
                SaveSnapShots(lbSnapShot);
            }

            // 保存应用设置
            SettingsManager.OnExit();

            // 保存Favorite Items 2017-08-12
            if (SettingsManager.ApplicationSettings.FavoriteItemsSaveOnExit && lbFavorite.Items.Count > 0)
            {
                SaveFavoriteItems();
            }

            
            if (annotationCanvas.GraphicsList.Count > 0)
            {
                // MainWindow_Closing 已做标注保存提示
                annotationCanvas.Clear();
            }                    

            this.Close();
        }


        private void SaveSnapShots(DependencyObject targetElement)
        {
            var count = VisualTreeHelper.GetChildrenCount(targetElement);
            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(targetElement, i);
                if (child is Image)
                {
                    Image targetItem = (Image)child;

                    string filePath = "SnapShots\\snapshot_" + i.ToString() + ".png";
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)targetItem.Source));
                        encoder.Save(fileStream);
                    }
                }
                else
                {
                    SaveSnapShots(child);                    
                }
            }
        }      

        private void MenuOption_Click(object sender, RoutedEventArgs e)
        {
            OptionDialog optionDialog = new OptionDialog();
            Nullable<bool> result = optionDialog.ShowDialog();
        }

        private void menuGoto_Click(object sender, RoutedEventArgs e)
        {
            if (originImageWidth > 0 && originImageHeight > 0)
            {
                GotoDialog gotoDialog = new GotoDialog();
                string info = "Image Information:\n Width:" + originImageWidth.ToString() + "\n";
                info += " Height:" + originImageHeight.ToString() + "\n";

                gotoDialog.XPositionMax = originImageWidth;
                gotoDialog.YPositionMax = originImageHeight;
                gotoDialog.tbImageInfo.Text = info;
                gotoDialog.Topmost = true;
                gotoDialog.Owner = this;
                bool? result = gotoDialog.ShowDialog();

                if (result == true)
                {
                    ivCamera.Focus();
                    if (magnifier == Magnifier.X20)
                    {
                        switch (gotoDialog.Mag)
                        {
                            case "20.0X":
                                ivCamera.CurrentPage = 0;
                                ivCamera.FullScaleZoom = 1;
                                mapZoom.Zoom = 1.0;

                                break;
                            case "10.0X":
                                ivCamera.CurrentPage = 0;
                                ivCamera.FullScaleZoom = 0.5;
                                mapZoom.Zoom = 0.5;
                                break;
                            case "5.0X":
                                ivCamera.CurrentPage = 1;
                                ivCamera.FullScaleZoom = 0.25;
                                mapZoom.Zoom = 1.0;
                                break;
                            case "3.75X":
                                ivCamera.CurrentPage = 1;
                                ivCamera.FullScaleZoom = 0.1875;
                                mapZoom.Zoom = 0.75;
                                break;
                            case "2.5X":
                                ivCamera.CurrentPage = 1;
                                ivCamera.FullScaleZoom = 0.125;
                                mapZoom.Zoom = 0.5;
                                break;
                            case "1.25X":
                                ivCamera.CurrentPage = 2;
                                ivCamera.FullScaleZoom = 0.0625;
                                mapZoom.Zoom = 1.0;
                                break;
                        }
                    }

                    if (magnifier == Magnifier.X40)
                    {
                        switch (gotoDialog.Mag)
                        {
                            case "40.0X":
                                ivCamera.CurrentPage = 0;
                                ivCamera.FullScaleZoom = 1;
                                break;
                            case "20.0X":
                                ivCamera.CurrentPage = 0;
                                ivCamera.FullScaleZoom = 0.5;
                                break;
                            case "10.0X":
                                ivCamera.CurrentPage = 1;
                                ivCamera.FullScaleZoom = 0.25;
                                break;
                            case "7.5X":
                                ivCamera.CurrentPage = 1;
                                ivCamera.FullScaleZoom = 0.1875;
                                break;
                            case "5.0X":
                                ivCamera.CurrentPage = 1;
                                ivCamera.FullScaleZoom = 0.125;
                                break;
                            case "2.5X":
                                ivCamera.CurrentPage = 2;
                                ivCamera.FullScaleZoom = 0.0625;
                                break;
                        }
                    }
                    //gotoDialog.XPosition * mapZoom.Zoom
                    AllocateNodes(ref imageCanvas, tileTiffReader, inputTiffFile, ivCamera.CurrentPage);

                    // 尚未考虑当图像ExtendWidth 小于 ViewPortWidth的情况， 2017-08-25 wkj
                    if (ivCamera.imageCanvas.ExtentWidth < ivCamera.ScrollImageViewer.ViewportHeight)
                        ivCamera.ScrollToImageCenter(new Point(gotoDialog.XPosition, gotoDialog.YPosition), new Point(0, 0));
                    else
                        ivCamera.ScrollToImageCenter(new Point(gotoDialog.XPosition, gotoDialog.YPosition), new Point(ivCamera.ScrollImageViewer.ViewportWidth / 2, ivCamera.ScrollImageViewer.ViewportHeight / 2));
                }
            }
        }
        #endregion

        #region RisCapture
        private void SnapShot_Click(object sender, RoutedEventArgs e)
        {
            Thread.Sleep(300);
            screenCaputre.StartCaputre(30, lastSize);
        }


        private System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// 使用System.Drawing.Image创建WPF使用的ImageSource类型缩略图（不放大小图）
        /// </summary>
        /// <param name="sourceImage">System.Drawing.Image 对象</param>
        /// <param name="width">指定宽度</param>
        /// <param name="height">指定高度</param>
        public static ImageSource CreateImageSourceThumbnia(System.Drawing.Image sourceImage, double width, double height)
        {
            if (sourceImage == null) return null;
            double rw = width / sourceImage.Width;
            double rh = height / sourceImage.Height;
            var aspect = (float)Math.Min(rw, rh);
            int w = sourceImage.Width, h = sourceImage.Height;
            if (aspect < 1)
            {
                w = (int)Math.Round(sourceImage.Width * aspect);
                h = (int)Math.Round(sourceImage.Height * aspect);
            }

            /*
                float iScale = bs.Height > bs.Width ? (float)bs.Height / 220 : (float)bs.Width / 180;
                int w = (int)(bs.Width / iScale);
                int h = (int)(bs.Height / iScale); 
            */

            System.Drawing.Bitmap sourceBmp = new System.Drawing.Bitmap(sourceImage, w, h);
            IntPtr hBitmap = sourceBmp.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();
            DeleteObject(hBitmap);
            sourceImage.Dispose();
            sourceBmp.Dispose();

            return bitmapSource;
        }

        private void OnScreenCaputreCancelled(object sender, System.EventArgs e)
        {
            Show();
            Focus();
        }

        private void OnScreenCaputred(object sender, RisCaptureLib.ScreenCaputredEventArgs e)
        {
            //set last size
            lastSize = new Size(e.Bmp.Width, e.Bmp.Height);

            Show();

            var bmp = e.Bmp;
            RisCaptureWin = new Window { SizeToContent = SizeToContent.WidthAndHeight, ResizeMode = ResizeMode.NoResize };

            var canvas = new Canvas { Width = bmp.Width, Height = bmp.Height, Background = new ImageBrush(bmp) };

            RisCaptureWin.Content = canvas;
            RisCaptureWin.Title = "Capture Picture [ " + bmp.Width.ToString() + " ×" + bmp.Height.ToString() + "px]";
            RisCaptureWin.Show();

            MenuItem menuItem;

            ContextMenu RisCaptureContextMenu = new ContextMenu();

            menuItem = new MenuItem();
            menuItem.Header = "Save As... (_A)";
            menuItem.Tag = RisCaptureContextMenuCommand.SaveAs;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            RisCaptureContextMenu.Items.Add(menuItem);
            RisCaptureWin.MouseDoubleClick += new MouseButtonEventHandler(RisCaptureWin_Click);

            menuItem = new MenuItem();
            menuItem.Header = "Save to SnapShot (_S)";
            menuItem.Tag = RisCaptureContextMenuCommand.SaveToSnapShot;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            RisCaptureContextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "Exit (_X)";
            menuItem.Tag = RisCaptureContextMenuCommand.Exit;
            menuItem.Click += new RoutedEventHandler(contextMenuItem_Click);
            RisCaptureContextMenu.Items.Add(menuItem);

            RisCaptureWin.ContextMenu = RisCaptureContextMenu;
        }


        Window RisCaptureWin;

        void RisCaptureWin_Click(object sender, RoutedEventArgs e)
        {
            RisCaptureSaveToSnapShot();
        }
        void contextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item == null)
            {
                return;
            }

            RisCaptureContextMenuCommand command = (RisCaptureContextMenuCommand)item.Tag;

            switch (command)
            {
                case RisCaptureContextMenuCommand.SaveAs:
                    RisCaptureSaveAs();
                    break;
                case RisCaptureContextMenuCommand.SaveToSnapShot:
                    RisCaptureSaveToSnapShot();
                    break;
                case RisCaptureContextMenuCommand.Exit:
                    RisCaptureExit();
                    break;
            }
        }

        private void RisCaptureSaveAs()
        {
            // Show Save File dialog
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Filter = "Image file (*.png)|*.png";
            dlg.OverwritePrompt = true;
            dlg.DefaultExt = "png";
            dlg.InitialDirectory = SettingsManager.ApplicationSettings.InitialDirectory;
            dlg.RestoreDirectory = true;
            dlg.FileName = "Untitled.png";

            string fileName;
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                fileName = dlg.FileName;

                Canvas cs = RisCaptureWin.Content as Canvas;

                var rtb = new RenderTargetBitmap(
                    (int)cs.Width, //width
                    (int)cs.Height, //height
                    96, //dpi x
                    96, //dpi y
                    PixelFormats.Pbgra32 // pixelformat
                    );
                rtb.Render(cs);

                SaveRTBAsPNG(rtb, fileName);

                // 保存后关闭截图窗口
                RisCaptureWin.Close();
            }
        }

        private static void SaveRTBAsPNG(RenderTargetBitmap bmp, string filename)
        {
            var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
            enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));

            using (var stm = System.IO.File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        private void RisCaptureExit()
        {
            if (RisCaptureWin != null)
                RisCaptureWin.Close();
        }

        int index_snapshot = 0;
        private void RisCaptureSaveToSnapShot()
        {
            Canvas cs = RisCaptureWin.Content as Canvas;

            var rtb = new RenderTargetBitmap(
                (int)cs.Width, //width
                (int)cs.Height, //height
                96, //dpi x
                96, //dpi y
                PixelFormats.Pbgra32 // pixelformat
                );
            rtb.Render(cs);

            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);

            System.Drawing.Image sourceImage = System.Drawing.Image.FromStream(stream);
            System.Drawing.Bitmap sourceBmp = new System.Drawing.Bitmap(sourceImage, 160, 120);

            IntPtr hBitmap = sourceBmp.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            lbSnapShot.Items.Insert(index_snapshot, new { ImgPath = bitmapSource, ImgTxt = "SnapShot_" + index_snapshot.ToString() });
            index_snapshot++;
            DeleteObject(hBitmap);
            sourceImage.Dispose();
            sourceBmp.Dispose();
            stream.Dispose();

            RisCaptureWin.Close();
        }

        private void btDeleteSnapShot_Click(object sender, RoutedEventArgs e)
        {
            if (lbSnapShot.SelectedIndex >= 0 && index_snapshot >= 0)
            {
                index_snapshot--;
                lbSnapShot.Items.RemoveAt(lbSnapShot.SelectedIndex);
            }
        }
        #endregion

        #region mSlideTalk 
        private void btTalkLogin_Click(object sender, RoutedEventArgs e)
        {
            string path = SettingsManager.ApplicationSettings.mSlideTalkAppPath;
            if (path != "" && path.Contains("mSlideTalk.exe"))
            {
                Process.Start(path);
            }
        }

        private void btTalkSettings_Click(object sender, RoutedEventArgs e)
        {
            OptionDialog optionDialog = new OptionDialog();
            optionDialog.mSlideTalk.IsSelected = true;

            Nullable<bool> result = optionDialog.ShowDialog();

            if (result == true)
            {


            }
        }
        #endregion
   
        //搜索词条 
        #region Favorite Items Manager
        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txSearch.Text != "")
            {
                DataSet ds;
                try
                {
                    ds = new DataSet();
                    ds.Reset();
                    //可选择中文或英文xml
                    // ds.ReadXml("ICDOdata.xml");
                    ds.ReadXml("ICDOdata中文.xml");
                }
                catch
                {
                    MessageBox.Show("XML文件打开失败");
                    return;
                }

                DataRow[] drs = ds.Tables["subTerm"].Select();
                DataRow[] drm = ds.Tables["mainTerm"].Select();
                lbSearch.Items.Clear();
                CompareInfo Compare = CultureInfo.InvariantCulture.CompareInfo;
                foreach (DataRow dr in drs)
                {
                    if (Compare.IndexOf(dr["name"].ToString(), txSearch.Text, CompareOptions.IgnoreCase) != -1)
                    {
                        CheckBox b = new CheckBox();
                        b.Content = dr["name"].ToString();
                        lbSearch.Items.Add(b);
                    }
                }
                foreach (DataRow dr in drm)
                {
                    if (Compare.IndexOf(dr["name"].ToString(), txSearch.Text, CompareOptions.IgnoreCase) != -1
                        || Compare.IndexOf(dr["code"].ToString(), txSearch.Text, CompareOptions.IgnoreCase) != -1)
                    {
                        CheckBox b = new CheckBox();
                        b.Content = dr["name"].ToString() + " ,code:" + dr["code"].ToString();
                        lbSearch.Items.Add(b);
                    }
                }
            }
        }

        // 添加Tree中的选中项和SearchResult中的选中项到Favorite框
        public void LoadFavoriteItems()
        {
            // MTY - 首先判断是否存在FavoriteGlossaryItems文件夹，是否存在 FavoriteItems.xml 文件   
            // 判断文件的存在
            if (File.Exists("FavoriteGlossaryItems/FavoriteItems.xml") == false)
            {
                File.Create("FavoriteGlossaryItems/FavoriteItems.xml");
            }
            else
            {
                DataSet ds;
                try
                {
                    ds = new DataSet();
                    ds.Reset();
                    ds.ReadXml("FavoriteGlossaryItems/FavoriteItems.xml");
                }
                catch
                {
                    MessageBox.Show("xml文件打开失败");
                    return;
                }

                //添加tree中的选中项到Favorite框
                DataRow[] drm = ds.Tables["mainTerm"].Select();
                DataRow[] drs = ds.Tables["subTerm"].Select();

                foreach (DataRow dr in drm)
                {
                    if (dr["check"].ToString() == "True" && lbFavorite.Items.Contains(dr["name"].ToString() + " ,code:" + dr["code"].ToString()) == false)
                    {
                        lbFavorite.Items.Add(dr["name"].ToString() + " ,code:" + dr["code"].ToString());
                    }
                }

                foreach (DataRow dr in drs)
                {
                    if (dr["check"].ToString() == "True" && lbFavorite.Items.Contains(dr["name"].ToString()) == false)
                    {
                        lbFavorite.Items.Add(dr["name"].ToString());
                    }
                }
            }
        }

      
        private void btFavoriteItemsAdd_Click(object sender, RoutedEventArgs e)
        {
            // Default path of Favorite Items File
            string path = "FavoriteGlossaryItems/FavoriteItems.xml";
            
            XmlDataProvider xmldata = Resources["xmldata"] as XmlDataProvider;
            xmldata.Document.Save(path);

            if (File.Exists(path) == true)
            {
                try
                {
                    if (lbSearch.Items.Count > 0)
                    {
                        //添加SearchResult中的选中项到Favorite框
                        foreach (CheckBox t in lbSearch.Items)
                        {
                            string w1 = t.Content.ToString();
                            if (t.IsChecked == true && lbFavorite.Items.Contains(w1) == false)
                            {
                                lbFavorite.Items.Add(w1);
                            }
                        }
                    }

                    // 根据 Check记录数组，如果count>0,执行以下操作
                    // 增加 Treeview Check 事件的记录功能


                    //将tree中的选中项信息同步至newdata.xml                    
                    DataSet ds;
                    try
                    {
                        ds = new DataSet();
                        ds.Reset();
                        ds.ReadXml(path);
                    }
                    catch
                    {
                        MessageBox.Show("xml文件打开失败");
                        return;
                    }

                    //添加tree中的选中项到Favorite框
                    DataRow[] drm = ds.Tables["mainTerm"].Select();
                    DataRow[] drs = ds.Tables["subTerm"].Select();

                    foreach (DataRow dr in drm)
                    {
                        if (dr["check"].ToString() == "True" && lbFavorite.Items.Contains(dr["name"].ToString() + " ,code:" + dr["code"].ToString()) == false)
                        {
                            lbFavorite.Items.Add(dr["name"].ToString() + " ,code:" + dr["code"].ToString());
                        }
                    }
                    foreach (DataRow dr in drs)
                    {
                        if (dr["check"].ToString() == "True" && lbFavorite.Items.Contains(dr["name"].ToString()) == false)
                        {
                            lbFavorite.Items.Add(dr["name"].ToString());
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("filed");
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);

                //  XmlNode node1 = xmlDoc.CreateNode(XmlNodeType.Element, "User", null);
                XmlNode root = xmlDoc.SelectSingleNode("root");
                XmlNodeList rootlist = root.SelectNodes("mainTerm");
                foreach (XmlNode rootX in rootlist)
                {
                    XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, "ImageID", null);
                    rootX.AppendChild(node);
                }
                xmlDoc.Save(path);
            }
            else
            {
                return;
            }
        }

        private void DefaultFavoriteItemsFileCreate(string path)
        {
            File.Create(path);
            // 创建XML基本结构

        }

        private void SaveFavoriteItems()
        {
            if (lbFavorite.Items.Count > 0)
            {
                if (File.Exists("FavoriteGlossaryItems/FavoriteItems.xml") == false)
                {
                    File.Create("FavoriteGlossaryItems/FavoriteItems.xml");
                }
                string path = "FavoriteGlossaryItems/FavoriteItems.xml";
                ExportXmlFile(path);
            }
        }

        private void btFavoriteItemsImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Favorate Glossary (*.xml)|*.xml";
            string path = null;
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                path = dlg.FileName;

                try
                {      
                    // 清空 lbFavorite   

                    // 将tree中的选中项信息同步至newdata.xml                    
                    DataSet ds;
                    try
                    {
                        ds = new DataSet();
                        ds.Reset();
                        ds.ReadXml(path);
                    }
                    catch
                    {
                        MessageBox.Show("xml文件打开失败");
                        return;
                    }

                    //添加tree中的选中项到Favorite框
                    DataRow[] drm = ds.Tables["mainTerm"].Select();
                    DataRow[] drs = ds.Tables["subTerm"].Select();

                    foreach (DataRow dr in drm)
                    {
                        if (dr["check"].ToString() == "True" && lbFavorite.Items.Contains(dr["name"].ToString() + " ,code:" + dr["code"].ToString()) == false)
                        {
                            lbFavorite.Items.Add(dr["name"].ToString() + " ,code:" + dr["code"].ToString());                        
                        }
                    }

                    foreach (DataRow dr in drs)
                    {
                        if (dr["check"].ToString() == "True" && lbFavorite.Items.Contains(dr["name"].ToString()) == false)
                        {
                            lbFavorite.Items.Add(dr["name"].ToString());
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("failed");
                }
            }
        }

        private void btFavoriteItemsExport_Click(object sender, RoutedEventArgs e)
        {
            if (lbFavorite.Items.Count > 0)
            {
                SaveFileDialog XmlFileSave = new SaveFileDialog();
                Nullable<bool> result = XmlFileSave.ShowDialog();
                XmlFileSave.Filter = "Favorite Glossary(*.xml)|*.*";
                XmlFileSave.FilterIndex = 1;
                string path = null;
                if (result == XmlFileSave.ShowDialog())
                {
                    path = XmlFileSave.FileName;
                    ExportXmlFile(path);
                }
            }
        }

        public void CreateNode(XmlDocument xmlDoc, XmlNode parentNode, string name, string value)
        {
            XmlNode node = xmlDoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentNode.AppendChild(node);
        }

        public void ExportXmlFile(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //创建类型声明节点  
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(node);
            //创建根节点  
            XmlNode root = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(root);
            XmlNode node1 = xmlDoc.CreateNode(XmlNodeType.Element, "mainTermName", null);
            for (int k = 0; k < lbFavorite.Items.Count; k++)
            {

                CreateNode(xmlDoc, node1, "SubTermName", lbFavorite.Items[k].ToString());

            }

            root.AppendChild(node1);


            try
            {
                xmlDoc.Save(path);
            }
            catch (Exception e)
            {
                //显示错误信息  
                Console.WriteLine(e.Message);
            }
            //Console.ReadLine();  

        }

        private void btFavoriteItemsDelete_Click(object sender, RoutedEventArgs e)
        {
            lbFavorite.Items.Remove(lbFavorite.SelectedItem);
        }

        void ExpandAll(ItemsControl items, bool expand)
        {
            foreach (object obj in items.Items)
            {
                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null)
                {
                    ExpandAll(childControl, expand);
                }
                TreeViewItem item = childControl as TreeViewItem;
                if (item != null)
                    item.IsExpanded = true;
            }
        }


        //双击删除Favorite中的item,并更新favorite.txt [请修改，与Delete相同，调用同一个函数]
        private void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            lbFavorite.Items.Remove(lbFavorite.SelectedItem);
            StreamWriter sw = new StreamWriter("favorite.txt");
            string w = "";
            sw.Write(w);
            sw.Close();
            StreamWriter sw1 = File.AppendText("favorite.txt");
            foreach (string lb in lbFavorite.Items)
            {
                sw1.WriteLine(lb);
            }
            sw1.Close();
        }

        #endregion

        #region ImageView Canvas Visible & Hidden
        private void btZoomBar_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera != null)
            {
                if (btZoomBar.IsChecked == true)
                {
                    menuZoomBar.IsChecked = true;
                    if (ivCamera.zoombarCanvas.Visibility == Visibility.Hidden)            
                        ivCamera.zoombarCanvas.Visibility = Visibility.Visible;
                 
                }
                else
                {
                    menuZoomBar.IsChecked = false;
                    if (ivCamera.zoombarCanvas.Visibility == Visibility.Visible)
                        ivCamera.zoombarCanvas.Visibility = Visibility.Hidden;
                }
            }
        }
        private void menuZoomBar_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera != null)
            {
                if (menuZoomBar.IsChecked == true)
                {
                    btZoomBar.IsChecked = true;
                    if (ivCamera.zoombarCanvas.Visibility == Visibility.Hidden)                  
                        ivCamera.zoombarCanvas.Visibility = Visibility.Visible;               
                 
                }
                else
                {
                    btZoomBar.IsChecked = true;
                    if (ivCamera.zoombarCanvas.Visibility == Visibility.Visible)
                        ivCamera.zoombarCanvas.Visibility = Visibility.Hidden;
                }
            }
        }
        private void btThumbnail_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera != null)
            {

                if (btThumbnail.IsChecked == true)
                {
                    menuThumbnail.IsChecked = true;
                    if (ivCamera.thumbnailCanvas.Visibility == Visibility.Hidden)
                    {
                        ivCamera.thumbnailCanvas.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    menuThumbnail.IsChecked = false;
                    if (ivCamera.thumbnailCanvas.Visibility == Visibility.Visible)
                        ivCamera.thumbnailCanvas.Visibility = Visibility.Hidden;
                }
            }
        }
        /*
        private void GenerateOverview()
        {
            if (inputTiffFile != null)
            {
                int width = tileTiffReader.pageLevel[2].Page_Width;
                int height = tileTiffReader.pageLevel[2].Page_Height;
                int xCount = tileTiffReader.pageLevel[2].Tile_XCount;
                int yCount = tileTiffReader.pageLevel[2].Tile_YCount;

                Byte[] targetBuf = new Byte[width * height * 3];
                tileTiffReader.MultiTilesRead(inputTiffFile, out targetBuf, tileTiffReader.pageLevel[2], 0, 0, xCount, yCount);

                BitmapSource bs = BitmapSource.Create(width, height, 96d, 96d,
                                                    PixelFormats.Rgb24, null, targetBuf, 3 * width);

                ivCamera.ThumbnailImage = CreateImageSourceThumbnia(BitmapFromSource(bs), 250, 250);

          
                ivCamera.thumbnailBorder.Width = ivCamera.thumbnailCanvas.Width = ivCamera.ThumbnailImage.Width;
 
                ivCamera.thumbnailBorder.Height = ivCamera.thumbnailCanvas.Height = ivCamera.ThumbnailImage.Height;

                // Show ThumbnailCanvas    
                if (ivCamera.thumbnailCanvas.Visibility == Visibility.Hidden)
                    ivCamera.thumbnailCanvas.Visibility = Visibility.Visible;

                ivCamera.ThumbnailScale = (ivCamera.ThumbnailImage.Width / tileTiffReader.pageLevel[ivCamera.CurrentPage].Page_Width) / ivCamera.FullScaleZoom;
                double scale2 = ivCamera.ScrollImageViewer.ViewportWidth / (ivCamera.ScrollImageViewer.ViewportWidth + ivCamera.ScrollImageViewer.ViewportHeight);              

                // 缺省宽40pixel 
                ivCamera.ModifyThumbDisplay(0, 0, 40 * scale2, 40 * (1 - scale2));                
                ivCamera.RectThumbInit(ivCamera.ThumbnailScale);

                GC.Collect();
            }
            else
            {

            }
        }
        */
        private void menuThumbnail_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera != null)
            {
                if (menuThumbnail.IsChecked == true)
                {
                    btThumbnail.IsChecked = true;
                    if (ivCamera.thumbnailCanvas.Visibility == Visibility.Hidden)
                    {
                        ivCamera.thumbnailCanvas.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    btThumbnail.IsChecked = false;
                    if (ivCamera.thumbnailCanvas.Visibility == Visibility.Visible)
                        ivCamera.thumbnailCanvas.Visibility = Visibility.Hidden;
                }
            }
        }
       
        private void menuMagnifier_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera != null)
            {
                if (menuMagnifier.IsChecked == true)
                {
                    btMagnifier.IsChecked = true;
                    ivCamera.magnifierCanvas.Visibility = Visibility.Visible;
                }
                else
                {
                    btMagnifier.IsChecked = false;
                    ivCamera.magnifierCanvas.Visibility = Visibility.Hidden;
                }
            }
        }
        private void btMagnifier_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera != null)
            {
                if (btMagnifier.IsChecked == true)
                {
                    menuMagnifier.IsChecked = true;
                    ivCamera.magnifierCanvas.Visibility = Visibility.Visible;
                }
                else
                {
                    menuMagnifier.IsChecked = false;
                    ivCamera.magnifierCanvas.Visibility = Visibility.Hidden;
                }
            }
        }
        private void btSlideLabel_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera != null)
            {
                if (btSlideLabel.IsChecked == true)
                {
                    menuSlideLabel.IsChecked = true;
                    ivCamera.labelImageCanvas.Visibility = Visibility.Visible;
                    ivCamera.magnifierCanvas.Margin = new Thickness(175, 5, 0, 0); 
                }
                else
                {
                    menuSlideLabel.IsChecked = false;
                    ivCamera.labelImageCanvas.Visibility = Visibility.Hidden;
                    ivCamera.magnifierCanvas.Margin = new Thickness(5, 5, 0, 0);
                }
            }
        }

        private void menuSlideLabel_Click(object sender, RoutedEventArgs e)
        {
            if (ivCamera != null)
            {
                if (menuSlideLabel.IsChecked == true)
                {
                    btSlideLabel.IsChecked = true;
                    ivCamera.labelImageCanvas.Visibility = Visibility.Visible;
                }
                else
                {
                    btSlideLabel.IsChecked = false;
                    ivCamera.labelImageCanvas.Visibility = Visibility.Hidden;
                }
            }
        }

        private void menuStatusBar_Click(object sender, RoutedEventArgs e)
        {
            if (menuStatusBar.IsChecked == true)
            {
                statusBar.Visibility = Visibility.Visible;
            }
            else
            {
                statusBar.Visibility = Visibility.Hidden;

            }
        }
        #endregion

        #region AvalonDock
        private void OnLayoutRootPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
        #endregion

        private void menuInformation_Click(object sender, RoutedEventArgs e)
        {
            if (aperioTileTiffRead != null)
            {
                ImageInfo aperioinfo = new ImageInfo(aperioTileTiffRead);
                Nullable<bool> result = aperioinfo.ShowDialog();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            AppClose();
        }

        private void OpenImageType2_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }

    public class MyCommands
    {
        public static RoutedUICommand MenuGotoCommand =
            new RoutedUICommand("Go to...", "MenuGotoCommand", typeof(MyCommands),
                new InputGestureCollection(new InputGesture[] {
                    new KeyGesture(Key.G, ModifierKeys.Control) }));
    }
}
