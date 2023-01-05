using MIMS.Mini.Foundation;
using MIMS.Mini.Infrastructure;
using MIMS.Mini.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static MIMS.Mini.Defines.MIMSMiniMessage;
using System.Windows.Media.Imaging;
using System.IO;

namespace MIMS.Mini
{
    public class ImageViewerViewModel : ViewModelBase
    {
        private ObservableCollection<ImageInfoModel> _imageInfoList;
        private MIMSClientEngine _engine;
        private ImageInfoModel _selectedImage;
        private PatientInfoModel _patientInfo;
        private MemoryStream stream;
        private string _patientNameAndNumber;
        private double _scaleX = 1;
        private double _scaleY = 1;

        public ImageViewerViewModel(MIMSClientEngine engine)
        {
            _engine = engine;
        }

        #region Load_UnLoad
        public override bool Load()
        {
            _engine.NotifyManager.RegObserver(this);
            return true;
        }

        public override void Unload()
        {
            _engine.NotifyManager.UnregObserver(this);
        }
        #endregion

        #region ICommand
        public ICommand TurnRightCommand
        {
            get
            {
                return new DelegateCommand(TurnRightImage);
            }
        }

        public ICommand TurnLeftCommand
        {
            get
            {
                return new DelegateCommand(TurnLeftImage);
            }
        }

        public ICommand FlipImageHorizontallyCommand
        {
            get
            {
                return new DelegateCommand(FlipImageHorizontally);
            }
        }

        public ICommand FlipImageVerticallyCommand
        {
            get
            {
                return new DelegateCommand(FlipImageVertically);
            }
        }

        public ICommand ZoomInCommand
        {
            get
            {
                return new DelegateCommand(ZoomIn);
            }
        }

        public ICommand ZoomOutCommand
        {
            get
            {
                return new DelegateCommand(ZoomOut);
            }
        }

        #endregion

        #region ICommand_Function

        public void ZoomIn(object obj)
        {
            ScaleX *= 1.1;
            ScaleY *= 1.1;
        }

        public void ZoomOut(object obj)
        {
            ScaleX /= 1.1;
            ScaleY /= 1.1;
        }

        public void TurnRightImage(object obj)
        {
            if (ScaleX == -1 && ScaleY == -1)
            {
                TurnClockWise();
                return;
            }

            if (ScaleX == -1 || ScaleY == -1)
            {
                TurnAntiClockWise();
                return;
            }

            TurnClockWise();
        }

        public void TurnLeftImage(object obj)
        {
            if (ScaleX == -1 && ScaleY == -1)
            {
                TurnAntiClockWise();
                return;
            }

            if (ScaleX == -1 || ScaleY == -1)
            {
                TurnClockWise();
                return;
            }

            TurnAntiClockWise();
        }

        public void FlipImageHorizontally(object obj)
        {
            ScaleX *= -1;
        }

        public void FlipImageVertically(object obj)
        {
            ScaleY *= -1;
        }

        #endregion

        public ObservableCollection<ImageInfoModel> ImageInfoList
        {
            get { return _imageInfoList; }
            set
            {
                _imageInfoList = value;
                OnPropertyChanged("ImageInfoList");
            }
        }

        public ImageInfoModel SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                _selectedImage = value;
                _selectedImage.ImagePath = _engine.DataManager.GetImagePath(_selectedImage);
                _selectedImage.OriginalImage = GetImageSourceFromFilePath(_selectedImage.ImagePath);
                ScaleX = 1;
                ScaleY = 1;
                using (var s = File.OpenRead(_selectedImage.ImagePath))
                {
                    stream = new MemoryStream();

                    s.CopyTo(stream);
                }
                OnPropertyChanged("SelectedImage");
            }
        }

        public PatientInfoModel PatientInfo
        {
            get { return _patientInfo; }
            set
            {
                _patientInfo = value;
                PatientNameAndNumber = PatientInfo.PatientName + " " + "[" + PatientInfo.PatientNumber.ToString() + "]" + " ";
                OnPropertyChanged("PatientInfo");
            }
        }

        #region Property

        public double ScaleX
        {
            get { return _scaleX; }
            set
            {
                _scaleX = value;
                OnPropertyChanged("ScaleX");
            }
        }

        public double ScaleY
        {
            get { return _scaleY; }
            set
            {
                _scaleY = value;
                OnPropertyChanged("ScaleY");
            }
        }

        public string PatientNameAndNumber
        {
            get { return _patientNameAndNumber; }
            set
            {
                _patientNameAndNumber = value;
                OnPropertyChanged("PatientNameAndNumber");
            }
        }

        #endregion

        #region Function

        public BitmapImage GetImageSourceFromFilePath(string filePath)
        {
            using (var s = File.OpenRead(filePath))
            {
                BitmapImage image = new BitmapImage();

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = s;
                image.EndInit();
                image.Freeze();

                return image.Clone();
            }
        }

        private void TurnClockWise()
        {
            Rotation rotation = _selectedImage.OriginalImage.Rotation;
            int a = 0;

            if (rotation == Rotation.Rotate270)
            {
                rotation = Rotation.Rotate0;
                a = -1;
            }

            using (MemoryStream memory = new MemoryStream())
            {
                BitmapImage image = new BitmapImage();

                stream.Position = 0;
                stream.CopyTo(memory);
                memory.Seek(0, SeekOrigin.Begin);
                image.BeginInit();

                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memory;
                image.Rotation = rotation + 1 + a;
                image.EndInit();
                image.Freeze();
                _selectedImage.OriginalImage = image.Clone();
            }
        }

        private void TurnAntiClockWise()
        {
            Rotation rotation = _selectedImage.OriginalImage.Rotation;
            int a = 0;

            if (rotation == Rotation.Rotate0)
            {
                rotation = Rotation.Rotate270;
                a = 1;
            }

            using (MemoryStream memory = new MemoryStream())
            {
                BitmapImage image = new BitmapImage();

                stream.Position = 0;
                stream.CopyTo(memory);
                memory.Seek(0, SeekOrigin.Begin);
                image.BeginInit();

                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memory;
                image.Rotation = rotation - 1 + a;
                image.EndInit();
                image.Freeze();
                _selectedImage.OriginalImage = image.Clone();
            }
            return;
        }

        #endregion

        //public override void Update(object sender, NotifyMsg msg)
        //{
        //    if (null == msg || this == sender)
        //        return;

        //    var message = (NOTIFY_MESSAGE)msg.Message;
        //    switch (message)
        //    {
        //        case NOTIFY_MESSAGE.OpenView:
        //            {

        //            }
        //            break;
        //    }
        //}
    }
}