using Azure.Core;
using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostBuilderContext, services) =>
    {
        var configuration = hostBuilderContext.Configuration;
        var hostEnvironment = hostBuilderContext.HostingEnvironment;

        // Only set up the `PublicDataDbContext` in non-integration test
        // environments. Otherwise, the connection string will be null and
        // cause the data source builder to throw a host exception.
        if (!hostEnvironment.IsIntegrationTest())
        {
            var connectionString = configuration.GetConnectionString("PublicDataDb")!;

            if (hostEnvironment.IsDevelopment())
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(
                    configuration.GetConnectionString("PublicDataDb"));

                // Set up the data source outside the `AddDbContext` action as this
                // prevents `ManyServiceProvidersCreatedWarning` warnings due to EF
                // creating over 20 `IServiceProvider` instances.
                var dbDataSource = dataSourceBuilder.Build();

                services.AddDbContext<PublicDataDbContext>(options =>
                {
                    options
                        .UseNpgsql(dbDataSource)
                        .EnableSensitiveDataLogging();
                });
            }
            else
            {
                services.AddDbContext<PublicDataDbContext>(options =>
                {
                    var sqlServerTokenProvider = new DefaultAzureCredential();
                    var accessToken = sqlServerTokenProvider.GetToken(
                        new TokenRequestContext(scopes: new[] { "https://ossrdbms-aad.database.windows.net/.default" } )).Token;

                    var connectionStringWithAccessToken =
                        connectionString.Replace("[access_token]", accessToken);

                    var dbDataSource = new NpgsqlDataSourceBuilder(connectionStringWithAccessToken).Build();

                    options.UseNpgsql(dbDataSource);
                });
            }
        }
    })
    .Build();

host.Run();
