using MIMS.Mini.Foundation;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MIMS.Mini.Model
{
    public class ImageInfoModel : BindableBase, ICloneable
    {
        private ObjectId _imageID;
        private ObjectId _tImageID;
        private DateTime _imageDate;
        private string _PatientName;
        private long _PatientNumber;
        private int _imageCategory;
        private string _imageUniqueName;
        private string _tImageUniqueName;
        private string _imagePath;
        private string _tImagePath;
        private BitmapImage _originalImage;
        private BitmapImage _thumbnailImage;

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

        public string TImagePath
        {
            get { return _tImagePath; }
            set
            {
                _tImagePath = value;
                OnPropertyChanged("TImagePath");
            }
        }

        public BitmapImage OriginalImage
        {
            get { return _originalImage; }
            set
            {
                _originalImage = value;
                OnPropertyChanged("OriginalImage");
            }
        }

        public BitmapImage ThumbnailImage
        {
            get { return _thumbnailImage; }
            set
            {
                _thumbnailImage = value;
                OnPropertyChanged("ThumbnailImage");
            }
        }

        public ObjectId ImageID
        {
            get { return _imageID; }
            set
            {
                _imageID = value;
                OnPropertyChanged("ImageID");
            }
        }

        public ObjectId TimageID
        {
            get { return _tImageID; }
            set
            {
                _tImageID = value;
                OnPropertyChanged("TimageID");
            }
        }

        public DateTime ImageDate
        {
            get { return _imageDate; }
            set
            {
                _imageDate = value;
                OnPropertyChanged("ImageDate");
                OnPropertyChanged("ShowImageDate");
            }
        }        

        public string PatientName
        {
            get { return _PatientName; }
            set
            {
                _PatientName = value;
                OnPropertyChanged("PatientName");
            }
        }

        public string ShowImageDate
        {
            get 
            {
                return _imageDate.ToString("yyyy-MM-dd"); 
            }            
        }

        public long PatientNumber
        {
            get { return _PatientNumber; }
            set
            {
                _PatientNumber = value;
                OnPropertyChanged("PatientNumber");
            }            
        }        
        public int ImageCategory
        {
            get { return _imageCategory; }
            set
            {
                _imageCategory = value;
                OnPropertyChanged("ImageCategory");
            }
        }

        public string ImageUniqueName
        {
            get { return _imageUniqueName; }
            set
            {
                _imageUniqueName = value;
                OnPropertyChanged("ImageUniqueName");
            }
        }

        public string TimageUniqueName
        {
            get { return _tImageUniqueName; }
            set
            {
                _tImageUniqueName = value;
                OnPropertyChanged("TimageUniqueName");
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
