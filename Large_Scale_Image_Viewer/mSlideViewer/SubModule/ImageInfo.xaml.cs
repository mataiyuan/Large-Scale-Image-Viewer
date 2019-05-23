using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace mSlideViewer
{
    /// <summary>
    /// Interaction logic for ImageInfo.xaml
    /// </summary>
    public partial class ImageInfo : Window
    {
        public AperioTileTiffRead Info;
        public ImageInfo(AperioTileTiffRead tiffReader)
        {
            InitializeComponent();

            Info = tiffReader;
            if (Info == null)
                return;


            string s = "";
            s += "AppMag=" + Info.scanimageinfo.AppMag + "\r\n";
            s += "Magnifier=" + Info.scanimageinfo.Magnifier + "\r\n";
            s += "FileName=" + Info.scanimageinfo.FileName + "\r\n";
            s += "ScanDeviceID=" + Info.scanimageinfo.ScanDeviceID + "\r\n";
            s += "Title=" + Info.scanimageinfo.Title + "\r\n";
            s += "Date=" + Info.scanimageinfo.Date + "\r\n";
            s += "TimeZone=" + Info.scanimageinfo.TimeZone + "\r\n";
            s += "Filtered=" + Info.scanimageinfo.Filtered + "\r\n";
            s += "FocusOffset=" + Info.scanimageinfo.FocusOffset + "\r\n";
            s += "DSR_ID=" + Info.scanimageinfo.DSR_ID + "\r\n";
            s += "ImageID=" + Info.scanimageinfo.ImageID + "\r\n";
            s += "Left=" + Info.scanimageinfo.Left + "\r\n";
            s += "LineAreaXOffset=" + Info.scanimageinfo.LineAreaXOffset + "\r\n";
            s += "LineAreaYOffset=" + Info.scanimageinfo.LineAreaYOffset + "\r\n";

            s += "LineCameraSkew=" + Info.scanimageinfo.LineCameraSkew + "\r\n";
            s += "MPP=" + Info.scanimageinfo.MPP + "\r\n";
            s += "ExposureTime=" + Info.scanimageinfo.ExposureTime + "\r\n";
            s += "ExposureScale=" + Info.scanimageinfo.ExposureScale + "\r\n";
            s += "DisplayColor=" + Info.scanimageinfo.DisplayColor + "\r\n";
            s += "OriginalHeight=" + Info.scanimageinfo.OriginalHeight + "\r\n";
            s += "OriginalWidth=" + Info.scanimageinfo.OriginalWidth + "\r\n";
            s += "ICCProfile=" + Info.scanimageinfo.ICCProfile + "\r\n";
            s += "Parmset=" + Info.scanimageinfo.Parmset + "\r\n";
            s += "DeviceID=" + Info.scanimageinfo.DeviceID + "\r\n";
            s = "StripWidth=" + Info.scanimageinfo.StripWidth + "\r\n";
            s += "ScanTime=" + Info.scanimageinfo.ScanTime + "\r\n";
            s += "Top=" + Info.scanimageinfo.Top + "\r\n";
            s += "User=" + Info.scanimageinfo.User + "\r\n";
            s += "Comment=" + Info.scanimageinfo.Comment + "\r\n";
            s += "Mpp_X=" + Info.scanimageinfo.Mpp_X + "\r\n";
            s += "Mpp_Y=" + Info.scanimageinfo.Mpp_Y + "\r\n";
            s += "Objective_Power=" + Info.scanimageinfo.Objective_Power + "\r\n";
            s += "QuichHash=" + Info.scanimageinfo.QuichHash + "\r\n";
            s += "Vendor=" + Info.scanimageinfo.Vendor + "\r\n";
            s += "ImageDseciption=" + Info.scanimageinfo.ImageDseciption + "\r\n";
            s += "ResolutionUnit=" + Info.scanimageinfo.ResolutionUnit + "\r\n";
            s += "nTileCounts=" + Info.scanimageinfo.nTileCounts;

            imageInfo.Text = s;
        }
    }
}
