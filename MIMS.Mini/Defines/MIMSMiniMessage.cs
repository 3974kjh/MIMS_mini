﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIMS.Mini.Defines
{
    public class MIMSMiniMessage
    {
        public enum NOTIFY_MESSAGE : int
        {
            /// <summary>
            /// 환자를 선택한다.
            /// </summary>
            SelectPatient,

            NewPatient,

            DeletePatient,

            InsertOrEditPatient
        }        
    }
}
