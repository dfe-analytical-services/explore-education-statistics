using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.CommandHandlers.ReindexSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveSearchableDocument;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests;

public class ProgramTests
{
    private const string FullConfig = """
                                        {
                                            "App": {
                                                "SearchStorageConnectionString": "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=mystorageaccount;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=https://mystorageaccount.blob.core.windows.net/;FileEndpoint=https://mystorageaccount.file.core.windows.net/;QueueEndpoint=https://mystorageaccount.queue.core.windows.net/;TableEndpoint=https://mystorageaccount.table.core.windows.net/",
                                                "SearchableDocumentsContainerName": "searchable-documents-container-name"
                                            },
                                            "ContentApi": {
                                                "Url": "http://contentapi.test.url:8123"
                                            }
                                        }
                                       """;

    private IHost GetSut() => GetSutWithConfig(FullConfig);

    private IHost GetSutWithConfig(params string[] jsonConfig) => GetSut(builder =>
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            foreach (var s in jsonConfig)
            {
                configurationBuilder.AddJsonString(s);
            }
        }));

    private IHost GetSut(Func<IHostBuilder, IHostBuilder> modifyHostBuilder) =>
        modifyHostBuilder(new HostBuilder()).BuildHost();

    public class BasicTests : ProgramTests
    {
        [Fact]
        public void Can_instantiate_Sut() => Assert.NotNull(GetSut());
    }

    public class GivenConfiguredHostTests : ProgramTests
    {
        public class HttpClient : GivenConfiguredHostTests
        {
            [Fact]
            public void ShouldBeConfigured()
            {
                // ARRANGE
                var sut = GetSut();

                // ACT
                var httpClient = sut.Services.GetRequiredService<System.Net.Http.HttpClient>();

                // ASSERT
                Assert.NotNull(httpClient);
            }
        }

        public class SearchableDocumentCreatorTests : ProgramTests
        {
            [Fact]
            public void Should_resolve_ISearchableDocumentCreator()
            {
                // ARRANGE
                var sut = GetSut();

                // ACT
                var actual = sut.Services.GetRequiredService<ISearchableDocumentCreator>();

                // ASSERT
                Assert.NotNull(actual);
                Assert.IsType<SearchableDocumentCreator>(actual);
            }
        }

        public class SearchableDocumentRemoverTests : ProgramTests
        {
            [Fact]
            public void Should_resolve_ISearchableDocumentRemover()
            {
                // ARRANGE
                var sut = GetSut();

                // ACT
                var actual = sut.Services.GetRequiredService<ISearchableDocumentRemover>();

                // ASSERT
                Assert.NotNull(actual);
                Assert.IsType<SearchableDocumentRemover>(actual);
            }
        }

        public class ClientApiTests : GivenConfiguredHostTests
        {
            [Fact]
            public void BaseAddressShouldBeConfigured()
            {
                // ARRANGE
                var sut = base.GetSutWithConfig(FullConfig,
                    """
                    {
                     "ContentApi": {
                          "Url": "http://my.test.url:123"
                     }
                    }
                    """);

                // ACT
                var actual = sut.Services.GetRequiredService<IContentApiClient>();

                // ASSERT
                Assert.NotNull(actual);
                var contentApiClient = Assert.IsType<ContentApiClient>(actual);
                Assert.Equal("http://my.test.url:123/", contentApiClient.HttpClient.BaseAddress?.ToString());
            }

            [Fact]
            public void WhenBaseAddressIsNotConfiguredShouldThrowOnStartup()
            {
                // ARRANGE
                IHost StartupHost() =>
                    base.GetSutWithConfig(FullConfig, """
                                                      {
                                                       "ContentApi": {
                                                            "Url": null
                                                       }
                                                      }
                                                      """);

                // ACT
                var actual = StartupHost();

                // ASSERT
                var options = actual.Services.GetRequiredService<IOptions<ContentApiOptions>>();
                Assert.NotNull(options);
            }
        }

        public class AzureBlobStorageClientApiTests : GivenConfiguredHostTests
        {
            [Fact]
            public void ShouldBeConfigured()
            {
                // ARRANGE
                var sut = base.GetSutWithConfig(FullConfig,
                    """
                    {
                       "App": {
                            "SearchStorageConnectionString": "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=mystorageaccount;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=https://mystorageaccount.blob.core.windows.net/;FileEndpoint=https://mystorageaccount.file.core.windows.net/;QueueEndpoint=https://mystorageaccount.queue.core.windows.net/;TableEndpoint=https://mystorageaccount.table.core.windows.net/",
                            "SearchableDocumentsContainerName": "searchable-documents-container-name"
                       }
                    }
                    """);

                // ACT
                var actual = sut.Services.GetRequiredService<IAzureBlobStorageClient>();

                // ASSERT
                Assert.NotNull(actual);
                var azureBlobStorageClient = Assert.IsType<AzureBlobStorageClient>(actual);
                var blobServiceClient = azureBlobStorageClient.BlobServiceClient;
                Assert.Equal("https://mystorageaccount.blob.core.windows.net/", blobServiceClient.Uri.ToString());
                Assert.Equal("mystorageaccount", blobServiceClient.AccountName);
            }

            [Fact]
            public void WhenConnectionStringNotConfiguredShouldNotThrowOnStartUp()
            {
                // ARRANGE
                IHost StartupHost() =>
                    base.GetSutWithConfig(FullConfig, """
                                                      {
                                                         "App": {
                                                              "SearchStorageConnectionString": null,
                                                              "SearchableDocumentsContainerName": "searchable-documents-container-name"
                                                         }
                                                      }
                                                      """);

                // ACT
                var host = StartupHost();

                // ASSERT
                var options = host.Services.GetRequiredService<IOptions<ContentApiOptions>>();
                Assert.NotNull(options);
            }

            [Fact]
            public void WhenContainerNameNotConfiguredShouldNotThrowOnStartup()
            {
                // ARRANGE
                IHost StartupHost() =>
                    base.GetSutWithConfig(FullConfig, """
                                                      {
                                                         "App": {
                                                              "SearchStorageConnectionString": "UseDevelopmentStorage=true",
                                                              "SearchableDocumentsContainerName": null
                                                         }
                                                      }
                                                      """);

                // ACT
                var host = StartupHost();

                // ASSERT
                var options = host.Services.GetRequiredService<IOptions<AppOptions>>();
                Assert.NotNull(options);
            }
        }

        public class ReindexSearchableDocumentsFunctionTests : ProgramTests
        {
            [Fact]
            public void Should_resolve_ReindexSearchableDocumentsFunction()
            {
                // ARRANGE
                var sut = GetSut();

                // ACT
                var actual = ActivatorUtilities.CreateInstance<ReindexSearchableDocumentsFunction>(sut.Services);

                // ASSERT
                Assert.NotNull(actual);
            }

            [Fact]
            public void GivenNotConfigured_WhenResolvingReindexSearchableDocumentsFunction_ThenShouldResolve()
            {
                // ARRANGE
                var sut = GetSutWithConfig("{}");

                // ACT
                var actual = sut.Services.GetRequiredService<IOptions<ReindexSearchableDocumentsFunction>>();

                // ASSERT
                Assert.NotNull(actual);
            }

            [Fact]
            public void GivenNotConfigured_WhenResolvingAzureSearchOptions_ThenShouldResolve()
            {
                // ARRANGE
                var sut = GetSutWithConfig("{}");

                // ACT
                var options = sut.Services.GetRequiredService<IOptions<AzureSearchOptions>>();

                // ASSERT
                Assert.NotNull(options.Value);
                Assert.Equal(string.Empty, options.Value.SearchServiceEndpoint);
                Assert.Null(options.Value.SearchServiceAccessKey);
                Assert.Equal(string.Empty, options.Value.IndexerName);
            }
        }

        public class ResolveFunctionTests : ProgramTests
        {
            public static TheoryData<Type> GetAzureFunctionTypes()
            {
                var types = typeof(Program)
                    .Assembly
                    .GetTypes()
                    .Where(type => type.Namespace?.StartsWith(
                            "GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Functions.") == true)
                    .Where(type => type.Name.EndsWith("Function"));

                return new TheoryData<Type>(types);
            }

            [Theory]
            [MemberData(nameof(GetAzureFunctionTypes))]
            public void Can_resolve_AzureFunction(Type functionType)
            {
                // ARRANGE
                var sut = GetSut();

                // ACT
                var actual = ActivatorUtilities.CreateInstance(sut.Services, functionType);

                // ASSERT
                Assert.NotNull(actual);
            }
        }
    }
}
