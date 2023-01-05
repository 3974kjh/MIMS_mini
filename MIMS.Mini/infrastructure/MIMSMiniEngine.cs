using MIMS.Mini.Infrastructure;
using MIMS.Mini.Foundation; // NotifyManager()
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MIMS.Mini.Infrastructure
{
    public class MIMSClientEngine
    {
        public MIMSClientEngine()
        {
            this.NotifyManager = new NotifyManager();
            this.DataManager = new DataManager();
        }
        public bool IsInit { get; private set; }

        public NotifyManager NotifyManager { get;  private set; }
        public DataManager DataManager { get; private set; }
        public bool Init()
        {
            this.IsInit = true;
            this.DataManager.Init();
            this.DataManager.CreateFolder();

            return this.IsInit;
        }

        public void Term()
        {
            this.IsInit = false;
        }
    }
}
