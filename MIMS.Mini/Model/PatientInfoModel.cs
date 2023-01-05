using MIMS.Mini.Foundation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIMS.Mini.Model
{ 
    public class PatientInfoModel : BindableBase, ICloneable
    {
        private long _patientNumber;
        private string _patientName;
        private string _patientResnum;
        private string _patientBirthday;
        private string _patientPhonenum;
        private bool _checkPatientInfoChange;
        private string _patientImagePath;

        public long PatientNumber
        {
            get { return _patientNumber; }
            set
            {
                _patientNumber = value;
                OnPropertyChanged("PatientNumber");
            }
        }
        public string PatientName
        {
            get { return _patientName; }
            set
            {
                _patientName = value;
                _checkPatientInfoChange = false;
                OnPropertyChanged("PatientName");
            }
        }
        public string PatientResnum
        {
            get { return _patientResnum; }
            set
            {
                _patientResnum = value;
                _checkPatientInfoChange = false;
                OnPropertyChanged("PatientResnum");
            }
        }
        public string PatientBirthday
        {
            get { return _patientBirthday; }
            set
            {
                _patientBirthday = value;
                _checkPatientInfoChange = false;
                OnPropertyChanged("PatientBirthday");
            }
        }
        public string PatientPhonenum
        {
            get { return _patientPhonenum; }
            set
            {
                _patientPhonenum = value;
                _checkPatientInfoChange = false;
                OnPropertyChanged("PatientPhonenum");
            }
        }
        public bool CheckPatientInfoChange
        {
            get { return _checkPatientInfoChange; }
            set
            {
                _checkPatientInfoChange = value;
            }
        }
        public string PatientImagePath
        {
            get { return _patientImagePath; }
            set
            {
                _patientImagePath = value;
                OnPropertyChanged("PatientImagePath");
            }
        }

        public ImageInfoModel ImageInfo { get; set; }

        public object Clone()
        {
            var cloned =  this.MemberwiseClone();

            if (null != this.ImageInfo)
            {
                (cloned as PatientInfoModel).ImageInfo = this.ImageInfo.Clone() as ImageInfoModel;
            }

            return cloned;
        }
    }
}
