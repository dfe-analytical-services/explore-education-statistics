using System.Reflection;
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Exceptions;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Clients.ContentApi;

public class ContentApiClientTests(ITestOutputHelper output)
{
    private IContentApiClient GetSut(Action<HttpClient>? modifyHttpClient = null)
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        modifyHttpClient?.Invoke(httpClient);
        return new ContentApiClient(httpClient);
    }

    protected void Print(string s) => output.WriteLine(s);
    protected void PrintAsJson<T>(T obj) => output.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));

    /// <summary>
    /// Separately assert that each of the public properties of two instances of an object are equal.
    /// This provides a finer grained explanation of where two objects differ in equality.  
    /// </summary>
    private void AssertAllPropertiesMatch<T>(T expected, T actual)
    {
        AssertAll(typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(propertyInfo => (Action)(() => Assert.Equal(propertyInfo.GetValue(expected), propertyInfo.GetValue(actual)))));
        Assert.Equal(expected, actual); // Belt and braces
    }

    private void AssertAll(params IEnumerable<Action>[] assertions) =>
        Assert.All(assertions.SelectMany(a => a), assertion => assertion());

    public class BasicTests(ITestOutputHelper output) : ContentApiClientTests(output)
    {
        [Fact]
        public void Can_instantiate_SUT() => Assert.NotNull(GetSut());
    }

    public abstract class LocalDevelopmentIntegrationTests(ITestOutputHelper output) : ContentApiClientTests(output)
    {
        /// <summary>
        /// Ensure ContentAPI is running locally on port 5010
        /// </summary>
        public class CallLocalService(ITestOutputHelper output) : LocalDevelopmentIntegrationTests(output)
        {
            private IContentApiClient GetSut() =>
                base.GetSut(httpClient => httpClient.BaseAddress = new Uri("http://localhost:5010"));

            [Fact(Skip = "This test is only for local development")]
            public async Task GetExampleSeedDocument()
            {
                // ARRANGE
                var sut = GetSut();
                var publicationSlug = "seed-publication-permanent-and-fixed-period-exclusions-in-england";

                // ACT
                var actual = await sut.GetPublicationLatestReleaseSearchableDocument(publicationSlug);

                // ASSERT
                Assert.NotNull(actual);
                var expected = new ReleaseSearchableDocument
                {
                    ReleaseId = new Guid("4fcb18ba-2adb-49ff-9414-f91f9f5d2e57"),
                    ReleaseVersionId = new Guid("46c5d916-ee40-49bd-cfdc-08dc1c5c621e"),
                    PublicationId = new Guid("346fd6f2-3938-4006-9867-08dc1c5c66c3"),
                    ThemeId = new Guid("0396c130-4d59-4099-9050-08dc1c5c669e"),
                    ThemeTitle = "Seed theme - Pupils and schools",
                    Published = DateTimeOffset.Parse("2018-07-18T23:00:00Z"),
                    PublicationTitle = "Seed publication - Permanent and fixed-period exclusions in England",
                    Summary = "Seed publication - Permanent and fixed-period exclusions in England summary",
                    ReleaseType = "OfficialStatistics",
                    TypeBoost = 5,
                    PublicationSlug = "seed-publication-permanent-and-fixed-period-exclusions-in-england",
                    ReleaseSlug = "2016-17",
                    HtmlContent = "<html>\n    <head>\n        <title>Seed publication - Permanent and fixed-period exclusions in England</title>\n    </head>\n    <body>\n<h1>Seed publication - Permanent and fixed-period exclusions in England</h1>\n<h2>Academic year 2016/17</h2>\n<h3>Summary</h3>\n<p>Read national statistical summaries, view charts and tables and download data files.</p>\n<h3>Headlines</h3>\n<p>The rate of permanent exclusions has increased since last year from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17.</p>\n<h3>About this release</h3>\n<p>The statistics and data cover permanent and fixed period exclusions and school-level exclusions during the 2016/17 academic year in the following state-funded school types as reported in the school census.</p>\n<h3>Permanent exclusions</h3>\n<p>The number of permanent exclusions has increased across all state-funded primary, secondary and special schools to 7,720 - up from 6,685 in 2015/16.</p>\n<h3>Fixed-period exclusions</h3>\n<p>The number of fixed-period exclusions has increased across all state-funded primary, secondary and special schools to 381,865 - up from 339,360 in 2015/16.</p>\n<h3>Number and length of fixed-period exclusions</h3>\n<p>The number of pupils with one or more fixed-period exclusion has increased across state-funded primary, secondary and special schools to 183,475 (2.29% of pupils) up from 167,125 (2.11% of pupils) in 2015/16.</p>\n<h3>Reasons for exclusions</h3>\n<p>All reasons (except bullying and theft) saw an increase in permanent exclusions since 2015/16.</p>\n<h3>Exclusions by pupil characteristics</h3>\n<p>There was a similar pattern to previous years where the following groups (where higher exclusion rates are expected) showed an increase in exclusions since 2015/16.</p>\n<h3>Independent exclusion reviews</h3>\n<p>There were 560 reviews lodged with independent review panels in maintained primary, secondary and special schools and academies of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement.</p>\n<h3>Pupil referral units exclusions</h3>\n<p>The permanent exclusion rate in pupil referral units decreased to 0.13 - down from 0.14% in 2015/16.</p>\n<h3>Regional and local authority (LA) breakdown</h3>\n<p>There's considerable variation in the permanent exclusion and fixed-period exclusion rate at the LA level.</p>\n    </body>\n</html>\n"
                };
                AssertAllPropertiesMatch(expected, actual);
            }
        }

        public class CallUnknownService(ITestOutputHelper output) : LocalDevelopmentIntegrationTests(output)
        {
                private IContentApiClient GetSut() =>
                    base.GetSut(httpClient => httpClient.BaseAddress = new Uri("http://localhost:8123")); // Cause a 404

                [Fact(Skip = "This test is only for local development")]
                public async Task UnknownEndpointShouldThrow()
                {
                    // ARRANGE
                    var sut = GetSut();
                    var publicationSlug = "seed-publication-permanent-and-fixed-period-exclusions-in-england";
                    
                    // ACT
                    var exception = await Record.ExceptionAsync(() => sut.GetPublicationLatestReleaseSearchableDocument(publicationSlug));
                    
                    // ASSERT
                    Assert.NotNull(exception);
                    var unableToGetPublicationLatestReleaseSearchViewModelException = Assert.IsType<UnableToGetPublicationLatestReleaseSearchViewModelException>(exception);
                    Assert.Contains(publicationSlug, unableToGetPublicationLatestReleaseSearchViewModelException.Message);
                }
            }
    }

    public class IntegrationTests(ITestOutputHelper output) : ContentApiClientTests(output)
    {
        private const string ContentApiBaseAddress = "-- insert Content API base address here --";

        private IContentApiClient GetSut() =>
            base.GetSut(httpClient =>
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

        [Fact(Skip = "Call Content API to get releases for a specified publication slug")]
        public async Task GetReleasesForPublication()
        {
            var sut = GetSut();
            const string publicationSlug = "seed-publication-pupil-absence-in-schools-in-england";
            var releases = await sut.GetReleasesForPublication(publicationSlug);
            Print($"{releases.Length} releases found");
            foreach (var release in releases)
            {
                Print(release.ReleaseId.ToString());
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
        
        [Fact(Skip = "Call Content API to get release summary for publication and release slug")]
        public async Task GetReleaseSummary()
        {
            var sut = GetSut();
            var publicationSlug = "seed-publication-pupil-absence-in-schools-in-england";
            var releaseSlug = "2016-17";
            var releaseSummary = await sut.GetReleaseSummary(publicationSlug, releaseSlug);
            PrintAsJson(releaseSummary);
        }
        
        [Fact(Skip = "Ping Content API")]
        public async Task Ping()
        {
            var sut = GetSut();
            var result = await sut.Ping();
            Print($"Success: {result.WasSuccesssful}");
            Print($"ErrorMessage: {result.ErrorMessage ?? "<null>"}");
        }

        [Fact(Skip = "Call Content API to get a searchable document")]
        public async Task GetSearchableDocument()
        {
            // ARRANGE
            var sut = GetSut();
            var publicationSlug = "seed-publication-permanent-and-fixed-period-exclusions-in-england";

            // ACT
            var result = await sut.GetPublicationLatestReleaseSearchableDocument(publicationSlug);

            // ASSERT
            PrintAsJson(result);
        }
    }
}
