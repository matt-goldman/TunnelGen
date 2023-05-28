using Microsoft.Build.Framework;

namespace TunnelGen.Extensions.Tasks;

public class ReadTunnelTask : ITask
{
    public IBuildEngine BuildEngine { get; set; }
    public ITaskHost HostObject { get; set; }
    
    public string TunnelName { get; set; }

    [Output]
    public string TunnelUrl { get; set; }

    public bool Execute()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string filePath = Path.Combine(appDataPath, $".{TunnelName}", "vstunnel.txt");
        string value = File.ReadAllText(filePath);

        TunnelUrl = value;

        return true;
    }
}
