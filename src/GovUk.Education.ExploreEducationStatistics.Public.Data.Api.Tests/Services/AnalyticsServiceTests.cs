using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

// ReSharper disable once ClassNeverInstantiated.Global
public class AnalyticsServiceTestsFixture()
    : OptimisedPublicApiCollectionFixture(capabilities: [PublicApiIntegrationTestCapability.Postgres])
{
    public IAnalyticsService AnalyticsService = null!;
    public Mock<IAnalyticsManager> AnalyticsManagerMock = null!;
    public Mock<IHttpContextAccessor> HttpContextAccessorMock = null!;

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        serviceModifications
            .ReplaceServiceWithMock<IAnalyticsManager>()
            .ReplaceServiceWithMock<IHttpContextAccessor>()
            .ReplaceService<IAnalyticsService, AnalyticsService>(serviceLifetime: ServiceLifetime.Scoped);
    }

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups<Startup> lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        AnalyticsManagerMock = lookups.GetMockService<IAnalyticsManager>();
        HttpContextAccessorMock = lookups.GetMockService<IHttpContextAccessor>();
        AnalyticsService = lookups.GetService<IAnalyticsService>();
    }

    public override async Task BeforeEachTest()
    {
        await base.BeforeEachTest();

        AnalyticsManagerMock.Reset();
        HttpContextAccessorMock.Reset();
    }
}

[CollectionDefinition(nameof(AnalyticsServiceTestsFixture))]
public class AnalyticsServiceTestsCollection : ICollectionFixture<AnalyticsServiceTestsFixture>;

[Collection(nameof(AnalyticsServiceTestsFixture))]
public abstract class AnalyticsServiceTests(AnalyticsServiceTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class CaptureTopLevelCallTests(AnalyticsServiceTestsFixture fixture) : AnalyticsServiceTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(
                        new CaptureTopLevelCallRequest(
                            fixture.GetUtcNow(),
                            TopLevelCallType.GetPublications,
                            new PaginationParameters(1, 5)
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            await fixture.AnalyticsService.CaptureTopLevelCall(
                TopLevelCallType.GetPublications,
                new PaginationParameters(Page: 1, PageSize: 5)
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }

        [Fact]
        public async Task RequestFromEes_AnalyticsIgnored()
        {
            fixture.HttpContextAccessorMock.SetupHasHeader(RequestHeaderNames.RequestSource);

            await fixture.AnalyticsService.CaptureTopLevelCall(
                TopLevelCallType.GetPublications,
                new PaginationParameters(Page: 1, PageSize: 5)
            );

            fixture.AnalyticsManagerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ExceptionThrown_Caught()
        {
            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(It.IsAny<CaptureTopLevelCallRequest>(), It.IsAny<CancellationToken>())
                )
                .Throws(new Exception("Error"));

            await fixture.AnalyticsService.CaptureTopLevelCall(
                TopLevelCallType.GetPublications,
                new PaginationParameters(Page: 1, PageSize: 5)
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }
    }

    public class CapturePublicationCallTests(AnalyticsServiceTestsFixture fixture) : AnalyticsServiceTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var publicationId = Guid.NewGuid();
            var publicationTitle = "Publication title";

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(
                        new CapturePublicationCallRequest(
                            publicationId,
                            publicationTitle,
                            fixture.GetUtcNow(),
                            PublicationCallType.GetDataSets,
                            new PaginationParameters(2, 5)
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            await fixture.AnalyticsService.CapturePublicationCall(
                publicationId: publicationId,
                publicationTitle: publicationTitle,
                type: PublicationCallType.GetDataSets,
                parameters: new PaginationParameters(2, 5)
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }

        [Fact]
        public async Task RequestFromEes_AnalyticsIgnored()
        {
            fixture.HttpContextAccessorMock.SetupHasHeader(RequestHeaderNames.RequestSource);

            await fixture.AnalyticsService.CapturePublicationCall(
                publicationId: Guid.NewGuid(),
                publicationTitle: "Publication title",
                type: PublicationCallType.GetDataSets,
                parameters: new PaginationParameters(2, 5)
            );

            fixture.AnalyticsManagerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ExceptionThrown_Caught()
        {
            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(It.IsAny<CapturePublicationCallRequest>(), It.IsAny<CancellationToken>())
                )
                .Throws(new Exception("Error"));

            await fixture.AnalyticsService.CapturePublicationCall(
                publicationId: Guid.NewGuid(),
                publicationTitle: "Publication title",
                type: PublicationCallType.GetDataSets,
                parameters: new PaginationParameters(2, 5)
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }
    }

    public class CaptureDataSetCallTests(AnalyticsServiceTestsFixture fixture) : AnalyticsServiceTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            PreviewToken previewToken = DataFixture.DefaultPreviewToken();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithVersions(DataFixture.DefaultDataSetVersion().WithPreviewTokens([previewToken]).GenerateList(1));

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture.HttpContextAccessorMock.SetupHasHeader(RequestHeaderNames.PreviewToken, previewToken.Id.ToString());

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(
                        ItIs.DeepEqualTo(
                            new CaptureDataSetCallRequest(
                                dataSet.Id,
                                dataSet.Title,
                                new PreviewTokenRequest(
                                    previewToken.Label,
                                    previewToken.DataSetVersionId,
                                    previewToken.Created,
                                    previewToken.Expires
                                ),
                                fixture.GetUtcNow(),
                                DataSetCallType.GetVersions,
                                new PaginationParameters(2, 5)
                            )
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            await fixture.AnalyticsService.CaptureDataSetCall(
                dataSetId: dataSet.Id,
                type: DataSetCallType.GetVersions,
                parameters: new PaginationParameters(2, 5)
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }

        [Fact]
        public async Task RequestFromEes_AnalyticsIgnored()
        {
            fixture.HttpContextAccessorMock.SetupHasHeader(RequestHeaderNames.RequestSource);

            await fixture.AnalyticsService.CaptureDataSetCall(
                dataSetId: Guid.NewGuid(),
                type: DataSetCallType.GetVersions,
                parameters: new PaginationParameters(2, 5)
            );

            fixture.AnalyticsManagerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ExceptionThrown_Caught()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithVersions(DataFixture.DefaultDataSetVersion().GenerateList(1));

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.PreviewToken);

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(It.IsAny<CaptureDataSetCallRequest>(), It.IsAny<CancellationToken>())
                )
                .Throws(new Exception("Error"));

            await fixture.AnalyticsService.CaptureDataSetCall(
                dataSetId: dataSet.Id,
                type: DataSetCallType.GetVersions,
                parameters: new PaginationParameters(2, 5)
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }
    }

    public class CaptureDataSetVersionCallTests(AnalyticsServiceTestsFixture fixture) : AnalyticsServiceTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            PreviewToken previewToken = DataFixture.DefaultPreviewToken();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithVersions(DataFixture.DefaultDataSetVersion().WithPreviewTokens([previewToken]).GenerateList(1));

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            var dataSetVersion = dataSet.Versions.Single();

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture.HttpContextAccessorMock.SetupHasHeader(RequestHeaderNames.PreviewToken, previewToken.Id.ToString());

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(
                        ItIs.DeepEqualTo(
                            new CaptureDataSetVersionCallRequest(
                                dataSet.Id,
                                dataSetVersion.Id,
                                dataSetVersion.SemVersion().ToString(),
                                dataSet.Title,
                                new PreviewTokenRequest(
                                    previewToken.Label,
                                    previewToken.DataSetVersionId,
                                    previewToken.Created,
                                    previewToken.Expires
                                ),
                                "1.2.0",
                                fixture.GetUtcNow(),
                                DataSetVersionCallType.GetChanges,
                                new PaginationParameters(2, 5)
                            )
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            await fixture.AnalyticsService.CaptureDataSetVersionCall(
                dataSetVersionId: dataSetVersion.Id,
                type: DataSetVersionCallType.GetChanges,
                parameters: new PaginationParameters(2, 5),
                requestedDataSetVersion: "1.2.0",
                cancellationToken: CancellationToken.None
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }

        [Fact]
        public async Task RequestFromEes_AnalyticsIgnored()
        {
            fixture.HttpContextAccessorMock.SetupHasHeader(RequestHeaderNames.RequestSource);

            await fixture.AnalyticsService.CaptureDataSetVersionCall(
                dataSetVersionId: Guid.NewGuid(),
                type: DataSetVersionCallType.GetChanges,
                parameters: new PaginationParameters(2, 5),
                requestedDataSetVersion: "1.3.0"
            );

            fixture.AnalyticsManagerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ExceptionThrown_Caught()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithVersions(DataFixture.DefaultDataSetVersion().GenerateList(1));

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.PreviewToken);

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(It.IsAny<CaptureDataSetVersionCallRequest>(), It.IsAny<CancellationToken>())
                )
                .Throws(new Exception("Error"));

            await fixture.AnalyticsService.CaptureDataSetVersionCall(
                dataSetVersionId: dataSet.Versions.Single().Id,
                type: DataSetVersionCallType.GetChanges,
                parameters: new PaginationParameters(2, 5),
                requestedDataSetVersion: "1.3.0"
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }
    }

    public class CaptureDataSetVersionQueryTests(AnalyticsServiceTestsFixture fixture) : AnalyticsServiceTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            PreviewToken previewToken = DataFixture.DefaultPreviewToken();

            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithVersions(DataFixture.DefaultDataSetVersion().WithPreviewTokens([previewToken]).GenerateList(1));

            await fixture.GetPublicDataDbContext().AddTestData(context => context.PreviewTokens.Add(previewToken));

            var dataSetVersion = dataSet.Versions.Single();

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture.HttpContextAccessorMock.SetupHasHeader(RequestHeaderNames.PreviewToken, previewToken.Id.ToString());

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(
                        ItIs.DeepEqualTo(
                            new CaptureDataSetVersionQueryRequest(
                                dataSet.Id,
                                dataSetVersion.Id,
                                dataSetVersion.SemVersion().ToString(),
                                dataSet.Title,
                                new PreviewTokenRequest(
                                    previewToken.Label,
                                    previewToken.DataSetVersionId,
                                    previewToken.Created,
                                    previewToken.Expires
                                ),
                                "1.2.0",
                                0,
                                100,
                                fixture.GetUtcNow().UtcDateTime,
                                fixture.GetUtcNow().UtcDateTime,
                                new DataSetQueryRequest { Debug = true }
                            )
                        ),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            await fixture.AnalyticsService.CaptureDataSetVersionQuery(
                dataSetVersion: dataSetVersion,
                requestedDataSetVersion: "1.2.0",
                query: new DataSetQueryRequest { Debug = true },
                new DataSetQueryPaginatedResultsViewModel
                {
                    Results = [],
                    Paging = new PagingViewModel(1, 10, 100),
                    Warnings = [],
                },
                startTime: fixture.GetUtcNow().UtcDateTime,
                cancellationToken: CancellationToken.None
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }

        [Fact]
        public async Task RequestFromEes_AnalyticsIgnored()
        {
            fixture.HttpContextAccessorMock.SetupHasHeader(RequestHeaderNames.RequestSource);

            await fixture.AnalyticsService.CaptureDataSetVersionQuery(
                dataSetVersion: DataFixture.DefaultDataSetVersion(),
                requestedDataSetVersion: "1.3.0",
                query: new DataSetQueryRequest { Debug = true },
                results: new DataSetQueryPaginatedResultsViewModel
                {
                    Warnings = [],
                    Results = [],
                    Paging = new PagingViewModel(1, 10, 100),
                },
                startTime: fixture.GetUtcNow().UtcDateTime,
                cancellationToken: CancellationToken.None
            );

            fixture.AnalyticsManagerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ExceptionThrown_Caught()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithVersions(DataFixture.DefaultDataSetVersion().GenerateList(1));

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.RequestSource);

            fixture.HttpContextAccessorMock.SetupDoesNotHaveHeader(RequestHeaderNames.PreviewToken);

            fixture
                .AnalyticsManagerMock.Setup(s =>
                    s.Add(It.IsAny<CaptureDataSetVersionQueryRequest>(), It.IsAny<CancellationToken>())
                )
                .Throws(new Exception("Error"));

            await fixture.AnalyticsService.CaptureDataSetVersionQuery(
                dataSetVersion: dataSet.Versions.Single(),
                requestedDataSetVersion: "1.2.0",
                query: new DataSetQueryRequest { Debug = true },
                new DataSetQueryPaginatedResultsViewModel
                {
                    Results = [],
                    Paging = new PagingViewModel(1, 10, 100),
                    Warnings = [],
                },
                startTime: fixture.GetUtcNow().UtcDateTime,
                cancellationToken: CancellationToken.None
            );

            MockUtils.VerifyAllMocks(fixture.AnalyticsManagerMock);
        }
    }
}
