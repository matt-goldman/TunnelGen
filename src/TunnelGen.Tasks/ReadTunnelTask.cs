using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TunnelGen.Tasks
{

    public class ReadTunnelTask : ITask
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        public string TunnelNames { get; set; }

        [Output]
        public string Tunnels { get; set; }

        public bool Execute()
        {
            var tunnelList = new List<Tunnel>();

            var tunnelNames = GetTunnelNames(TunnelNames);

            if(tunnelNames.Any())
            {
                foreach(var tunnel in tunnelNames)
                {
                    var url = GetTunnelUrl(tunnel);
                    tunnelList.Add(new Tunnel
                    {
                        TunnelName = tunnel,
                        TunnelUrl = url
                    });
                }
            }
            else
            {
                var url = GetTunnelUrl("defaulttunnel");

                if (!string.IsNullOrWhiteSpace(url))
                {
                    tunnelList.Add(new Tunnel
                    {
                        TunnelName = "defaulttunnel",
                        TunnelUrl = url
                    });
                }
            }
            
            if (tunnelList.Any())
            {
                Tunnels = SerialiseTunnels(tunnelList);
                return true;
            }
            else
            {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, $"[ReadTunnelTask] No named tunnels found and no URL for default tunnel.", "", "ReadTunnelTask"));
                return false;
            }
        }


        private List<string> GetTunnelNames(string tunnels)
        {
            var tunnelNames = new List<string>();

            var tunnelList = tunnels.Split(',').ToList();

            foreach (var tunnel in tunnelList)
            {
                if (!string.IsNullOrEmpty(tunnel))
                {
                    tunnelNames.Add(tunnel.Trim());
                }
            }

            return tunnelNames;
        }

        private string GetTunnelUrl (string tunnelName)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filePath = Path.Combine(appDataPath, $".{tunnelName}", "vstunnel.txt");

            try
            {
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"[ReadTunnelTask] Tunnel Name: {TunnelNames}", "", "ReadTunnelTask", MessageImportance.High));
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"[ReadTunnelTask] Attempting to get Tunnel URL from: {filePath}", "", "ReadTunnelTask", MessageImportance.High));
                string value = File.ReadAllText(filePath);
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"[ReadTunnelTask] Tunnel URL: {value}", "", "ReadTunnelTask", MessageImportance.High));
                return value;
            }
            catch (Exception e)
            {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, $"Error reading tunnel file: {e.Message}", "", "ReadTunnelTask"));
                return string.Empty;
            }
        }

        private string SerialiseTunnels(List<Tunnel> list)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var tunnel in list)
            {
                sb.Append($"{tunnel.TunnelName}|{tunnel.TunnelUrl},");
            }

            return sb.ToString().TrimEnd(',');
        }
    }
}