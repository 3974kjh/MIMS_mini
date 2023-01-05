using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIMS.Mini.Defines
{
    public class MIMSMiniDefines
    {
        /// <summary>
        /// MIMSClient 프로그램명
        /// </summary>
        public static readonly string PROGRAM_NAME = "MIMSClient";

        /// <summary>
        /// MIMSClient 로그 파일명
        /// </summary>
        public static readonly string LOG_FILE_NAME = "MIMSClient.log";

        /// <summary>
        /// MIMSClient 로그 폴더명
        /// </summary>
        public static readonly string LOG_FOLDER_NAME = "log";
    }

    //class
    public class MIMSMiniMODALITYDefines
    {
        public enum IMAGE_MODALITY : int
        {
            /// <summary>
            /// 다운받을 파일이 x-ray인지 ct인지 mri인지 선택한다.
            /// </summary>
            XRAY = 1,

            CT = 2,

            MRI = 4,

            TOTAL = 7,
        }
    }
}
