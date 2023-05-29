using Microsoft.Build.Framework;

namespace TunnelGen.Extensions.Tasks;

public class ReadTunnelTask : ITask
{
    public IBuildEngine BuildEngine { get; set; }
    public ITaskHost HostObject { get; set; }
    
    public string? TunnelName { get; set; }

    [Output]
    public string TunnelUrl { get; set; }

    public bool Execute()
    {
        TunnelName = string.IsNullOrWhiteSpace(TunnelName) ? "defaultunnel" : TunnelName;

        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string filePath = Path.Combine(appDataPath, $".{TunnelName}", "vstunnel.txt");

        try
        {
            string value = File.ReadAllText(filePath);
            TunnelUrl = value;
        }
        catch (Exception e)
        {
            BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, $"Error reading tunnel file: {e.Message}", "", "ReadTunnelTask"));
            return false;
        }

        return true;
    }

}
