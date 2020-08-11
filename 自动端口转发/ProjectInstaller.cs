using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace 自动端口转发
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            var svcLogInstaller = new EventLogInstaller();
            svcLogInstaller.Source = "服务";
            svcLogInstaller.Log = "自动端口转发";
            Installers.Add(svcLogInstaller);

            var netLogInstaller = new EventLogInstaller();
            netLogInstaller.Source = "网络";
            netLogInstaller.Log = "自动端口转发";
            Installers.Add(netLogInstaller);
        }
    }
}
