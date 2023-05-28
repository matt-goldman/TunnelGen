namespace UI;

public partial class App : Application
{
	public static string ApiClient = "ApiClient";
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
