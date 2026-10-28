using CateringDemo.Api.Data;
using CateringDemo.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Hosts like Railway tell the app which port to listen on through PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Market prices: BLS client, in-memory cache, and a daily background refresh.
// Set BLS_API_KEY to enable; without it the app uses estimated prices only.
builder.Services.AddSingleton<MarketPriceCache>();
builder.Services.AddHttpClient<BlsClient>(client =>
{
    client.BaseAddress = new Uri("https://api.bls.gov/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddHostedService<MarketPriceRefresher>();

var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173").Split(',');
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();

app.Run();
