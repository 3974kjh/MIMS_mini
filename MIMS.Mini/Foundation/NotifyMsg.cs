using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIMS.Mini.Foundation
{
    public class NotifyMsg
    {
        private object _message;
        private object _mainValue;
        private object _subValue;

        public object Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public object MainValue
        {
            get { return _mainValue; }
            set { _mainValue = value; }
        }

        public object SubValue
        {
            get { return _subValue; }
            set { _subValue = value; }
        }

        public NotifyMsg(object message, object mainValue = null, object subValue = null)
        {
            _message = message;
            _mainValue = mainValue;
            _subValue = subValue;
        }
    }
}
