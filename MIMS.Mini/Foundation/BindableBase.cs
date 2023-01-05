using System;
using System.Collections.Generic;
using System.ComponentModel; // INotifyPropertyChanged
using System.Diagnostics; // []
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIMS.Mini.Foundation
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            try
            {
                this.VerifyPropertyName(propertyName);

                var eventHandler = this.PropertyChanged;
                if (eventHandler != null)
                {
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }
        public void ChangedAllProperties()
        {
            foreach (PropertyDescriptor dec in TypeDescriptor.GetProperties(this))
            {
                OnPropertyChanged(dec.Name);
            }
        }
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name : " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }
    }
}
