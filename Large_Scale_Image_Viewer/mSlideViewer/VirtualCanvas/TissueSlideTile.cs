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
    /// <summary>
    /// This interface is implemented by the objects that you want to put in the VirtualCanvas.
    /// </summary>
    public interface IVirtualChild
    {
        /// <summary>
        /// The bounds of your child object
        /// </summary>
        Rect Bounds { get; }

        /// <summary>
        /// Raise this event if the Bounds changes.
        /// </summary>
        event EventHandler BoundsChanged;

        /// <summary>
        /// Return the current Visual or null if it has not been created yet.
        /// </summary>
        UIElement Visual { get; }

        /// <summary>
        /// 预先获取图像数据.
        /// </summary>
        Byte[] TileBuf { get; set; }

        /// <summary>
        /// Create the WPF visual for this object.
        /// </summary>
        /// <param name="parent">The canvas that is calling this method</param>
        /// <returns>The visual that can be displayed</returns>
        UIElement CreateVisual(VirtualCanvas parent);

        /// <summary>
        /// 预先获取图像数据.
        /// </summary>
        bool FillVisualData(VirtualCanvas parent);   

        /// <summary>
        /// Dispose the WPF visual for this object.
        /// </summary>
        void DisposeVisual();
    }
    class TissueSlideTile : IVirtualChild
    {
        #region Class Members
        Rect _bounds;
        UIElement _visual;

        int _currentPage;
        byte[] _tileBuf;
        string path = null;
        byte[] _tiffFileBuffer;
        Rectangle _rect;
        int TileWidth;
        int TileHeight;
        #endregion

        public event EventHandler BoundsChanged;

        public TissueSlideTile(Rect bounds,int ImageWidth,int ImageHeight,string Path)
        {            
            _bounds = bounds;  
            _currentPage = 0;
            _tileBuf = null;
            TileWidth = ImageWidth;
            TileHeight = ImageHeight;
            _rect = new Rectangle();
           
            _rect.Width = ImageWidth;
            _rect.Height = ImageHeight;

            // 消除间隙【取巧而已】
            _rect.StrokeThickness = 0;
            _rect.Stroke = Brushes.Transparent;
            path = Path;
            
        }

        public UIElement Visual
        {
            get { return _visual; }
        }

        public Byte[] TileBuf
        {
            get { return _tileBuf; }
            set { _tileBuf = value; }
        }

        public int CurrentPage
        {
            set { _currentPage = value; }
        }
        
        public byte [] TileTiffBuffer
        {
            set { _tiffFileBuffer = value; }
        }        

        public bool FillVisualData(VirtualCanvas parent)
        {
            int offset = 0; 
            if (_tileBuf == null&&_currentPage==1)
            {
                _tileBuf = new byte[TileWidth * TileHeight * 3];

                int _xGrid = (int)(_bounds.X / TileWidth);
                int _yGrid = (int)(_bounds.Y / TileHeight);

                Tiff tiff_file = Tiff.ClientOpen("In-Memory-Target", "r", new MemoryStream(_tiffFileBuffer, false), new TiffStream());

                tiff_file.SetDirectory((short)_currentPage); // page必须为short类型
                int tile_width = tiff_file.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                int image_width = tiff_file.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
               // int tile_width = 66 * 256/4;
               // int image_width = 65 * 256/4;
                int tile_xCount = image_width / tile_width/4;
                if (tile_xCount * tile_width < image_width) { offset = 1; }
                 //creatVisualFromDisk(_xGrid, _yGrid,out _tileBuf,(int) _currentPage);如果想从文件夹里面读取要这样
                int result = tiff_file.ReadEncodedTile(_xGrid + _yGrid * (tile_xCount+offset), _tileBuf, 0, tile_width * tile_width * 3);

                if (_tileBuf.Length !=TileWidth * TileHeight * 3)
                    return false;   // _tileBuf is loaded with error.
                else
                    return true;
            }
            else if(_tileBuf == null && _currentPage == 0)
                {
                _tileBuf = new byte[TileWidth * TileHeight * 3];

                int _xGrid = (int)(_bounds.X / TileWidth );
                int _yGrid = (int)(_bounds.Y / TileHeight);

                Tiff tiff_file = Tiff.ClientOpen("In-Memory-Target", "r", new MemoryStream(_tiffFileBuffer, false), new TiffStream());

                tiff_file.SetDirectory((short)_currentPage); // page必须为short类型
                int tile_width = tiff_file.GetField(TiffTag.TILEWIDTH)[0].ToInt();
                int image_width = tiff_file.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                //int tile_width = 66 * 256 ;
               // int image_width = 65 * 256;
                int tile_xCount = image_width / tile_width ;
                if (tile_xCount * tile_width < image_width) { offset = 1; }
                //creatVisualFromDisk(_xGrid, _yGrid, out _tileBuf, (int)_currentPage);
                 int result = tiff_file.ReadEncodedTile(_xGrid + _yGrid * (tile_xCount+offset), _tileBuf, 0, tile_width * tile_width * 3);

                if (_tileBuf.Length != TileWidth * TileHeight * 3)
                    return false;   // _tileBuf is loaded with error.
                else
                    return true;
            }
            else
            {
                return false;  // _tileBuf is filled before;
            }
        }

        public UIElement CreateVisual(VirtualCanvas parent)
        {
            if (_visual == null && _tileBuf != null && _tileBuf.Length == TileWidth * TileHeight * 3)
            {
                ImageSource source = BitmapSource.Create(TileWidth, TileHeight, 96, 96,
                                                      PixelFormats.Rgb24, null, _tileBuf, TileWidth * 3);
                
                // Fill rectangle with an ImageBrush
                _rect.Fill = new ImageBrush(source); // StrideSize = 768 = 1 Tile * 3 Channels * 256 pixels    
                
                _visual = _rect;
                
            }
            return _visual;
        }  

        private bool creatVisualFromDisk(int xGrid,int yGrid,out byte[] Tile_buffer,int _currentpage)
        {
            string gridx = null;
            string gridy = null;
            string filename = null;
            string attach = null;
            string des;
            Tiff client;
            byte[] buffer = new byte[256 * 256 * 3];
            byte[] linebuffer = new byte[256 * 3];
            Tile_buffer = buffer;
          if(_currentpage==0)
            {
                attach = "1";
                 des =path + attach + "/";
                if (xGrid >= 0 && xGrid < 10 && yGrid >= 0 && yGrid < 10) { gridx = "000" + xGrid.ToString(); gridy = "000" + yGrid.ToString(); }
                if (xGrid >= 10 && yGrid >= 0 && yGrid < 10) { gridx = "00" + xGrid.ToString(); gridy = "000" + yGrid.ToString(); }
                if (xGrid >= 10 && yGrid >= 10) { gridx = "00" + xGrid.ToString(); gridy = "00" + yGrid.ToString(); }
                if (xGrid >= 0 && xGrid < 10 && yGrid >= 10) { gridx = "000" + xGrid.ToString(); gridy = "00" + yGrid.ToString(); }
                filename = des + gridx + "-" + gridy + ".tif";
               client = Tiff.Open(filename, "r");
                for (int i = 0; i < 256; i++)
                {
                    client.ReadScanline(linebuffer, i);
                    Buffer.BlockCopy(linebuffer, 0, buffer, 256 * 3 * i, 256 * 3);
                }
                Tile_buffer = buffer;

            }
          else if(_currentpage==1)
            {
                attach = "0.5";
                des = path + attach + "/";
                des = path + attach + "/";
                if (xGrid >= 0 && xGrid < 10 && yGrid >= 0 && yGrid < 10) { gridx = "000" + xGrid.ToString(); gridy = "000" + yGrid.ToString(); }
                if (xGrid >= 10 && yGrid >= 0 && yGrid < 10) { gridx = "00" + xGrid.ToString(); gridy = "000" + yGrid.ToString(); }
                if (xGrid >= 10 && yGrid >= 10) { gridx = "00" + xGrid.ToString(); gridy = "00" + yGrid.ToString(); }
                if (xGrid >= 0 && xGrid < 10 && yGrid >= 10) { gridx = "000" + xGrid.ToString(); gridy = "00" + yGrid.ToString(); }

                filename = des + gridx + "-" + gridy + ".tif";
                client = Tiff.Open(filename, "r");
                for (int i = 0; i < 256; i++)
                {
                    client.ReadScanline(linebuffer, i);
                    Buffer.BlockCopy(linebuffer, 0, buffer, 256 * 3 * i, 256 * 3);
                }
                Tile_buffer = buffer;
            }
          
          
            return true;
        }

        public void DisposeVisual()
        {
            _visual = null;
            
            _tileBuf = null;
        
        }

        public Rect Bounds
        {
            get { return _bounds; }
        }
    } 
}
