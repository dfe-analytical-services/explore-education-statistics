using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Clients.ContentApi;

public class ContentApiClientTests(ITestOutputHelper output)
{
    private IContentApiClient GetSut(
        HttpMessageHandler? httpMessageHandler = null,
        Action<HttpClient>? modifyHttpClient = null,
        ILogger<ContentApiClient>? logger = null
    )
    {
        var httpClient = httpMessageHandler is null ? new HttpClient() : new HttpClient(httpMessageHandler);
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        modifyHttpClient?.Invoke(httpClient);
        return new ContentApiClient(httpClient, logger ?? new NullLogger<ContentApiClient>());
    }

    protected void Print(string s) => output.WriteLine(s);

    protected void PrintAsJson<T>(T obj) =>
        output.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));

    /// <summary>
    /// Separately assert that each of the public properties of two instances of an object are equal.
    /// This provides a finer grained explanation of where two objects differ in equality.
    /// </summary>
    private void AssertAllPropertiesMatch<T>(T expected, T actual)
    {
        AssertAll(
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(propertyInfo =>
                    (Action)(() => Assert.Equal(propertyInfo.GetValue(expected), propertyInfo.GetValue(actual)))
                )
        );
        Assert.Equal(expected, actual); // Belt and braces
    }

    private void AssertAll(params IEnumerable<Action>[] assertions) =>
        Assert.All(assertions.SelectMany(a => a), assertion => assertion());

    private class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync
    ) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => sendAsync(request, cancellationToken);
    }

    public class BasicTests(ITestOutputHelper output) : ContentApiClientTests(output)
    {
        [Fact]
        public void Can_instantiate_SUT() => Assert.NotNull(GetSut());

        [Fact]
        public async Task InvalidJsonResponse_ThrowsException()
        {
            var sut = GetSut(
                httpMessageHandler: new StubHttpMessageHandler(
                    (_, _) =>
                        Task.FromResult(
                            new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent("not-valid-json", Encoding.UTF8, "application/json"),
                            }
                        )
                ),
                modifyHttpClient: httpClient => httpClient.BaseAddress = new Uri("http://localhost:5010")
            );

            var exception = await Assert.ThrowsAsync<UnableToGetPublicationInfosException>(() =>
                sut.GetAllLivePublicationInfos(CancellationToken.None)
            );
            Assert.Contains("Error deserialising response content", exception.Message);
        }
    }

    public class GetAllLivePublicationInfosTests(ITestOutputHelper output) : ContentApiClientTests(output)
    {
        [Fact]
        public async Task ValidJsonResponse_ReturnsPublicationInfos()
        {
            var sut = GetSut(
                httpMessageHandler: new StubHttpMessageHandler(
                    (_, _) =>
                        Task.FromResult(
                            new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new StringContent(
                                    """
                                    [
                                      {
                                        "publicationSlug": "publication-1",
                                        "latestPublishedRelease": {
                                          "releaseSlug": "2024"
                                        }
                                      },
                                      {
                                        "publicationSlug": "publication-2",
                                        "latestPublishedRelease": {
                                          "releaseSlug": "2025"
                                        }
                                      }
                                    ]
                                    """,
                                    Encoding.UTF8,
                                    "application/json"
                                ),
                            }
                        )
                ),
                modifyHttpClient: httpClient => httpClient.BaseAddress = new Uri("http://localhost:5010")
            );

            var result = await sut.GetAllLivePublicationInfos(CancellationToken.None);

            Assert.Collection(
                result,
                publicationInfo =>
                {
                    Assert.Equal("publication-1", publicationInfo.PublicationSlug);
                    Assert.Equal("2024", publicationInfo.LatestReleaseSlug);
                },
                publicationInfo =>
                {
                    Assert.Equal("publication-2", publicationInfo.PublicationSlug);
                    Assert.Equal("2025", publicationInfo.LatestReleaseSlug);
                }
            );
        }
    }

    public class GetDataSetDocumentKeysTests(ITestOutputHelper output) : ContentApiClientTests(output)
    {
        private IContentApiClient GetSut(
            HttpStatusCode statusCode,
            string json,
            Action<HttpRequestMessage>? onRequest
        ) =>
            GetSut(
                httpMessageHandler: new StubHttpMessageHandler(
                    (request, _) =>
                    {
                        onRequest?.Invoke(request);
                        return Task.FromResult(
                            new HttpResponseMessage(statusCode)
                            {
                                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                            }
                        );
                    }
                ),
                modifyHttpClient: httpClient => httpClient.BaseAddress = new Uri("http://localhost:5010")
            );

        [Fact]
        public async Task ValidJsonResponse_ReturnsDataSetDocumentKeys()
        {
            string? requestedPath = null;
            var sut = GetSut(
                HttpStatusCode.OK,
                """
                [
                  {
                    "publicationId": "11111111-1111-1111-1111-111111111111",
                    "releaseId": "22222222-2222-2222-2222-222222222222",
                    "fileId": "33333333-3333-3333-3333-333333333333"
                  },
                  {
                    "publicationId": "44444444-4444-4444-4444-444444444444",
                    "releaseId": "55555555-5555-5555-5555-555555555555",
                    "fileId": "66666666-6666-6666-6666-666666666666"
                  }
                ]
                """,
                onRequest: request => requestedPath = request.RequestUri?.AbsolutePath
            );

            var result = await sut.GetDataSetDocumentKeys(CancellationToken.None);

            Assert.Equal("/api/data-sets/searchable/keys", requestedPath);
            Assert.Equal(
                [
                    new DataSetDocumentKey
                    {
                        PublicationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        ReleaseId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        FileId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    },
                    new DataSetDocumentKey
                    {
                        PublicationId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                        ReleaseId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                        FileId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    },
                ],
                result
            );
        }

        [Fact]
        public async Task EmptyResponse_ReturnsNoDataSetDocumentKeys()
        {
            var sut = GetSut(HttpStatusCode.OK, "[]", onRequest: null);

            var result = await sut.GetDataSetDocumentKeys(CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task UnsuccessfulResponse_ThrowsException()
        {
            var sut = GetSut(HttpStatusCode.InternalServerError, "", onRequest: null);

            await Assert.ThrowsAsync<UnableToGetDataSetDocumentKeysException>(() =>
                sut.GetDataSetDocumentKeys(CancellationToken.None)
            );
        }
    }

    public class DataSetDocumentsTests(ITestOutputHelper output) : ContentApiClientTests(output)
    {
        private const string DataSetDocumentsJson = """
            [
              {
                "fileId": "3d6cf2b9-6b0a-4b0e-9d6f-0f5b7c1f6a01",
                "dataSetFileId": "5f1f4d0e-2c57-4a4e-8a52-7f0d5b9f3c02",
                "subjectId": "9a7b2c1d-4e3f-4a5b-8c6d-1e2f3a4b5c03",
                "filename": "data-file.csv",
                "fileExtension": "csv",
                "fileSize": "10 Kb",
                "title": "Data Set Title",
                "content": "This is a data set summary.",
                "themeId": "498e449b-d093-43dc-959c-bc3a2c23fdfb",
                "themeTitle": "Theme Title",
                "publicationId": "8db69a42-ef47-441b-bed5-5e91d012194f",
                "publicationTitle": "Publication Title",
                "publicationSlug": "publication-slug",
                "releaseId": "08228000-71de-4a1a-9adf-35f3d2bd061a",
                "releaseVersionId": "a20f1068-4298-495d-8d80-c662ca72731d",
                "releaseTitle": "Academic year 2024/25",
                "releaseSlug": "2024-25",
                "releaseType": "OfficialStatistics",
                "latestData": true,
                "isSuperseded": false,
                "published": "2025-02-21T09:30:00+00:00",
                "lastUpdated": "2025-03-14T10:15:00+00:00",
                "api": { "id": "0d1e2f3a-4b5c-4d6e-8f70-8192a3b4c504", "version": "1.2" },
                "numDataFileRows": 1234,
                "geographicLevels": [
                  { "code": "LA", "label": "Local authority", "csvOnly": false },
                  { "code": "NAT", "label": "National", "csvOnly": true }
                ],
                "timePeriodRange": { "from": "2020/21", "to": "2024/25" },
                "filters": ["Filter 1", "Filter 2"],
                "indicators": ["Indicator 1", "Indicator 2"],
                "publishingOrganisations": [
                  { "id": "7cbcfe03-9f7e-478a-8512-a1a5e0ca793b", "title": "Department for Education" },
                  { "id": "7c3252c6-6e94-4a34-8762-ff52aae5c0c0", "title": "Ofsted" }
                ]
              }
            ]
            """;

        private static readonly DataSetDocument ExpectedDataSetDocument = new DataSetDocumentBuilder()
            .WithApi(new DataSetDocumentApi { Id = new Guid("0d1e2f3a-4b5c-4d6e-8f70-8192a3b4c504"), Version = "1.2" })
            .Build();

        private IContentApiClient GetSut(
            HttpStatusCode statusCode,
            string json,
            Action<HttpRequestMessage>? onRequest = null,
            ILogger<ContentApiClient>? logger = null
        ) =>
            GetSut(
                httpMessageHandler: new StubHttpMessageHandler(
                    (request, _) =>
                    {
                        onRequest?.Invoke(request);
                        return Task.FromResult(
                            new HttpResponseMessage(statusCode)
                            {
                                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                            }
                        );
                    }
                ),
                modifyHttpClient: httpClient => httpClient.BaseAddress = new Uri("http://localhost:5010"),
                logger: logger
            );

        public class GetReleaseDataSetDocumentsTests(ITestOutputHelper output) : DataSetDocumentsTests(output)
        {
            [Fact]
            public async Task ValidJsonResponse_ReturnsDataSetDocuments()
            {
                string? requestedPath = null;
                var sut = GetSut(
                    HttpStatusCode.OK,
                    DataSetDocumentsJson,
                    onRequest: request => requestedPath = request.RequestUri?.AbsolutePath
                );

                var result = await sut.GetReleaseDataSetDocuments("publication-slug", "2024-25");

                Assert.Equal("/api/publications/publication-slug/releases/2024-25/data-sets/searchable", requestedPath);
                Assert.Equal([ExpectedDataSetDocument], result);
            }

            [Fact]
            public async Task DataSetWithoutApi_ReturnsDataSetDocumentWithoutApi()
            {
                var json = JsonNode.Parse(DataSetDocumentsJson)!;
                json[0]!["api"] = null;
                var sut = GetSut(HttpStatusCode.OK, json.ToJsonString());

                var result = await sut.GetReleaseDataSetDocuments("publication-slug", "2024-25");

                Assert.Equal([new DataSetDocumentBuilder().Build()], result);
            }

            [Fact]
            public async Task EmptyResponse_ReturnsNoDataSetDocuments()
            {
                var sut = GetSut(HttpStatusCode.OK, "[]");

                var result = await sut.GetReleaseDataSetDocuments("publication-slug", "2024-25");

                Assert.Empty(result);
            }

            [Fact]
            public async Task NotFoundResponse_ThrowsExceptionAndLogsWarning()
            {
                var loggerMockBuilder = new LoggerMockBuilder<ContentApiClient>();
                var sut = GetSut(HttpStatusCode.NotFound, "", logger: loggerMockBuilder.Build());

                await Assert.ThrowsAsync<UnableToGetReleaseDataSetDocumentsException>(() =>
                    sut.GetReleaseDataSetDocuments("publication-slug", "2024-25")
                );

                loggerMockBuilder.Assert.LoggedWarningContains(
                    """Release "2024-25" of publication "publication-slug" was not found by the Content API"""
                );
            }

            [Fact]
            public async Task UnsuccessfulResponse_ThrowsException()
            {
                var sut = GetSut(HttpStatusCode.InternalServerError, "");

                await Assert.ThrowsAsync<UnableToGetReleaseDataSetDocumentsException>(() =>
                    sut.GetReleaseDataSetDocuments("publication-slug", "2024-25")
                );
            }

            [Fact]
            public async Task ResponseMissingField_ThrowsException()
            {
                var json = JsonNode.Parse(DataSetDocumentsJson)!;
                json[0]!.AsObject().Remove("fileId");
                var sut = GetSut(HttpStatusCode.OK, json.ToJsonString());

                await Assert.ThrowsAsync<UnableToGetReleaseDataSetDocumentsException>(() =>
                    sut.GetReleaseDataSetDocuments("publication-slug", "2024-25")
                );
            }
        }

        public class GetPublicationDataSetDocumentsTests(ITestOutputHelper output) : DataSetDocumentsTests(output)
        {
            [Fact]
            public async Task ValidJsonResponse_ReturnsDataSetDocuments()
            {
                string? requestedPath = null;
                var sut = GetSut(
                    HttpStatusCode.OK,
                    DataSetDocumentsJson,
                    onRequest: request => requestedPath = request.RequestUri?.AbsolutePath
                );

                var result = await sut.GetPublicationDataSetDocuments("publication-slug");

                Assert.Equal("/api/publications/publication-slug/data-sets/searchable", requestedPath);
                Assert.Equal([ExpectedDataSetDocument], result);
            }

            [Fact]
            public async Task EmptyResponse_ReturnsNoDataSetDocuments()
            {
                var sut = GetSut(HttpStatusCode.OK, "[]");

                var result = await sut.GetPublicationDataSetDocuments("publication-slug");

                Assert.Empty(result);
            }

            [Fact]
            public async Task NotFoundResponse_ThrowsExceptionAndLogsWarning()
            {
                var loggerMockBuilder = new LoggerMockBuilder<ContentApiClient>();
                var sut = GetSut(HttpStatusCode.NotFound, "", logger: loggerMockBuilder.Build());

                await Assert.ThrowsAsync<UnableToGetPublicationDataSetDocumentsException>(() =>
                    sut.GetPublicationDataSetDocuments("publication-slug")
                );

                loggerMockBuilder.Assert.LoggedWarningContains(
                    """Publication "publication-slug" was not found by the Content API"""
                );
            }

            [Fact]
            public async Task UnsuccessfulResponse_ThrowsException()
            {
                var sut = GetSut(HttpStatusCode.InternalServerError, "");

                await Assert.ThrowsAsync<UnableToGetPublicationDataSetDocumentsException>(() =>
                    sut.GetPublicationDataSetDocuments("publication-slug")
                );
            }
        }
    }

    public abstract class LocalDevelopmentIntegrationTests(ITestOutputHelper output) : ContentApiClientTests(output)
    {
        /// <summary>
        /// Ensure ContentAPI is running locally on port 5010
        /// </summary>
        public class CallLocalService(ITestOutputHelper output) : LocalDevelopmentIntegrationTests(output)
        {
            private IContentApiClient GetSut() =>
                base.GetSut(modifyHttpClient: httpClient => httpClient.BaseAddress = new Uri("http://localhost:5010"));

            [Fact(Skip = "This test is only for local development")]
            public async Task GetExampleSeedDocument()
            {
                // ARRANGE
                var sut = GetSut();
                const string publicationSlug = "seed-publication-permanent-fixed-period-exclusions-in-england";

                // ACT
                var actual = await sut.GetPublicationLatestReleaseDocument(publicationSlug);

                // ASSERT
                Assert.NotNull(actual);
                var expected = new ReleaseDocument
                {
                    ReleaseId = new Guid("2ae1f7b1-0088-4fd1-c78a-08dec542c3bb"),
                    ReleaseVersionId = new Guid("90f5ae5d-0eec-4029-f805-08dec542c394"),
                    PublicationId = new Guid("a5dd5b19-db74-4fa3-8eed-08dec542afe3"),
                    ThemeId = new Guid("04cadfe8-acac-4203-a6d6-08dec542a06f"),
                    ThemeTitle = "Seed theme - Pupils and schools",
                    Published = DateTimeOffset.Parse("2020-03-26T09:30:00Z"),
                    PublicationTitle = "Seed publication - Permanent & fixed-period exclusions in England",
                    Summary = "Seed publication - Permanent & fixed-period exclusions in England summary",
                    ReleaseType = "OfficialStatistics",
                    TypeBoost = 5,
                    PublicationSlug = "seed-publication-permanent-fixed-period-exclusions-in-england",
                    ReleaseSlug = "2016-17",
                    PublishingOrganisations = [],
                    HtmlContent =
                        "<html>\n    <head>\n        <title>Seed publication - Permanent & fixed-period exclusions in England</title>\n    </head>\n    <body>\n<h1>Seed publication - Permanent & fixed-period exclusions in England</h1>\n<h2>Academic year 2016/17</h2>\n<h3>Summary</h3>\nRead national statistical summaries, view charts and tables and download data files.\n<h3>Headlines</h3>\n<p>The rate of permanent exclusions has increased since last year from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17.</p>\n<h3>About this release</h3>\n<p>The statistics and data cover permanent and fixed period exclusions and school-level exclusions during the 2016/17 academic year in the following state-funded school types as reported in the school census.</p>\n<h3>Permanent exclusions</h3>\n<p>The number of permanent exclusions has increased across all state-funded primary, secondary and special schools to 7,720 - up from 6,685 in 2015/16.</p>\n<h3>Fixed-period exclusions</h3>\n<p>The number of fixed-period exclusions has increased across all state-funded primary, secondary and special schools to 381,865 - up from 339,360 in 2015/16.</p>\n<h3>Number and length of fixed-period exclusions</h3>\n<p>The number of pupils with one or more fixed-period exclusion has increased across state-funded primary, secondary and special schools to 183,475 (2.29% of pupils) up from 167,125 (2.11% of pupils) in 2015/16.</p>\n<h3>Reasons for exclusions</h3>\n<p>All reasons (except bullying and theft) saw an increase in permanent exclusions since 2015/16.</p>\n<h3>Exclusions by pupil characteristics</h3>\n<p>There was a similar pattern to previous years where the following groups (where higher exclusion rates are expected) showed an increase in exclusions since 2015/16.</p>\n<h3>Independent exclusion reviews</h3>\n<p>There were 560 reviews lodged with independent review panels in maintained primary, secondary and special schools and academies of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement.</p>\n<h3>Pupil referral units exclusions</h3>\n<p>The permanent exclusion rate in pupil referral units decreased to 0.13 - down from 0.14% in 2015/16.</p>\n<h3>Regional and local authority (LA) breakdown</h3>\n<p>There's considerable variation in the permanent exclusion and fixed-period exclusion rate at the LA level.</p>\n    </body>\n</html>\n",
                };
                AssertAllPropertiesMatch(expected, actual);
            }
        }

        public class CallUnknownService(ITestOutputHelper output) : LocalDevelopmentIntegrationTests(output)
        {
            private IContentApiClient GetSut() =>
                base.GetSut(modifyHttpClient: httpClient => httpClient.BaseAddress = new Uri("http://localhost:8123")); // Cause a 404

            [Fact(Skip = "This test is only for local development")]
            public async Task UnknownEndpointShouldThrow()
            {
                // ARRANGE
                var sut = GetSut();
                const string publicationSlug = "seed-publication-permanent-fixed-period-exclusions-in-england";

                // ACT
                var exception = await Record.ExceptionAsync(() =>
                    sut.GetPublicationLatestReleaseDocument(publicationSlug)
                );

                // ASSERT
                Assert.NotNull(exception);
                var unableToGetPublicationLatestReleaseDocumentException =
                    Assert.IsType<UnableToGetPublicationLatestReleaseDocumentException>(exception);
                Assert.Contains(publicationSlug, unableToGetPublicationLatestReleaseDocumentException.Message);
            }
        }
    }

    public class IntegrationTests(ITestOutputHelper output) : ContentApiClientTests(output)
    {
        private const string ContentApiBaseAddress = "-- insert Content API base address here --";

        private IContentApiClient GetSut() =>
            base.GetSut(modifyHttpClient: httpClient =>
            {
                httpClient.BaseAddress = new Uri(ContentApiBaseAddress);
            });

        [Fact(Skip = "Call Content API to get publications for a specified theme id")]
        public async Task GetPublicationsForTheme()
        {
            var sut = GetSut();
            var themeId = Guid.Parse("a4a32a06-c3ad-4103-faf4-08dab1d50de2");
            var publicationsForTheme = await sut.GetPublicationsForTheme(themeId);
            foreach (var publicationInfo in publicationsForTheme)
            {
                PrintAsJson(publicationInfo);
            }
        }

        [Fact(Skip = "Call Content API to get published release id's for a specified publication slug")]
        public async Task GetPublicationReleaseIds()
        {
            var sut = GetSut();
            const string publicationSlug = "seed-publication-pupil-absence-in-schools-in-england";
            var releaseIds = await sut.GetPublicationReleaseIds(publicationSlug);
            Print($"{releaseIds.Length} release id's found");
            foreach (var releaseId in releaseIds)
            {
                Print(releaseId.ToString());
            }
        }

        [Fact(Skip = "Call Content API to get all publication infos")]
        public async Task GetAllPublicationInfos()
        {
            var sut = GetSut();
            var publicationInfos = await sut.GetAllLivePublicationInfos();
            Print($"{publicationInfos.Length} publications found");
            foreach (var publicationInfo in publicationInfos)
            {
                Print(publicationInfo.ToString());
            }
        }

        [Fact(
            Skip = "Call Content API to get the latest published release version summary for the specified publication slug and release slug"
        )]
        public async Task GetReleaseVersionSummary()
        {
            var sut = GetSut();
            const string publicationSlug = "seed-publication-pupil-absence-in-schools-in-england";
            const string releaseSlug = "2016-17";
            var releaseVersionSummary = await sut.GetReleaseVersionSummary(publicationSlug, releaseSlug);
            PrintAsJson(releaseVersionSummary);
        }

        [Fact(Skip = "Ping Content API")]
        public async Task Ping()
        {
            var sut = GetSut();
            var result = await sut.Ping();
            Print($"Success: {result.WasSuccesssful}");
            Print($"ErrorMessage: {result.ErrorMessage ?? "<null>"}");
        }

        [Fact(Skip = "Call Content API to get a release document")]
        public async Task GetReleaseDocument()
        {
            // ARRANGE
            var sut = GetSut();
            const string publicationSlug = "seed-publication-permanent-fixed-period-exclusions-in-england";

            // ACT
            var result = await sut.GetPublicationLatestReleaseDocument(publicationSlug);

            // ASSERT
            PrintAsJson(result);
        }
    }
}
