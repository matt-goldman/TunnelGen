using Microsoft.Build.Framework;
using System;
using System.IO;

namespace TunnelGen.Tasks
{

    public class ReadTunnelTask : ITask
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        public string TunnelName { get; set; }

        [Output]
        public string TunnelUrl { get; set; }

        public bool Execute()
        {
            TunnelName = string.IsNullOrWhiteSpace(TunnelName) ? "defaultunnel" : TunnelName;

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filePath = Path.Combine(appDataPath, $".{TunnelName}", "vstunnel.txt");

            try
            {
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"Attempting to get Tunnel URL from: {filePath}", "", "ReadTunnelTask", MessageImportance.High));
                string value = File.ReadAllText(filePath);
                TunnelUrl = value;
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"Tunnel URL: {value}", "", "ReadTunnelTask", MessageImportance.High));
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"Tunnel Name: {TunnelName}", "", "ReadTunnelTask", MessageImportance.High));
            }
            catch (Exception e)
            {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, $"Error reading tunnel file: {e.Message}", "", "ReadTunnelTask"));
                return false;
            }

            return true;
        }

    }
}