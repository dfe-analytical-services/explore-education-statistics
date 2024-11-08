using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api;
using Microsoft.Extensions.Logging.ApplicationInsights;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Configuration.AddJsonFile(
    path: "appsettings.Local.json",
    optional: true,
    reloadOnChange: false);


// Logging

builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Program).FullName, LogLevel.Debug);
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Startup).FullName, LogLevel.Debug);
builder.Logging.AddAzureWebAppDiagnostics();

var startup = new Startup(builder.Configuration, builder.Environment);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

await app.StartAsync();

app.Urls.ForEach(address => Console.WriteLine($"Server listening on address: {address}"));

await app.WaitForShutdownAsync();
