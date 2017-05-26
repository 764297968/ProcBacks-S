using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerminalTrance;

namespace ProcBackups
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;

            string path = LogHelper.Path;
            LogHelper.WriteFile(path, "应用程序的主入口点");
            ServicesToRun = new ServiceBase[]
            {
                new Backup()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
