using Microsoft.Win32;
using MIMS.Mini.Defines;
using MIMS.Mini.Foundation;
using MIMS.Mini.Infrastructure;
using MIMS.Mini.Model;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static MIMS.Mini.Defines.MIMSMiniMessage;

namespace MIMS.Mini
{
    public class ImageListViewModel : ViewModelBase
    {
        private PatientInfoModel _patientInfo;
        private ObservableCollection<ImageInfoModel> _imageInfoList;
        private MIMSClientEngine _engine;
        private ImageInfoModel _selectedImage;
        private List<ImageInfoModel> _saveEntireImageList;
        private MIMSMiniMODALITYDefines.IMAGE_MODALITY _state = 0;
        private int imageCategory = 0;
        private bool _isDescendingOrder;
        private bool _isXray;
        private bool _isCt;
        private bool _isMri;

        public ImageListViewModel(MIMSClientEngine engine)
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
        public ICommand GetFileCommand
        {
            get
            {
                return new DelegateCommand(this.GetPatientImage);
            }
        }

        public ICommand DeleteOneCommand
        {
            get
            {
                return new DelegateCommand(this.DeleteOnePatientImageInfo);
            }
        }

        public ICommand DescendingOrderCommand
        {
            get
            {
                return new DelegateCommand(this.DescendingOrderToPatientImage);
            }
        }

        public ICommand AscendingOrderCommand
        {
            get
            {
                return new DelegateCommand(this.AscendingOrderToPatientImage);
            }
        }

        public ICommand TotalCommand
        {
            get
            {
                return new DelegateCommand(this.TotalImageLIst);
            }
        }

        public ICommand XrayCommand
        {
            get
            {
                return new DelegateCommand(this.XrayImageLIst);
            }
        }

        public ICommand CtCommand
        {
            get
            {
                return new DelegateCommand(this.CtImageLIst);
            }
        }

        public ICommand MriCommand
        {
            get
            {
                return new DelegateCommand(this.MriImageLIst);
            }
        }
        #endregion

        #region ICommand_Function
        private void GetPatientImage(object obj)
        {
            try
            {
                if (null == _patientInfo)
                {
                    MessageBox.Show("환자 정보가 없습니다.");
                    return;
                }

                if (imageCategory < 1)
                {
                    MessageBox.Show("카테고리를 선택해 주세요.");
                    return;
                }

                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Multiselect = false;
                dialog.RestoreDirectory = true;

                if (false == dialog.ShowDialog())
                    return;
                if (false == dialog.CheckPathExists)
                    return;

                var filePath = dialog.FileName;

                CreateNewImageInfo(filePath, imageCategory);
            }
            catch (Exception ex)
            {
            }
        }

        private void DeleteOnePatientImageInfo(object obj)
        {
            var imageModel = obj as ImageInfoModel;

            if (null == imageModel)
                return;

            _selectedImage = imageModel;

            if (MessageBox.Show("환자 정보를 삭제하시겠습니까?", "환자 정보 삭제", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            if (false == _engine.DataManager.DeleteOneImageInfoLIst(_selectedImage.TimageUniqueName))
                return;

            UpdateImageList();
        }

        private void DescendingOrderToPatientImage(object obj)
        {
            if (_patientInfo == null)
                return;

            if (ImageInfoList == null)
                return;

            var descending = ImageInfoList.OrderByDescending(x => x.ImageDate);

            OwnerView.Dispatcher.BeginInvoke(new Action(() =>
            {
                ImageInfoList = new ObservableCollection<ImageInfoModel>(descending);
            }));
        }

        private void AscendingOrderToPatientImage(object obj)
        {
            if (_patientInfo == null)
                return;

            if (ImageInfoList == null)
                return;

            var accending = ImageInfoList.OrderBy(x => x.ImageDate);

            OwnerView.Dispatcher.BeginInvoke(new Action(() =>
            {
                ImageInfoList = new ObservableCollection<ImageInfoModel>(accending);
            }));
        }

        private void TotalImageLIst(object obj)
        {
            UpdatePatientImageInfoList();
        }

        private void XrayImageLIst(object obj)
        {
            UpdatePatientImageInfoList();
        }

        private void CtImageLIst(object obj)
        {
            UpdatePatientImageInfoList();
        }

        private void MriImageLIst(object obj)
        {
            UpdatePatientImageInfoList();
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
                OnPropertyChanged("SelectedImage");
            }
        }

        private void GetPatientImageInfoList()
        {
            this._saveEntireImageList = _engine.DataManager.GetTimageInfoList(_patientInfo.PatientNumber);
        }

        public override void Update(object sender, NotifyMsg msg)
        {
            if (null == msg || this == sender)
                return;

            var message = (NOTIFY_MESSAGE)msg.Message;

            switch (message)
            {
                case NOTIFY_MESSAGE.SelectPatient: // 환자 선택했을 때
                    {
                        _patientInfo = msg.MainValue as PatientInfoModel;    // 검색한 환자 정보 받아오기

                        if (null == _patientInfo)
                        {
                            _patientInfo = null;
                            return;
                        }

                        ClearAllImageLIst();
                        ClearCurImageLIst();      // 초기화 로직

                        GetPatientImageInfoList();      // DB에서 관련 이미지 데이터 가져오기
                        SetOffsetCategory();           // 카테고리 초기값설정

                        if (false == UpdatePatientImageInfoList())            // 이미지 보여주기
                            return;
                    }
                    break;
                case NOTIFY_MESSAGE.InsertOrEditPatient:
                    {
                        _patientInfo = msg.MainValue as PatientInfoModel;    // 검색한 환자 정보 받아오기

                        if (null == _patientInfo)
                        {
                            _patientInfo = null;
                            return;
                        }
                    }
                    break;
                case NOTIFY_MESSAGE.NewPatient: // new버튼 눌렀을 때
                    {
                        ClearCurImageLIst();
                        ClearAllImageLIst();
                        SetOffsetCategory();
                        _patientInfo = null;
                    }
                    break;
                case NOTIFY_MESSAGE.DeletePatient: // 휴지통버튼 눌렀을 때
                    {
                        ClearCurImageLIst();
                        ClearAllImageLIst();
                        SetOffsetCategory();
                        _patientInfo = null;
                    }
                    break;
            }
        }

        #region Property

        public bool IsDescendingOrder
        {
            get { return _isDescendingOrder; }
            set
            {
                _isDescendingOrder = value;
                OnPropertyChanged("IsDescendingOrder");
            }
        }

        public bool IsXray
        {
            get { return _isXray; }
            set
            {
                _isXray = value;
                if (_isXray == true)
                    imageCategory = 1;
                OnPropertyChanged("IsXray");
            }
        }

        public bool IsCt
        {
            get { return _isCt; }
            set
            {
                _isCt = value;
                if (_isCt == true)
                    imageCategory = 2;
                OnPropertyChanged("IsCt");
            }
        }

        public bool IsMri
        {
            get { return _isMri; }
            set
            {
                _isMri = value;
                if (_isMri == true)
                    imageCategory = 4;
                OnPropertyChanged("IsMri");
            }
        }

        public MIMSMiniMODALITYDefines.IMAGE_MODALITY State
        {
            get { return _state; }
            set
            {
                _state = value;
                OnPropertyChanged("State");
                OnPropertyChanged("Total");
                OnPropertyChanged("Xray");
                OnPropertyChanged("Ct");
                OnPropertyChanged("Mri");
            }
        }

        public bool Total
        {
            get { return State.HasFlag(MIMSMiniMODALITYDefines.IMAGE_MODALITY.TOTAL); }
            set
            {
                State = value ? MIMSMiniMODALITYDefines.IMAGE_MODALITY.TOTAL : 0;
            }
        }

        public bool Xray
        {
            get { return State.HasFlag(MIMSMiniMODALITYDefines.IMAGE_MODALITY.XRAY); }
            set
            {
                State = value ? State | MIMSMiniMODALITYDefines.IMAGE_MODALITY.XRAY : State ^ MIMSMiniMODALITYDefines.IMAGE_MODALITY.XRAY;
            }
        }

        public bool Ct
        {
            get { return State.HasFlag(MIMSMiniMODALITYDefines.IMAGE_MODALITY.CT); }
            set
            {
                State = value ? State | MIMSMiniMODALITYDefines.IMAGE_MODALITY.CT : State ^ MIMSMiniMODALITYDefines.IMAGE_MODALITY.CT;
            }
        }

        public bool Mri
        {
            get { return State.HasFlag(MIMSMiniMODALITYDefines.IMAGE_MODALITY.MRI); }
            set
            {
                State = value ? State | MIMSMiniMODALITYDefines.IMAGE_MODALITY.MRI : State ^ MIMSMiniMODALITYDefines.IMAGE_MODALITY.MRI;
            }
        }
        #endregion

        #region Function

        private void SetOffsetCategory()
        {
            State = MIMSMiniMODALITYDefines.IMAGE_MODALITY.TOTAL;
            IsDescendingOrder = true;
            IsXray = true;
        }

        private void ClearAllImageLIst()
        {
            if (null != this._saveEntireImageList)
            {
                this._saveEntireImageList.Clear();
                this._saveEntireImageList = null;
            }
        }

        private void ClearCurImageLIst()
        {
            OwnerView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (null != ImageInfoList)
                    {
                        ImageInfoList.Clear();
                        ImageInfoList = null;
                    }
                }));
        }

        private void CreateNewImageInfo(string filePath, int imageCategory)
        {
            if (false == _engine.DataManager.InsertImageInfo(filePath, imageCategory, _patientInfo))
                return;

            UpdateImageList();
        }

        private void UpdateImageList()                      // 함수명 고려
        {
            ClearAllImageLIst();                            // 기존에 담아뒀던 이미지 리스트 비워주기 ClearAllImageLIst
            GetPatientImageInfoList();                      // 새로 받아오기
            UpdatePatientImageInfoList();                   // 이미지 뿌려주기
        }

        private List<ImageInfoModel> FilteringImageInfoList()
        {
            var filteringImageList = new List<ImageInfoModel>();

            foreach (var imageInfo in _saveEntireImageList)
            {
                if (false == State.HasFlag((MIMSMiniMODALITYDefines.IMAGE_MODALITY)imageInfo.ImageCategory))
                    continue;

                filteringImageList.Add(imageInfo);
            }

            return filteringImageList;
        }

        private bool UpdatePatientImageInfoList()
        {
            if (null == _saveEntireImageList)
                return false;

            ClearCurImageLIst();
            IsDescendingOrder = true;                                            // 초기값 최신순 버튼 클릭상태

            var selectedCategoryList = FilteringImageInfoList();

            OwnerView.Dispatcher.BeginInvoke(new Action(() =>
            {
                ImageInfoList = new ObservableCollection<ImageInfoModel>(selectedCategoryList);
                
                foreach(var imageInfo in ImageInfoList)
                {
                    imageInfo.ThumbnailImage = GetImageSourceFromFilePath(imageInfo.TImagePath);
                }
            }));

            return true;
        }

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

        public void ShowImageViewer()
        {
            var view = new ImageViewer();

            view.GetImgaeLIstInfoAndSelectedImage(ImageInfoList, SelectedImage, _patientInfo);
            view.Show();
        }
        #endregion
    }
}