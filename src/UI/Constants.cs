using TunnelGen.Attributes;
namespace UI;

public static partial class Constants
{
    [SetTunnelUrl("WeatherForecasts")]
    private static string baseUrl = "https://myprodapi.com";

    [SetTunnelUrl("TestApp")]
    private static string testUrl = "https://mytestapi.com";
}

