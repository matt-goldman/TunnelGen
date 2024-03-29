﻿using Microsoft.Extensions.Logging;

namespace UI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<MainPage>();

		builder.Services.AddHttpClient(App.ApiClient, (opt) =>
						opt.BaseAddress = new Uri(Constants.BaseUrl));

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
