#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics;

public class TableBuilderControllerIntegrationTests : IntegrationTest<TestStartup>
{
    public TableBuilderControllerIntegrationTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
    {
    }

    private static readonly Guid ReleaseVersionId = Guid.NewGuid();
    private static readonly Guid SubjectId = Guid.NewGuid();

    private static readonly ObservationQueryContext ObservationQueryContext = new()
    {
        SubjectId = SubjectId,
        Filters = new List<Guid>(),
        Indicators = new List<Guid>(),
        LocationIds = new List<Guid>(),
        TimePeriod = new TimePeriodQuery
        {
            StartYear = 2021,
            StartCode = CalendarYear,
            EndYear = 2022,
            EndCode = CalendarYear
        }
    };

    private readonly TableBuilderResultViewModel _tableBuilderResults = new()
    {
        SubjectMeta = new SubjectResultMetaViewModel
        {
            TimePeriodRange = new List<TimePeriodMetaViewModel>
            {
                new(2020, AcademicYear),
                new(2021, AcademicYear),
            }
        },
        Results = new List<ObservationViewModel>
        {
            new()
            {
                TimePeriod = "2020_AY"
            },
            new()
            {
                TimePeriod = "2021_AY"
            }
        },
    };

    [Fact]
    public async Task Query()
    {
        var tableBuilderService = new Mock<ITableBuilderService>(Strict);
        tableBuilderService
            .Setup(s => s.Query(
                ReleaseVersionId,
                ItIs.DeepEqualTo(ObservationQueryContext),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_tableBuilderResults);

        var client = SetupApp(
                tableBuilderService: tableBuilderService.Object)
            .SetUser(AuthenticatedUser())
            .CreateClient();

        var response = await client.PostAsync(
            $"/api/data/tablebuilder/release/{ReleaseVersionId}",
            new JsonNetContent(ObservationQueryContext));

        VerifyAllMocks(tableBuilderService);

        response.AssertOk(_tableBuilderResults);
    }

    [Fact]
    public async Task Query_Csv()
    {
        var tableBuilderService = new Mock<ITableBuilderService>(Strict);
        tableBuilderService
            .Setup(s => s.QueryToCsvStream(
                ReleaseVersionId,
                ItIs.DeepEqualTo(ObservationQueryContext),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Instance)
            .Callback<Guid, ObservationQueryContext, Stream, CancellationToken>(
                (_, _, stream, _) => { stream.WriteText("Test csv"); });

        var client = SetupApp(tableBuilderService: tableBuilderService.Object)
            .SetUser(AuthenticatedUser())
            .CreateClient();

        var response = await client.PostAsync(
            $"/api/data/tablebuilder/release/{ReleaseVersionId}",
            content: new JsonNetContent(ObservationQueryContext),
            headers: new Dictionary<string, string>
            {
                { HeaderNames.Accept, ContentTypes.Csv}
            });

        VerifyAllMocks(tableBuilderService);

        response.AssertOk("Test csv");
    }

    private WebApplicationFactory<TestStartup> SetupApp(
        ITableBuilderService? tableBuilderService = null)
    {
        return TestApp.ConfigureServices(services =>
            services.ReplaceService(tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict)));
    }
}
