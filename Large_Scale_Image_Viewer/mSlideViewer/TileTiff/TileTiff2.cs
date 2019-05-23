// 2017-07-21 V2.4.5 wkj

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;
using Emgu.CV;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;


namespace mSlideViewer
{
    public struct PageLevel
    {
        public int Tile_Width;
        public int Tile_Height;
        public int tile_bytesize;
        public double DownSample;
        public int Page_Width;
        public int Page_Height;
        public short PageIndex;
        public int Tile_XCount;
        public int Tile_YCount;
    }

    public struct ScanImageInfo 
    {
        // Reference Aperio Tiff format
        public int Magnifier;
        public string FileName;
        public string AppMag;
        public string ScanDeviceID;
        public string Title;
        public string Date;
        public string TimeZone;      
        public int Filtered;
        public double FocusOffset;
        public string DSR_ID;
        public string ImageID;
        public double Left;
        public double LineAreaXOffset;
        public double LineAreaYOffset;
        public double LineCameraSkew;
        public double MPP;
        public double ExposureTime;
        public double ExposureScale;
        public int DisplayColor;
        public int OriginalHeight;
        public int OriginalWidth;
        public string ICCProfile;
        public string Parmset;
        public string DeviceID;
        public int StripWidth;
        public string ScanTime;  // 09:59:15
        public double Top;
        public string User;
        
        public string Comment;
        public double Mpp_X;
        public double Mpp_Y;
        public int Objective_Power;
        public string QuichHash; // Hash Code
        public string Vendor;
        public string ImageDseciption;
        public string ResolutionUnit; // inch
        public int nTileCounts;   
    }

    /// <summary>
    /// Tiff Multi-Page
    /// </summary>
    public enum TiffPage
    {
        Page0 = 0,
        Thumbnail = 3,
        Page1 = 1,
        Page2 = 2,
        Label = 4,
        Macro = 5
    }

    /*
    /// <summary>
    /// TiffPage - Aperio Tiff format
    /// </summary>
    public enum TiffPage
    {
        Page0 = 0,
        Thumbnail = 1;
        Page1 = 2,
        Page2 = 3,
        Label = 4,
        Macro = 5
    }
    */

    #region TileTiffRead
    public class TileTiffRead
    { 
        public PageLevel[] pageLevel = new PageLevel[3];
        public Byte[] TargetBuf;
        public TileTiffRead(int ImageWidth,int ImageHeight)
        {
            // PageLevel Configuration
            pageLevel[0].Tile_Height = ImageHeight;
            pageLevel[0].Tile_Width = ImageWidth;
            pageLevel[0].DownSample = 1.0;
            pageLevel[0].tile_bytesize = ImageWidth * ImageHeight * 3;
            pageLevel[0].PageIndex = 0;

            pageLevel[1].Tile_Height =ImageHeight;
            pageLevel[1].Tile_Width = ImageWidth;
            pageLevel[1].DownSample = 1.0 / 4;
            pageLevel[1].tile_bytesize = ImageWidth * ImageHeight * 3;
            pageLevel[1].PageIndex = 1;

            pageLevel[2].Tile_Height = ImageHeight;
            pageLevel[2].Tile_Width = ImageWidth;
            pageLevel[2].DownSample = 1.0 / 16;
            pageLevel[2].tile_bytesize = ImageHeight * ImageWidth * 3;
            pageLevel[2].PageIndex = 2;
        }

        public void MultiTilesRead(Tiff InputTiffFile, out Byte[] targetBuf, PageLevel pageLevel, int xGrid, int yGrid, int xTiles, int yTiles)
        {
            Compression compression = (Compression)InputTiffFile.GetField(TiffTag.COMPRESSION)[0].ToInt();
            if (compression != Compression.JPEG | xGrid < 0 | yGrid < 0)
            {
                targetBuf = null;
                return;
            }

            if ((pageLevel.Tile_Width == pageLevel.Tile_Height) && (pageLevel.PageIndex <=2))
            {
                InputTiffFile.SetDirectory(pageLevel.PageIndex); // page必须为short类型     
                InputTiffFile.ReadMultiEncodedTile(out TargetBuf, xGrid, yGrid, xTiles, yTiles, 0, pageLevel.tile_bytesize);
                
               /*  以下代码已被  Tiff ReadMultiEncodedTile 替代
                    int width = pageLevel.Tile_Width * xTiles;
                    int height = pageLevel.Tile_Height * yTiles;

                    targetBuf = new Byte[width * height * 3];
                    Byte[] tileBuf = new Byte[pageLevel.tile_bytesize];                          
                
                    for (int y = 0; y < yTiles; y++)
                    {
                        for (int x = 0; x < xTiles; x++)
                        {                        
                            InputTiffFile.ReadEncodedTile((xGrid + x) + (yGrid + y) * (pageLevel.Tile_XCount), tileBuf, 0, pageLevel.tile_bytesize);
                            for (int xx = 0; xx < pageLevel.Tile_Height; xx++)
                            {
                                Buffer.BlockCopy(tileBuf, xx * pageLevel.Tile_Width * 3, targetBuf,
                                    x * pageLevel.Tile_Width * 3 + (xx + y * pageLevel.Tile_Height) * width * 3, pageLevel.Tile_Width * 3);                        
                            }
                        }
                    }                
                */

                targetBuf = TargetBuf;
            }
            else
            {
                targetBuf = null;
                return;
            }
        }



        public void OverViewImageRead(Tiff InputTiffFile, out Byte[] overviewImage)
        {
            if (InputTiffFile == null)
            {
                overviewImage = null;
                return;
            }
            else
            {
                InputTiffFile.SetDirectory(3);
                int width = InputTiffFile.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = InputTiffFile.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                if (width == 1024 && height == 512)
                {
                    overviewImage = new byte[width * height * 3];         

                    for (int k = 0; k < height / 16; k++)
                    {
                        byte[] stripBuf = new byte[16 * width * 3];
                        InputTiffFile.ReadEncodedStrip(k, stripBuf, 0, 16 * width * 3);
                        int step = 16 * width * 3;
                        Buffer.BlockCopy(stripBuf, 0, overviewImage, k * step, step);
                    }
                }
                else
                    overviewImage = null;
            }
        }

        public void LabelImageRead(Tiff InputTiffFile, out byte[] labelImage)
        {
            if (InputTiffFile == null)
            {
                labelImage = null;
                return;
            }
            else
            { 
                InputTiffFile.SetDirectory(4);
                int width = InputTiffFile.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = InputTiffFile.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                if (width == 512 && height == 512)
                {
                    labelImage = new byte[width * height * 3];

                    for (int k = 0; k < height / 16; k++)
                    {
                        byte[] stripBuf = new byte[16 * width * 3];
                        InputTiffFile.ReadEncodedStrip(k, stripBuf, 0, 16 * width * 3);
                        int step = 16 * width * 3;
                        Buffer.BlockCopy(stripBuf, 0, labelImage, k * step, step);
                    }
                }
                else
                    labelImage = null;
            }
        }

        public void SingeTileRead(Tiff InputTiffFile, ref Byte[] targetBuf, PageLevel pageLevel, int xGrid, int yGrid)
        {
            if (InputTiffFile != null)
            {
                InputTiffFile.SetDirectory(pageLevel.PageIndex); // page必须为short类型   
                InputTiffFile.ReadEncodedTile(xGrid + yGrid * pageLevel.Tile_XCount, targetBuf, 0, pageLevel.Tile_Width * pageLevel.Tile_Width * 3);
            }
            else
                targetBuf = null;        
        }
    }
    #endregion
    
    #region AperioTileTiffRead
    public class AperioTileTiffRead
    {
        public PageLevel[] pageLevel = new PageLevel[3];
        public ScanImageInfo scanimageinfo = new ScanImageInfo();
        
        public Byte[] TargetBuf;
        private const string AperioInfo = "Aperio Image Library";
            // "Aperio Image Library vFS90 01\r\n90272x27227 [0,100 86631x27127] (240x240) JPEG/RGB Q=30|AppMag = 40|StripeWidth = 992|ScanScope ID = SS1511CNTLR|Filename = TCGA-FU-A5XV-01A-01-TS1|Title = TCGA-FU-A5XV-01A-01-TS1|Date = 03/06/13|Time = 08:20:16|Time Zone = GMT-05:00|User = 664ac3f2-da13-4e93-9281-b0de8ef18a65|Parmset = GOG136 on O: Drive|MPP = 0.2480|Left = 23.308456|Top = 14.137936|LineCameraSkew = 0.001071|LineAreaXOffset = 0.003267|LineAreaYOffset = -0.003095|Focus Offset = 0.000000|DSR ID = resc3-dsr2|ImageID = 125562|Exposure Time = 109|Exposure Scale = 0.000001|DisplayColor = 0|OriginalWidth = 90272|OriginalHeight = 27227|ICC Profile = ScanScope v1";
        Tiff inputTiffFile;
        string ImageDescription;

   //     private int CircleNum=0;
        public AperioTileTiffRead(int ImageWidth, int ImageHeight, Tiff InputTiffFile)
        {
            ImageDescription = InputTiffFile.GetField(TiffTag.IMAGEDESCRIPTION).ToString();
            inputTiffFile = InputTiffFile;
            // PageLevel Configuration
            pageLevel[0].Tile_Height = ImageHeight;
            pageLevel[0].Tile_Width = ImageWidth;
            pageLevel[0].DownSample = 1.0;
            pageLevel[0].tile_bytesize = ImageWidth * ImageHeight * 3;
            pageLevel[0].PageIndex = 0;

            pageLevel[1].Tile_Height = ImageHeight;
            pageLevel[1].Tile_Width = ImageWidth;
            pageLevel[1].DownSample = 1.0 / 4;
            pageLevel[1].tile_bytesize = ImageWidth * ImageHeight * 3;
            pageLevel[1].PageIndex = 1;

            pageLevel[2].Tile_Height = ImageHeight;
            pageLevel[2].Tile_Width = ImageWidth;
            pageLevel[2].DownSample = 1.0 / 16;
            pageLevel[2].tile_bytesize = ImageHeight * ImageWidth * 3;
            pageLevel[2].PageIndex = 2;
            ImageDescription = inputTiffFile.GetField(TiffTag.IMAGEDESCRIPTION)[0].ToString();

            
            if (IsAperioImage())
            {

                /*
                "Aperio Image Library v10.2.41\r\n43648x34931 [0,100 41888x34831] (240x240) J2K/YUV16 Q=70|AppMag = 40|StripeWidth = 992|
                    ScanScope ID = SS1546 | Filename = 24533 | Date = 03 / 03 / 11 | Time = 08:12:02 | User = 7c4a0433 - c087 - 4578 - b25d - 607689578f53 |
                    MPP = 0.2462 | Left = 30.155552 | Top = 15.967384 | LineCameraSkew = 0.000963 | LineAreaXOffset = 0.004954 |
                    LineAreaYOffset = -0.003469 | Focus Offset = 0.000000 | DSR ID = ap1546 - dsr | ImageID = 24533 | Exposure Time = 109 |
                    Exposure Scale = 0.000001 | DisplayColor = 0 | OriginalWidth = 43648 | OriginalHeight = 34931 | ICC Profile = ScanScope v1"
                */

                ///开始分割字符串
                string[] str = ImageDescription.Split('|');
                string baseInfo = str[0];


                for (int i = 1; i < str.Length; i++)
                {
                    string attribute = str[i].Split('=')[0].Trim();
                    string value = str[i].Split('=')[1].Trim();


                    switch (attribute)
                    {
                        case "AppMag":
                            scanimageinfo.AppMag = value;  // AppMag = 40
                            break;
                        case "StripeWidth":
                            scanimageinfo.StripWidth = Convert.ToInt16(value); // StripeWidth = 992
                            break;
                        case "ScanScope ID":
                            scanimageinfo.ScanDeviceID = value; // ScanScope ID = SS1511CNTLR
                            break;
                        case "Filename":
                            scanimageinfo.FileName = value; // Filename = TCGA-FU-A5XV-01A-01-TS1
                            break;
                        case "Title":
                            scanimageinfo.Title = value; // Title = TCGA-FU-A5XV-01A-01-TS1
                            break;
                        case "Date":
                            scanimageinfo.Date = value; // Date = 03/06/13
                            break;
                        case "Time":
                            scanimageinfo.ScanTime = value; // Time = 08:20:16
                            break;
                        case "Time Zone":
                            scanimageinfo.TimeZone = value;  // Time Zone = GMT-05:00
                            break;
                        case "User":
                            scanimageinfo.User = value; // User = 664ac3f2-da13-4e93-9281-b0de8ef18a65
                            break;
                        case "Parmset":
                            scanimageinfo.Parmset = value; // Parmset = GOG136 on O: Drive
                            break;
                        case "MPP":
                            scanimageinfo.MPP = Convert.ToDouble(value); // MPP = 0.2480
                            break;
                        case "Left":
                            scanimageinfo.Left = Convert.ToDouble(value); // Left = 23.308456
                            break;
                        case "Top":
                            scanimageinfo.Top = Convert.ToDouble(value); // Top = 14.137936
                            break;
                        case "LineCameraSkew":
                            scanimageinfo.LineCameraSkew = Convert.ToDouble(value); // LineCameraSkew = 0.001071
                            break;
                        case "LineAreaXOffset":
                            scanimageinfo.LineAreaXOffset = Convert.ToDouble(value); // LineAreaXOffset = 0.003267
                            break;
                        case "LineAreaYOffset":
                            scanimageinfo.LineAreaYOffset = Convert.ToDouble(value); // LineAreaYOffset = -0.003095
                            break;
                        case "Focus Offset":
                            scanimageinfo.FocusOffset = Convert.ToDouble(value); // Focus Offset = 0.000000
                            break;
                        case "DSR ID":
                            scanimageinfo.DSR_ID = value; // DSR ID = resc3-dsr2
                            break;
                        case "ImageID":
                            scanimageinfo.ImageID = value; // ImageID = 125562
                            break;
                        case "DisplayColor":
                            scanimageinfo.DisplayColor = Convert.ToInt32(value);  // DisplayColor = 0
                            break;
                        case "Exposure Time":
                            scanimageinfo.ExposureTime = Convert.ToDouble(value); // Exposure Time = 109
                            break;
                        case "Exposure Scale":
                            scanimageinfo.ExposureScale = Convert.ToDouble(value); // Exposure Scale = 0.000001
                            break;
                        case "":
                            scanimageinfo.OriginalWidth = Convert.ToInt32(value); // OriginalWidth = 90272
                            break;
                        case "OriginalHeight":
                            scanimageinfo.OriginalHeight = Convert.ToInt32(value); // OriginalHeight = 27227                            
                            break;
                        case "ICC Profile":
                            scanimageinfo.ICCProfile = value; // ICC Profile = ScanScope v1
                            break;

                        default:
                            break;
                    }
                }

            }
    }

        public bool IsAperioImage()
        {
            //string s = ImageDescription.Substring(0, 20);
            //if (s == AperioInfo)
            //    return true;
            //else
            //    return false;

            return false;
        }


 
        public void AperioMultiTilesRead(Tiff InputTiffFile, out Byte[] targetBuf, PageLevel pageLevel, int xGrid, int yGrid, int xTiles, int yTiles)
        {
            Compression compression = (Compression)InputTiffFile.GetField(TiffTag.COMPRESSION)[0].ToInt();
            if (compression != Compression.JPEG | xGrid < 0 | yGrid < 0)
            {
                targetBuf = null;
                return;
            }

            if ((pageLevel.Tile_Width == pageLevel.Tile_Height) && (pageLevel.PageIndex <= 2))
            {
                InputTiffFile.SetDirectory(pageLevel.PageIndex); // page必须为short类型     
                InputTiffFile.ReadMultiEncodedTile(out TargetBuf, xGrid, yGrid, xTiles, yTiles, 0, pageLevel.tile_bytesize);

                /*  以下代码已被  Tiff ReadMultiEncodedTile 替代
                     int width = pageLevel.Tile_Width * xTiles;
                     int height = pageLevel.Tile_Height * yTiles;

                     targetBuf = new Byte[width * height * 3];
                     Byte[] tileBuf = new Byte[pageLevel.tile_bytesize];                          

                     for (int y = 0; y < yTiles; y++)
                     {
                         for (int x = 0; x < xTiles; x++)
                         {                        
                             InputTiffFile.ReadEncodedTile((xGrid + x) + (yGrid + y) * (pageLevel.Tile_XCount), tileBuf, 0, pageLevel.tile_bytesize);
                             for (int xx = 0; xx < pageLevel.Tile_Height; xx++)
                             {
                                 Buffer.BlockCopy(tileBuf, xx * pageLevel.Tile_Width * 3, targetBuf,
                                     x * pageLevel.Tile_Width * 3 + (xx + y * pageLevel.Tile_Height) * width * 3, pageLevel.Tile_Width * 3);                        
                             }
                         }
                     }                
                 */

                targetBuf = TargetBuf;
            }
            else
            {
                targetBuf = null;
                return;
            }
        }

        public void AperioOverViewImageRead(Tiff InputTiffFile, out Byte[] overviewImage)
        {
            if (InputTiffFile == null)
            {
                overviewImage = null;
                return;
            }
            else
            {
                InputTiffFile.SetDirectory(3);
                int width = InputTiffFile.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = InputTiffFile.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                if (width == pageLevel[0].Tile_Width*4 && height == pageLevel[0].Tile_Height*4)
                {
                    overviewImage = new byte[width * height * 3];

                    for (int k = 0; k < height / 16; k++)
                    {
                        byte[] stripBuf = new byte[16 * width * 3];
                        InputTiffFile.ReadEncodedStrip(k, stripBuf, 0, 16 * width * 3);
                        int step = 16 * width * 3;
                        Buffer.BlockCopy(stripBuf, 0, overviewImage, k * step, step);
                    }
                }
                else
                    overviewImage = null;
            }
        }

        public void AperioLabelImageRead(Tiff InputTiffFile, out byte[] labelImage)
        {
            if (InputTiffFile == null)
            {
                labelImage = null;
                return;
            }
            else
            {
                InputTiffFile.SetDirectory(4);
                int width = InputTiffFile.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = InputTiffFile.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

                if (width == 512 && height == 512)
                {
                    labelImage = new byte[width * height * 3];

                    for (int k = 0; k < height / 16; k++)
                    {
                        byte[] stripBuf = new byte[16 * width * 3];
                        InputTiffFile.ReadEncodedStrip(k, stripBuf, 0, 16 * width * 3);
                        int step = 16 * width * 3;
                        Buffer.BlockCopy(stripBuf, 0, labelImage, k * step, step);
                    }
                }
                else
                    labelImage = null;
            }
        }

        public void AperioSingeTileRead(Tiff InputTiffFile, ref Byte[] targetBuf, PageLevel pageLevel, int xGrid, int yGrid)
        {
            if (InputTiffFile != null)
            {
                InputTiffFile.SetDirectory(pageLevel.PageIndex); // page必须为short类型   
                InputTiffFile.ReadEncodedTile(xGrid + yGrid * pageLevel.Tile_XCount, targetBuf, 0, pageLevel.Tile_Width * pageLevel.Tile_Width * 3);
            }
            else
                targetBuf = null;
        }
    }
    #endregion

    #region TileTiffWrite
    public class TileTiffWrite
    {   
        public PageLevel [] pageLevel = new PageLevel[3];   

        // 构造函数
        public TileTiffWrite(int ImageWidth,int ImageHeight)
        {
            // PageLevel Configuration
            pageLevel[0].Tile_Height = ImageHeight;
            pageLevel[0].Tile_Width = ImageWidth;
            pageLevel[0].DownSample = 1.0;
            pageLevel[0].tile_bytesize = ImageWidth * ImageHeight * 3;
            pageLevel[0].PageIndex = 0;

            pageLevel[1].Tile_Height = ImageHeight;
            pageLevel[1].Tile_Width = ImageWidth;
            pageLevel[1].DownSample = 1.0 / 4;
            pageLevel[1].tile_bytesize = ImageWidth * ImageHeight * 3;
            pageLevel[1].PageIndex = 1;

            pageLevel[2].Tile_Height = ImageHeight;
            pageLevel[2].Tile_Width = ImageWidth;
            pageLevel[2].DownSample = 1.0 / 16;
            pageLevel[2].tile_bytesize = ImageHeight * ImageWidth * 3;
            pageLevel[2].PageIndex = 2;
        }

        public bool TiffTagSetup(Tiff OutputTiffFile, int pageWidth, int pageHeight, int tileWidth, int tileHeight, int pageNumber, int pageCounts)
        {
            if (OutputTiffFile != null)
            {
                if (pageWidth % 16 == 0 && pageHeight % 16 == 0 && tileWidth % 16 == 0 && tileHeight % 16 == 0
                    && tileWidth == tileHeight)
                {
                    // specify that it's a page within the multipage file
                    OutputTiffFile.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                    // specify the page number             
                    OutputTiffFile.SetField(TiffTag.PAGENUMBER, pageNumber, pageCounts);

                    OutputTiffFile.SetField(TiffTag.IMAGEWIDTH, pageWidth);
                    OutputTiffFile.SetField(TiffTag.IMAGELENGTH, pageHeight);
                    OutputTiffFile.SetField(TiffTag.BITSPERSAMPLE, 8);
                    OutputTiffFile.SetField(TiffTag.COMPRESSION, Compression.JPEG);
                    OutputTiffFile.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                    OutputTiffFile.SetField(TiffTag.IMAGEDESCRIPTION, "Fuck!");
                    OutputTiffFile.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                    OutputTiffFile.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    OutputTiffFile.SetField(TiffTag.TILEWIDTH, tileWidth);
                    OutputTiffFile.SetField(TiffTag.TILELENGTH, tileHeight);
                    OutputTiffFile.SetField(TiffTag.IMAGEDEPTH, 1);   // Aperio ImageDepth = 1; 不规范，应该为3（RGB）

                    /*
                    OutputTiffFile.SetField(TiffTag.XRESOLUTION, 100.0);
                    OutputTiffFile.SetField(TiffTag.YRESOLUTION, 100.0);
                    OutputTiffFile.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    OutputTiffFile.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);     
                    */
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else 
            {
                return false;
            }
        }

        public  int pow(int value, int number)
        {
            int result = 1;
            for (int i = 0; i < number; i++)
            {
                result *= value;
            }
            return result;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public void EntireMultiPageTileWrite(Tiff OutputTiffFile, int YNum, int pageCounts, byte[] out4096LineBuf)
        {   
            if (out4096LineBuf == null)
                return;

            GCHandle out4096LineBufDataHandle = GCHandle.Alloc(out4096LineBuf, GCHandleType.Pinned);
            IntPtr out4096LineBufPtr = out4096LineBufDataHandle.AddrOfPinnedObject();
          
            int page0_pagewidth = pageLevel[0].Page_Width;
            Mat image = new Mat(4096, page0_pagewidth, Emgu.CV.CvEnum.DepthType.Cv8U, 3, out4096LineBufPtr, page0_pagewidth * 3);       
            
            int tile_width =pageLevel[pageCounts].Tile_Width;
            int tile_height = pageLevel[pageCounts].Tile_Height;
            int tile_bytesize = pageLevel[pageCounts].Tile_Width * pageLevel[pageCounts].Tile_Height * 3;

            Mat page0_tile_image = new Mat();
            Mat page1_tile_image = new Mat();
            Mat page2_tile_image = new Mat();

       
            //----------------------------------------------------------------------------------------------------//
            // Page0 写入 16 排 Tiles
            int XCount = this.pageLevel[0].Tile_XCount;
            int YCount = this.pageLevel[0].Tile_YCount;
            int WCount = XCount * 16;
            if (YNum == 0)
            {
                TiffTagSetup(OutputTiffFile, XCount * tile_width, YCount * tile_height, tile_width, tile_height, 0, pageCounts);
                // 不要使用CreateDirectory()
            }
            OutputTiffFile.SetDirectory(0);

            for (int k = 0; k < 16; k++)
            {
                byte[][] page0_tile_buf = new byte[XCount][];
           
                for (int i = 0; i < XCount; i++)
                {           
                    page0_tile_buf[i] = new byte[tile_bytesize];
                    Rectangle rect = new Rectangle(tile_width * i, tile_height * k, tile_width, tile_height);
                    page0_tile_image = new Mat(image, rect);
                    page0_tile_image.CopyTo(page0_tile_buf[i]);                
                }

                OutputTiffFile.WriteMultiEncodedTile(YNum * WCount + k * XCount, XCount, page0_tile_buf, 0, tile_bytesize);
            }
            OutputTiffFile.WriteDirectory();

            //----------------------------------------------------------------------------------------------------//
            // 1/4 下采样, page1 写入 4 排 Tiles
            XCount = this.pageLevel[1].Tile_XCount;
            YCount = this.pageLevel[1].Tile_YCount;
            WCount = XCount * 4;
            if (YNum == 0)
            {
                TiffTagSetup(OutputTiffFile, XCount * tile_width, YCount * tile_height, tile_width, tile_height, 1, pageCounts);
            }            
            OutputTiffFile.SetDirectory(1);
                      

            Mat temp0Image = new Mat();
            Mat roi0Image = new Mat();
            for (int k = 0; k < 4; k++)
            {
                byte[][] page1_tile_buf = new byte[XCount][];
                
                for (int i = 0; i < XCount; i++)
                {             
                    page1_tile_buf[i] = new byte[tile_bytesize];
                    Rectangle rect = new Rectangle(tile_width * 4 * i, tile_height * 4 * k, tile_width * 4, tile_height * 4);
                    roi0Image = new Mat(image, rect);
                    CvInvoke.PyrDown(roi0Image, temp0Image); //1/2
                    CvInvoke.PyrDown(temp0Image, page1_tile_image); //1/4
                    page1_tile_image.CopyTo(page1_tile_buf[i]);
             
                }
             
               OutputTiffFile.WriteMultiEncodedTile(YNum * WCount + k * XCount, XCount, page1_tile_buf, 0, tile_bytesize);
            }

            temp0Image.Dispose();
            roi0Image.Dispose();
            OutputTiffFile.WriteDirectory();

            //----------------------------------------------------------------------------------------------------//
            // 1/16 下采样, page2 写入 1 排 Tiles
            XCount = this.pageLevel[2].Tile_XCount;
            YCount = this.pageLevel[2].Tile_YCount;
            WCount = XCount * 1;
            if (YNum == 0)
            {
                TiffTagSetup(OutputTiffFile, XCount * tile_width, YCount * tile_height, tile_width, tile_height, 2, pageCounts);
            }            
            OutputTiffFile.SetDirectory(2);

            Mat temp1Image = new Mat();
            Mat temp2Image = new Mat();
            Mat temp3Image = new Mat();
            Mat roi1Image = new Mat();

            byte[][] page2_tile_buf = new byte[XCount][];
            
            for (int i = 0; i < XCount; i++)
            {
                page2_tile_buf[i] = new byte[tile_bytesize];
                Rectangle rect = new Rectangle(tile_width * 16 * i, 0, tile_width * 16, tile_height * 16);
                roi1Image = new Mat(image, rect);
                CvInvoke.PyrDown(roi1Image, temp1Image); //1/2
                CvInvoke.PyrDown(temp1Image, temp2Image); //1/4
                CvInvoke.PyrDown(temp2Image, temp3Image); //1/8
                CvInvoke.PyrDown(temp3Image, page2_tile_image); //1/16
                page2_tile_image.CopyTo(page2_tile_buf[i]);             
            }
           

            OutputTiffFile.WriteMultiEncodedTile(YNum * WCount, XCount, page2_tile_buf, 0, tile_bytesize);

            temp1Image.Dispose();
            temp2Image.Dispose();
            temp3Image.Dispose();
            roi1Image.Dispose();
            OutputTiffFile.WriteDirectory();

            /*
            // ---------------------------------------------------------------------------------------------------//
            // OverView 
            int n = 0;
            Mat tempImage1 = new Mat();
            Mat tempImage2 = new Mat();
           
            tempImage1 = image;            
            while (tempImage1.Width > 1024 )
            {
                CvInvoke.PyrDown(tempImage1, tempImage2);
                tempImage1 = tempImage2;
                n++;
            }

            int count = tempImage2.Width * tempImage2.Height * 3;
            byte[] stripBuffer = new byte[count];

            if (YNum == 0)
            {
                OverViewTagSetup(OutputTiffFile); // 需要完善
            }
            OutputTiffFile.SetDirectory(3);
            tempImage2.CopyTo(page2_tile_buf);
            OutputTiffFile.WriteEncodedStrip(YNum, stripBuffer, count);

            OutputTiffFile.WriteDirectory();
            tempImage1.Dispose(); tempImage2.Dispose();
            */
            // 释放内存

            image.Dispose();          
            DeleteObject(out4096LineBufPtr);
            out4096LineBufDataHandle.Free();

        }

        public void OverViewTagSetup(Tiff OutputTiffFile)
        {
            int width=0, height=0;

            if (width == pageLevel[0].Page_Width*2 && height == pageLevel[0].Page_Height*2)
            {
                // 设置 Tiff Tag 信息
                // specify that it's a page within the multipage file
                OutputTiffFile.SetField(TiffTag.SUBFILETYPE, FileType.REDUCEDIMAGE);
                // specify the page number             
                OutputTiffFile.SetField(TiffTag.PAGENUMBER, 3, 5);
                OutputTiffFile.SetField(TiffTag.IMAGEWIDTH, width);
                OutputTiffFile.SetField(TiffTag.IMAGELENGTH, height);
                OutputTiffFile.SetField(TiffTag.BITSPERSAMPLE, 8);
                OutputTiffFile.SetField(TiffTag.COMPRESSION, Compression.JPEG);
                OutputTiffFile.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                OutputTiffFile.SetField(TiffTag.IMAGEDESCRIPTION, "Fuck!");
                OutputTiffFile.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                OutputTiffFile.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                OutputTiffFile.SetField(TiffTag.ROWSPERSTRIP, 16);
                OutputTiffFile.SetField(TiffTag.IMAGEDEPTH, 1);

                OutputTiffFile.SetDirectory(3);
            }
        }

        public void OverViewImageWrite(Tiff OutputTiffFile, Tiff overviewImage)
        {
            if (overviewImage == null)
                return;

            int width = overviewImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = overviewImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            if (width == pageLevel[0].Page_Width*4 && height == pageLevel[0].Page_Height*2)
            {
                // 设置 Tiff Tag 信息
                // specify that it's a page within the multipage file
                OutputTiffFile.SetField(TiffTag.SUBFILETYPE, FileType.REDUCEDIMAGE);
                // specify the page number             
                OutputTiffFile.SetField(TiffTag.PAGENUMBER, 3, 5);
                OutputTiffFile.SetField(TiffTag.IMAGEWIDTH, width);
                OutputTiffFile.SetField(TiffTag.IMAGELENGTH, height);
                OutputTiffFile.SetField(TiffTag.BITSPERSAMPLE, 8);
                OutputTiffFile.SetField(TiffTag.COMPRESSION, Compression.JPEG);
                OutputTiffFile.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                OutputTiffFile.SetField(TiffTag.IMAGEDESCRIPTION, "OverViewImage!");
                OutputTiffFile.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                OutputTiffFile.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                OutputTiffFile.SetField(TiffTag.ROWSPERSTRIP, 16);
                OutputTiffFile.SetField(TiffTag.IMAGEDEPTH, 1);

                OutputTiffFile.SetDirectory(3);
                byte[] overviewLineBuf = new byte[width * 3];
                for (int k = 0; k < height / 16; k++)
                {
                    byte[] stripBuf = new byte[16 * width * 3];
                    for (int i = 0; i < 16; i++)
                    {
                        overviewImage.ReadScanline(overviewLineBuf, k * 16 + i);
                        Buffer.BlockCopy(overviewLineBuf, 0, stripBuf, i * width * 3, width * 3);                       
                    }
                    OutputTiffFile.WriteEncodedStrip(k, stripBuf, 16 * width * 3);
                }
                OutputTiffFile.WriteDirectory();
            }
        }

        public void LabelImageWrite(Tiff OutputTiffFile, Tiff labelImage)
        {
            if (labelImage == null)
                return;

            int width = labelImage.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = labelImage.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            if (width == pageLevel[0].Page_Width*2 && height == pageLevel[0].Page_Height*2)
            {
                // 设置 Tiff Tag 信息
                // specify that it's a page within the multipage file
                OutputTiffFile.SetField(TiffTag.SUBFILETYPE, FileType.REDUCEDIMAGE);
                // specify the page number             
                OutputTiffFile.SetField(TiffTag.PAGENUMBER, 4, 5);

                OutputTiffFile.SetField(TiffTag.IMAGEWIDTH, width);
                OutputTiffFile.SetField(TiffTag.IMAGELENGTH, height);
                OutputTiffFile.SetField(TiffTag.BITSPERSAMPLE, 8);
                OutputTiffFile.SetField(TiffTag.COMPRESSION, Compression.LZW);
                OutputTiffFile.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                OutputTiffFile.SetField(TiffTag.IMAGEDESCRIPTION, "LabelImage!");
                OutputTiffFile.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                OutputTiffFile.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                OutputTiffFile.SetField(TiffTag.ROWSPERSTRIP, 16);
                OutputTiffFile.SetField(TiffTag.PREDICTOR, 2);
                OutputTiffFile.SetField(TiffTag.IMAGEDEPTH, 1);

                OutputTiffFile.SetDirectory(4);
                byte[] labelLineBuf = new byte[width * 3];
                for (int k = 0; k < height / 16; k++)
                {
                    byte[] stripBuf = new byte[16 * width * 3];
                    for (int i = 0; i < 16; i++)
                    {
                        labelImage.ReadScanline(labelLineBuf, k * 16 + i);
                        Buffer.BlockCopy(labelLineBuf, 0, stripBuf, i * width * 3, width * 3);                     
                    }
                    OutputTiffFile.WriteEncodedStrip(k, stripBuf, 16 * width * 3);
                }

                OutputTiffFile.WriteDirectory();
            }
        }

        // 1/2下采样 （隔点抽样）
        public Byte [] PyrDown(Byte[] originBuf, int width, int height)
         {
             if (originBuf.Length == width * height * 3)
             {
                 Byte[] downSampleBuf = new Byte[width / 2 * height / 2 * 3];
                 
                 int orgStride = width * 3;
                 int dstStride = width * 3 / 2;

                 int m = 0, n = 0;
                 for (int i = 1; i < height; i = i + 3)
                 {
                     for (int j = 1; j < width; j = j + 3)
                     {
                         int k = 3 * j;
                         downSampleBuf[m * dstStride + 3 * n + 1] = originBuf[i * orgStride + k + 1];
                         downSampleBuf[m * dstStride + 3 * n + 2] = originBuf[i * orgStride + k + 2];
                         downSampleBuf[m * dstStride + 3 * n] = originBuf[i * orgStride + k];

                         n += 1;
                     }

                     m += 1;
                     n = 0;
                 }

                 return downSampleBuf;
             }
             else
                 return null;
         }       
    }
    #endregion
}
