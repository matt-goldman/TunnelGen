namespace UI;
public static partial class Constants
{
	public static string BaseUrl
	{
	#if DEBUG
		get => "";
	#else
		get => baseUrl;
	#endif
	}
}