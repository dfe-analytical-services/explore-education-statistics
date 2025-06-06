﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace GovUk.Education.ExploreEducationStatistics.Admin;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, builder) =>
            {
                if (!ctx.HostingEnvironment.IsDevelopment())
                {
                    return;
                }

                // Can specify `IdpConfig` variable if you want to choose between a custom config
                // and Keycloak, but otherwise, just delete appsettings.Idp.json if you don't need it.
                var idpConfig = Environment.GetEnvironmentVariable("IdpConfig") ?? "Idp";

                var idpFile = builder.GetFileProvider().GetFileInfo($"appsettings.{idpConfig}.json").Exists
                    ? $"appsettings.{idpConfig}.json"
                    : "appsettings.Keycloak.json";

                builder.AddJsonFile(
                    idpFile,
                    optional: false,
                    reloadOnChange: false
                );

                var bootstrapUsers = Environment.GetEnvironmentVariable("BootstrapUsers");

                if (bootstrapUsers is not null)
                {
                    builder.AddJsonFile(
                        $"appsettings.{bootstrapUsers}BootstrapUsers.json",
                        optional: true,
                        reloadOnChange: false
                    );
                }

                builder.AddJsonFile(
                    "appsettings.Local.json",
                    optional: true,
                    reloadOnChange: false);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.AddServerHeader = false;
                });
            })
            .ConfigureLogging(
                builder =>
                {
                    // Capture logs from early in the application startup
                    // pipeline from Startup.cs or Program.cs itself.
                    builder.AddApplicationInsights();

                    // Adding the filter below to ensure logs of all severity from Program.cs
                    // is sent to ApplicationInsights.
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Program).FullName, LogLevel.Debug);

                    // Adding the filter below to ensure logs of all severity from Startup.cs
                    // is sent to ApplicationInsights.
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Startup).FullName, LogLevel.Debug);

                    // Allow capturing logs in the App Service if turned on in the App Service logs settings page.
                    builder.AddAzureWebAppDiagnostics();
                }
            );
}
