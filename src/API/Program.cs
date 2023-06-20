var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

#if DEBUG
var tunnelUrl = Environment.GetEnvironmentVariable("VS_TUNNEL_URL");
Console.WriteLine($"Tunnel URL: {tunnelUrl}");
string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
string filePath = Path.Combine(appDataPath, "vstunnel.txt");
File.WriteAllText(filePath, tunnelUrl);
#endif

app.Run();
