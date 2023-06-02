using Microsoft.AspNetCore.Builder;

namespace TunnelGen.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication SetTunnelUrl(this WebApplication app, string? tunnelName)
    {
#if DEBUG
        tunnelName = string.IsNullOrWhiteSpace(tunnelName) ? "defaulttunnel" : tunnelName;

        var tunnelUrl = Environment.GetEnvironmentVariable("VS_TUNNEL_URL");
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folderPath = Path.Combine(appDataPath, $".{tunnelName}");

        DirectoryInfo di = Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "vstunnel.txt");
        File.WriteAllText(filePath, tunnelUrl);
#endif
        return app;
    }
}
