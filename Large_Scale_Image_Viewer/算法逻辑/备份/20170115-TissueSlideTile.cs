using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Globalization;
using System.Runtime.InteropServices;
using BitMiracle.LibTiff.Classic;
using mSlideViewer;


using System.Diagnostics;

namespace VirtualCanvasLib
{
    class TissueSlideTile : IVirtualChild
    {
        #region Class Members
        Rect _bounds;
        UIElement _visual;
        int _xGrid;
        int _yGrid;
        int _currentPage;

        public Tiff _inputTiffFile;        
        Rectangle _rect;
    
        #endregion

       public event EventHandler BoundsChanged;

        public TissueSlideTile(Rect bounds)
        {
            _bounds = bounds;
            
            _xGrid = (int)(bounds.X / 256.0);
            _yGrid = (int)(bounds.Y / 256.0);

            _currentPage = 0;

            _rect = new Rectangle();
            _rect.Width = 256;
            _rect.Height = 256;
            _rect.StrokeThickness = 0;
            _rect.Stroke = Brushes.Transparent;
            
        }

        public UIElement Visual
        {
            get { return _visual; }
        }

        public int CurrentPage
        {
            set { _currentPage = value; }
        }

        public Tiff InputTiffFile
        {
            set { _inputTiffFile = value; }
        }

     
        byte[] _tileTiffBuffer;
        public byte [] TileTiffBuffer
        {
            set { _tileTiffBuffer = value; }
        }
        public Byte[] AcquireVisualData(VirtualCanvas parent)
        {
            Byte[] targetBuffer = new byte[256 * 256 * 3];
            MemoryStream stream1 = new MemoryStream(_tileTiffBuffer, false);
            TiffStream streamTIFF1 = new TiffStream();
            Tiff tiff_file = Tiff.ClientOpen("In-Memory-Target", "r", stream1, streamTIFF1);

            tiff_file.SetDirectory((short)_currentPage); // page必须为short类型
            int tile_width = tiff_file.GetField(TiffTag.TILEWIDTH)[0].ToInt();
            int image_width = tiff_file.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int tile_xCount = image_width / tile_width;
            tiff_file.ReadEncodedTile(_xGrid + _yGrid * tile_xCount, targetBuffer, 0, tile_width * tile_width * 3);


            return targetBuffer;
        }

        public UIElement CreateVisual(VirtualCanvas parent)
        {
            if (_visual == null)
            {
                byte[] targetBuffer = new byte[256*256*3];

                //      var stopWatch = Stopwatch.StartNew();

                // 【期待加速】   
               
          //      Task t1 = Task.Factory.StartNew(() =>
          //      {
          //          MemoryStream stream1 = new MemoryStream(_tileTiffBuffer, false);
          //          TiffStream streamTIFF1 = new TiffStream();
          //          Tiff tiff_file = Tiff.ClientOpen("In-Memory-Target", "r", stream1, streamTIFF1);

                    _inputTiffFile.SetDirectory((short)_currentPage); // page必须为short类型
                    int tile_width = _inputTiffFile.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                    int image_width = _inputTiffFile.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                    int tile_xCount = image_width / tile_width;
                    _inputTiffFile.ReadEncodedTile(_xGrid + _yGrid * tile_xCount, targetBuffer, 0, tile_width * tile_width * 3);
        //        });

          //      t1.Wait();

           
                //  耗时 10ms - 12ms
                //        Console.WriteLine("1加载1个tile耗时 = {0} us\n", stopWatch.Elapsed.TotalSeconds * 1000000);

                // 耗时 200us
                ImageBrush imgBrush = new ImageBrush(BitmapSource.Create(256, 256, 96d, 96d,
                                                 PixelFormats.Rgb24, null, targetBuffer, 768)); // StrideSize = 768 = 1 Tile * 3 Channels * 256 pixels

                // Fill rectangle with an ImageBrush
                _rect.Fill = imgBrush;
                _visual = _rect;

                /*********************************** 多线程更新  *************************************/
                //   Action<UIElement,ImageBrush> updateAction = new Action<UIElement, ImageBrush>(UpdateShape);
                //   parent.Dispatcher.BeginInvoke(updateAction, _visual,  imgBrush);

                //   Task.Factory.StartNew(() => UpdateShape(_visual, imgBrush),
                //      new CancellationTokenSource().Token, TaskCreationOptions.None, _syncContextTaskScheduler).Wait();

                //   Task.Factory.StartNew(() => UpdateShape(_visual, imgBrush), new CancellationTokenSource().Token).Wait();
            }
            return _visual;
        }

    //    private readonly TaskScheduler _syncContextTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        private void UpdateShape(UIElement visual, ImageBrush imageBrush)
        {
            Rectangle rect = new Rectangle();
            rect.Width = 256;
            rect.Height = 256;
            rect.StrokeThickness = 0;
            rect.Stroke = Brushes.Transparent;

            rect.Fill = imageBrush;
            visual = rect;
        }

        public void DisposeVisual()
        {
            _visual = null;
        }

        public Rect Bounds
        {
            get { return _bounds; }
        }
    } 
}
