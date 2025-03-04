using System.Configuration;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.AzureBlobStorage;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    
    private IHost GetSut(Func<IHostBuilder, IHostBuilder> modifyHostBuilder) => modifyHostBuilder(new HostBuilder()).BuildHost();

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
                Action startupHost = () => base.GetSutWithConfig(FullConfig,
                    """
                    {
                     "ContentApi": {
                          "Url": null
                     }
                    }
                    """);
                
                // ACT
                var actual = Record.Exception(startupHost);

                // ASSERT
                Assert.NotNull(actual);
                Assert.IsType<ConfigurationErrorsException>(actual);
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
            public void WhenConnectionStringNotConfiguredShouldThrowOnStartUp()
            {
                // ARRANGE
                Action startupHost = () => base.GetSutWithConfig(FullConfig,
                    """
                    {
                       "App": {
                            "SearchStorageConnectionString": null,
                            "SearchableDocumentsContainerName": "searchable-documents-container-name"
                       }
                    }
                    """);

                // ACT
                var actual = Record.Exception(startupHost);

                // ASSERT
                Assert.NotNull(actual);
                Assert.IsType<ConfigurationErrorsException>(actual);
            }
            
            [Fact]
            public void WhenContainerNameNotConfiguredShouldThrowOnStartup()
            {
                // ARRANGE
                Action startupHost = () => base.GetSutWithConfig(FullConfig,
                    """
                    {
                       "App": {
                            "SearchStorageConnectionString": "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=mystorageaccount;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=https://mystorageaccount.blob.core.windows.net/;FileEndpoint=https://mystorageaccount.file.core.windows.net/;QueueEndpoint=https://mystorageaccount.queue.core.windows.net/;TableEndpoint=https://mystorageaccount.table.core.windows.net/",
                            "SearchableDocumentsContainerName": null
                       }
                    }
                    """);

                // ACT
                var actual = Record.Exception(startupHost);

                // ASSERT
                Assert.NotNull(actual);
                Assert.IsType<ConfigurationErrorsException>(actual);
            }
        }
    }
}
