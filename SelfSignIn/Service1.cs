using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TerminalTrance;

namespace SelfSignIn
{
    public partial class SelfSignIn : ServiceBase
    {
        public SelfSignIn()
        {
            InitializeComponent();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimedEvent);
            timer.Interval = 5000;//每5秒执行一次
            timer.Enabled = true;
        }
        private void TimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            Process.Start("http://www.17sucai.com/member/signin");
            //业务逻辑代码
            MessageAdd("测试输出" + DateTime.Now.ToString("yyyyMMddHHmmss"));
        }
        private void MessageAdd(string msg)
        {
            string path = LogHelper.Path;
            LogHelper.WriteFile(path, msg);
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
