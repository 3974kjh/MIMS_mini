using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIMS.Mini.Foundation
{
    public interface IObserver
    {
        void Update(object sender, NotifyMsg msg);
    }
}