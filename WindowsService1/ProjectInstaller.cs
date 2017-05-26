using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace WindowsService1
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            
            try
            {
                 
                // 允许服务桌面交互

                System.Management.ManagementObject myService = new System.Management.ManagementObject(string.Format("Win32_Service.Name='{0}'", this.serviceInstaller1.ServiceName));

                System.Management.ManagementBaseObject changeMethod = myService.GetMethodParameters("Change");

                changeMethod["DesktopInteract"] = true;

                System.Management.ManagementBaseObject OutParam = myService.InvokeMethod("Change", changeMethod, null);

            }

            catch (Exception ex)
            {

            }

        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
