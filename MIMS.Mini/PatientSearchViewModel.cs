using MIMS.Mini.Foundation;
using MIMS.Mini.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using static MIMS.Mini.Defines.MIMSMiniMessage;
using System.Windows.Input;
using MIMS.Mini.Model;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MIMS.Mini
{
    public class PatientSearchViewModel : ViewModelBase
    {
        private MIMSClientEngine _engine;
        private ObservableCollection<PatientInfoModel> _searchedPatientList;
        private PatientInfoModel _selectedPatient;
        public PatientInfoModel _patientInfo;
        private string _searchKeyword;
        private bool _isOpenSearchedPatientPopup;
        private string _filePath;
        private long _curNewPatientNumber;
        private bool _isEdit = false;

        public PatientSearchViewModel(MIMSClientEngine engine)
        {
            _engine = engine;
        }

        #region Load_UnLoad
        public override bool Load()
        {
            _engine.NotifyManager.RegObserver(this);

            RefreshPatient();

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
                return new DelegateCommand(this.GetPortraitByFileCommand);
            }
        }
        public ICommand NewCommand
        {
            get
            {
                return new DelegateCommand(this.NewPatientInfoCommand);
            }
        }
        public ICommand InsertOrEditCommand
        {
            get
            {
                return new DelegateCommand(this.InsertOrEditPatientInfoCommand);
            }
        }
        public ICommand DeleteAllCommand
        {
            get
            {
                return new DelegateCommand(this.DeleteAllPatientInfoBySelectedCommand);
            }
        }
        public ICommand SearchCommand
        {
            get
            {
                return new DelegateCommand(this.SearchedPatientInfoListCommand);
            }
        }
        #endregion

        #region ICommand_Function
        //-------------------------------------------------------------------------------------------------------------
        public void SearchedPatientInfoListCommand(object obj)
        {
            if (null != SearchedPatientList)
                SearchedPatientList.Clear();
            SearchedPatientList = null;

            var PatientInfolist = _engine.DataManager.GetPatientInfoListByPatientName(SearchKeyword);

            if (null == PatientInfolist || PatientInfolist.Count <= 0)
                return;

            SearchedPatientList = new ObservableCollection<PatientInfoModel>(PatientInfolist);
            IsOpenSearchedPatientPopup = true;
        }

        //---------------------------------------------------Info_Button--------------------------------------------------
        private void GetPortraitByFileCommand(object obj)
        {
            try
            {
                if (PatientInfo.PatientNumber == CurNewPatientNumber)
                    return;

                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Multiselect = false;
                dialog.RestoreDirectory = true;

                if (false == dialog.ShowDialog())
                    return;

                if (false == dialog.CheckPathExists)
                    return;

                FilePath = dialog.FileName;

                if (false == File.Exists(FilePath))
                {
                    MessageBox.Show("파일이 없습니다.");
                    FilePath = null;
                    return;
                }

                _patientInfo.PatientImagePath = FilePath;

                if (false == _engine.DataManager.InsertPortrait(_patientInfo))
                    return;

                _engine.DataManager.UpdatePortrait(_patientInfo.PatientNumber);
            }
            catch (Exception ex)
            {
            }
        }

        private void NewPatientInfoCommand(object obj)
        {
            _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.NewPatient));
            RefreshPatient();
        }

        private void RefreshPatient()
        {
            CreateNewPatientInfo();
            IsEdit = false;
        }

        private void InsertOrEditPatientInfoCommand(object obj)
        {
            if (null == PatientInfo)
                return;

            if (string.IsNullOrEmpty(PatientInfo.PatientName) == true || string.IsNullOrEmpty(PatientInfo.PatientResnum) == true ||
                string.IsNullOrEmpty(PatientInfo.PatientBirthday) == true || string.IsNullOrEmpty(PatientInfo.PatientPhonenum) == true)
            {
                MessageBox.Show("환자 정보를 다 작성해 주세요.");
                return;
            }

            if (false == _isEdit)
            {
                if (MessageBox.Show("환자 정보를 저장하시겠습니까?", "환자 정보 저장", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            if (true == _isEdit)
            {
                if (MessageBox.Show("환자 정보를 수정하시겠습니까?", "환자 정보 수정", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }

            if (true == this.PatientInfo.CheckPatientInfoChange)
            {
                if (true == _isEdit)
                {
                    MessageBox.Show("변경사항이 없습니다.");
                    return;
                }
                if (false == _isEdit)
                {
                    MessageBox.Show("이미 존재하는 환자입니다.");
                    return;
                }
            }

            if (false == _isEdit)
            {
                if (false == _engine.DataManager.InsertPatientInfo(_patientInfo))
                    return;

                this.CurNewPatientNumber = _engine.DataManager.GetNewPatientNumber();
                IsEdit = true;
            }

            else
            {
                if (false == _engine.DataManager.EditPatientInfo(_patientInfo.PatientNumber, _patientInfo))
                    return;
            }

            _patientInfo.CheckPatientInfoChange = true;
            _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.InsertOrEditPatient, _patientInfo));
        }


        private void DeleteAllPatientInfoBySelectedCommand(object obj)
        {
            if (null == PatientInfo)
                return;

            if (MessageBox.Show("환자 정보를 삭제하시겠습니까?", "환자 정보 삭제", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            if (false == _engine.DataManager.DeleteAllImageInfoLIst(_patientInfo.PatientNumber))
                return;

            if (false == _engine.DataManager.DeleteOnePatientInfoList(_patientInfo.PatientNumber))
                return;

            if (false == _engine.DataManager.DeleteOnePortrait(_patientInfo.PatientNumber))
                return;

            _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.DeletePatient));
            RefreshPatient();
        }
        #endregion

        public bool IsOpenSearchedPatientPopup
        {
            get { return _isOpenSearchedPatientPopup; }
            set
            {
                _isOpenSearchedPatientPopup = value;
                OnPropertyChanged("IsOpenSearchedPatientPopup");
            }
        }

        #region Property
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                _searchKeyword = value;
                OnPropertyChanged("SearchKeyword");
            }
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        public long CurNewPatientNumber
        {
            get { return _curNewPatientNumber; }
            set
            {
                _curNewPatientNumber = value;
                OnPropertyChanged("CurNewPatientNumber");
            }
        }

        public bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                _isEdit = value;
                OnPropertyChanged("IsEdit");
            }
        }
        #endregion

        public ObservableCollection<PatientInfoModel> SearchedPatientList
        {
            get { return _searchedPatientList; }
            set
            {
                _searchedPatientList = value;
                OnPropertyChanged("SearchedPatientList");
            }
        }

        public PatientInfoModel SelectedPatient
        {
            get { return _selectedPatient; }
            set
            {
                _selectedPatient = value;

                if (null == _selectedPatient)
                    return;

                PatientInfo = _selectedPatient;
                SearchKeyword = _selectedPatient.PatientName;

                IsOpenSearchedPatientPopup = false;
                var target = Clone(_patientInfo);  // _patientInfo -> 클론해서 밑에 로직 탐
                _engine.NotifyManager.Notify(this, new NotifyMsg(NOTIFY_MESSAGE.SelectPatient, target));
                _patientInfo.PatientImagePath = _engine.DataManager.GetPortraitByPatientNumber(_patientInfo.PatientNumber);
                IsEdit = true;

                OnPropertyChanged("SelectedPatient");
            }
        }

        public PatientInfoModel PatientInfo
        {
            get
            {
                if (null == _patientInfo)
                    _patientInfo = new PatientInfoModel();
                return _patientInfo;
            }
            set
            {
                _patientInfo = value;

                if (null != _patientInfo)
                {
                    _patientInfo.CheckPatientInfoChange = true;
                }

                OnPropertyChanged("PatientInfo");
            }
        }

        #region Function

        private PatientInfoModel Clone(PatientInfoModel source)
        {
            var props = source.GetType().GetProperties();
            var target = new PatientInfoModel();

            foreach (var prop in props)
            {
                if (prop.SetMethod != null)
                {
                    target.GetType().GetProperty(prop.Name).SetValue(target, prop.GetValue(source));
                }
            }
            return target;
        }

        private void CreateNewPatientInfo()
        {
            this.CurNewPatientNumber = _engine.DataManager.GetNewPatientNumber();

            this.PatientInfo = new PatientInfoModel();
            this.SearchKeyword = null;
            this.PatientInfo.PatientNumber = CurNewPatientNumber;
        }
        #endregion
    }
}