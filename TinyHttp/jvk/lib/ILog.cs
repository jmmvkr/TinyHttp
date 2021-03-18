using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jvk.lib
{

    public interface ILog
    {
        void log(string msg);
        void log(string msg, Exception ex);
    } // end - interface ILog

}
