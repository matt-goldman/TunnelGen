using Shared;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace UI;

public partial class MainPage : ContentPage
{
    private readonly HttpClient _http;

    public ObservableCollection<WeatherForecast> Forecasts { get; set; } = new();
    
    public MainPage(IHttpClientFactory clientFactory)
	{
		InitializeComponent();
        _http = clientFactory.CreateClient(App.ApiClient);
        ForecastsCollection.ItemsSource = Forecasts;
        RunningIndicator.IsVisible = false;
    }

    private async void GetData_Clicked(object sender, EventArgs e)
    {
        RunningIndicator.IsVisible = true;

        var forecasts = await _http.GetFromJsonAsync<List<WeatherForecast>>("WeatherForecast");

        foreach (var forecast in forecasts)
        {
            Forecasts.Add(forecast);
            GetDataButton.IsVisible = false;
        }

        RunningIndicator.IsVisible = false;
    }
}

