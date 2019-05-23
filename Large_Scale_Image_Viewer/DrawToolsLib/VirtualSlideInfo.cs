using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrawToolsLib
{
    [Serializable]
    [XmlRoot("VirtualSlideInfo")] 
    public class VirtualSlideInfo
    {
        public VirtualSlideInfo()
        {
            //default constructor
        }

        private String imageFileName;        
        private string authorOfAnnotation;
        private string dateOfAnnotation;
        private Int32 imageWidth;
        private Int32 imageHeight;
        private long fileSize;
        
        public String ImageFileName
        {
            get
            {
                return imageFileName;
            }
            set
            {
                imageFileName = value;
            }
        }
        
        public string AuthorOfAnnotation
        {
            get
            {
                return authorOfAnnotation;
            }
            set
            {
                authorOfAnnotation = value;
            }
        }

        public String DateOfAnnotation
        {
            get
            {
                return dateOfAnnotation;
            }
            set
            {
                dateOfAnnotation = value;
            }
        }

        public Int32 ImageWidth
        {
            get
            {
                return imageWidth;
            }
            set
            {
                imageWidth = value;
            }
        }

        public Int32 ImageHeight
        {
            get
            {
                return imageHeight;
            }
            set
            {
                imageHeight = value;
            }
        }

        public long FileSize
        {
            get
            {
                return fileSize;
            }
            set
            {
                fileSize = value;
            }
        }
        
    }
}
