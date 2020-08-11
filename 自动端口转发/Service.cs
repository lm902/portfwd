using Open.Nat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace 自动端口转发
{
    public partial class Service : ServiceBase
    {
        private EventLog netLog;
        private EventLog svcLog;

        public Service()
        {
            InitializeComponent();
            netLog = new EventLog();
            netLog.Source = "网络";
            netLog.Log = "自动端口转发";
            svcLog = new EventLog();
            svcLog.Source = "服务";
            svcLog.Log = "自动端口转发";
        }

        protected override void OnStart(string[] args)
        {
            svcLog.WriteEntry("服务已启动", EventLogEntryType.Information, 0);
            Timer timer = new Timer();
            timer.Interval = 30000;
            timer.Elapsed += new ElapsedEventHandler(this.OnTimerAsync);
            timer.Start();
        }

        protected override void OnStop()
        {
            svcLog.WriteEntry("服务已停止", EventLogEntryType.Information, 1);
        }

        public async void OnTimerAsync(object sender, ElapsedEventArgs args)
        {
            svcLog.WriteEntry("扫描端口监听状态", EventLogEntryType.Information, 2);
            try
            {
                var discoverer = new NatDiscoverer();
                var device = await discoverer.DiscoverDeviceAsync();
                var ip = await device.GetExternalIPAsync();
                netLog.WriteEntry("外部IP地址是：" + ip, EventLogEntryType.Information, 3);
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                foreach (var listener in properties.GetActiveTcpListeners())
                {
                    if (await device.GetSpecificMappingAsync(Protocol.Tcp, listener.Port) == null)
                    {
                        try
                        {
                            await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, listener.Port, listener.Port, "自动端口转发 TCP " + listener.Port));
                            netLog.WriteEntry("转发端口 TCP " + listener.Port, EventLogEntryType.Information, 5);
                        } catch (MappingException e)
                        {
                            netLog.WriteEntry("转发端口 TCP " + listener.Port + " 时发生错误：" + e.Message, EventLogEntryType.Error, 6);
                        }
                    }
                    else
                    {
                        netLog.WriteEntry("端口 TCP " + listener.Port + " 的转发规则已存在。", EventLogEntryType.Information, 9);
                    }
                }
                foreach (var listener in properties.GetActiveTcpListeners())
                {
                    if (await device.GetSpecificMappingAsync(Protocol.Udp, listener.Port) == null)
                    {
                        try
                        {
                            await device.CreatePortMapAsync(new Mapping(Protocol.Udp, listener.Port, listener.Port, "自动端口转发 UDP " + listener.Port));
                            netLog.WriteEntry("转发端口 UDP " + listener.Port, EventLogEntryType.Information, 5);
                        }
                        catch (MappingException e)
                        {
                            netLog.WriteEntry("转发端口 UDP " + listener.Port + " 时发生错误：" + e.Message, EventLogEntryType.Error, 6);
                        }
                    }
                    else
                    {
                        netLog.WriteEntry("端口 UDP " + listener.Port + " 的转发规则已存在。", EventLogEntryType.Information, 9);
                    }
                }
            }
            catch (NatDeviceNotFoundException)
            {
                netLog.WriteEntry("未找到可用于端口转发的UPnP或PMP设备", EventLogEntryType.Error, 4);
            }
            catch (Exception e)
            {
                svcLog.WriteEntry("内部错误 " + e.GetType().FullName + ": " + e.Message, EventLogEntryType.Error, 7);
            }
        }
    }
}
