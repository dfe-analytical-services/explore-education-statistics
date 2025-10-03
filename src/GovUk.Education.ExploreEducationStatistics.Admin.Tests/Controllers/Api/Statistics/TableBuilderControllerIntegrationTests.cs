#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics;

public class TableBuilderControllerIntegrationTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private static readonly Guid ReleaseVersionId = Guid.NewGuid();
    private static readonly Guid SubjectId = Guid.NewGuid();

    private static readonly FullTableQueryRequest FullTableQueryRequest = new()
    {
        SubjectId = SubjectId,
        Filters = new List<Guid>(), // data set might have no filters
        Indicators = new List<Guid> { Guid.NewGuid() },
        LocationIds = new List<Guid> { Guid.NewGuid() },
        TimePeriod = new TimePeriodQuery
        {
            StartYear = 2021,
            StartCode = CalendarYear,
            EndYear = 2022,
            EndCode = CalendarYear,
        },
        FilterHierarchiesOptions = null,
    };

    private static readonly FullTableQuery FullTableQuery = FullTableQueryRequest.AsFullTableQuery();

    private readonly TableBuilderResultViewModel _tableBuilderResults = new()
    {
        SubjectMeta = new SubjectResultMetaViewModel
        {
            TimePeriodRange =
            [
                new TimePeriodMetaViewModel(2020, AcademicYear),
                new TimePeriodMetaViewModel(2021, AcademicYear),
            ],
        },
        Results = new List<ObservationViewModel>
        {
            new() { TimePeriod = "2020_AY" },
            new() { TimePeriod = "2021_AY" },
        },
    };

    [Fact]
    public async Task Query()
    {
        var tableBuilderService = new Mock<ITableBuilderService>(Strict);
        tableBuilderService
            .Setup(s => s.Query(ReleaseVersionId, ItIs.DeepEqualTo(FullTableQuery), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tableBuilderResults);

        var client = SetupApp(tableBuilderService: tableBuilderService.Object)
            .SetUser(DataFixture.AuthenticatedUser())
            .CreateClient();

        var response = await client.PostAsync(
            $"/api/data/tablebuilder/release/{ReleaseVersionId}",
            new JsonNetContent(FullTableQueryRequest)
        );

        VerifyAllMocks(tableBuilderService);

        response.AssertOk(_tableBuilderResults);
    }

    [Fact]
    public async Task Query_Csv()
    {
        var tableBuilderService = new Mock<ITableBuilderService>(Strict);
        tableBuilderService
            .Setup(s =>
                s.QueryToCsvStream(
                    ReleaseVersionId,
                    ItIs.DeepEqualTo(FullTableQuery),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Unit.Instance)
            .Callback<Guid, FullTableQuery, Stream, CancellationToken>(
                (_, _, stream, _) =>
                {
                    stream.WriteText("Test csv");
                }
            );

        var client = SetupApp(tableBuilderService: tableBuilderService.Object)
            .SetUser(DataFixture.AuthenticatedUser())
            .CreateClient();

        var response = await client.PostAsync(
            $"/api/data/tablebuilder/release/{ReleaseVersionId}",
            content: new JsonNetContent(FullTableQueryRequest),
            headers: new Dictionary<string, string> { { HeaderNames.Accept, ContentTypes.Csv } }
        );

        VerifyAllMocks(tableBuilderService);

        response.AssertOk("Test csv");
    }

    private WebApplicationFactory<TestStartup> SetupApp(ITableBuilderService? tableBuilderService = null)
    {
        return TestApp.ConfigureServices(services =>
            services.ReplaceService(tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict))
        );
    }
}
