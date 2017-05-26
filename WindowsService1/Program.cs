using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerminalTrance;

namespace WindowsService1
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            string path = Application.StartupPath + @"\Log.txt";
            new FileClass().WriteFile(path, "测试");
            ServiceBase.Run(ServicesToRun);
          
        }
    }
}
