using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerminalTrance;

namespace WindowsService1
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer tim;
        private static string serviceName = "我的服务";
        public Service1()
        {
            InitializeComponent();

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimedEvent);
            timer.Interval = 5000;//每5秒执行一次
            timer.Enabled = true;
        }
        private void TimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            //业务逻辑代码
            MessageAdd("测试输出"+DateTime.Now.ToString("yyyyMMddHHmmss"));
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                this.tim.Enabled = true;
                this.tim.Start();
            }
            catch (Exception ex)
            {
                MessageAdd("OnStart错误：" + ex.Message);
            }
            MessageAdd(serviceName + "已成功启动!");
        }

        protected override void OnStop()
        {
            try
            {
                this.tim.Stop();
            }
            catch (Exception ex)
            {
                MessageAdd("OnStop错误：" + ex.Message);
            }
            MessageAdd(serviceName + "已停止!");
        }
        private void MessageAdd(string msg)
        {
            string path = Application.StartupPath + @"\Log";
            new FileClass().WriteFile(path,msg);
        }
        protected override void OnContinue()
        {
            this.tim.Start();
            base.OnContinue();
        }

        protected override void OnPause()
        {
            this.tim.Stop();
            base.OnPause();
        }
    }
}
