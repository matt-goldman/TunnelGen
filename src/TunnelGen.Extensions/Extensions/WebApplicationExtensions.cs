using Microsoft.AspNetCore.Builder;

namespace TunnelGen.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication SetTunnelUrl(this WebApplication app, string tunnelName)
    {
#if DEBUG
        var tunnelUrl = Environment.GetEnvironmentVariable("VS_TUNNEL_URL");
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string filePath = Path.Combine(appDataPath, $".{tunnelName}", "vstunnel.txt");
        File.WriteAllText(filePath, tunnelUrl);
#endif
        return app;
    }
}
