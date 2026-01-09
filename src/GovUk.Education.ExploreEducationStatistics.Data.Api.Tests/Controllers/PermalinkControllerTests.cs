#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Fixtures.Optimised;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global
public class PermalinkControllerTestsFixture : OptimisedDataApiCollectionFixture
{
    public Mock<IPermalinkService> PermalinkServiceMock = null!;

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        PermalinkServiceMock = new Mock<IPermalinkService>(MockBehavior.Strict);
        serviceModifications.ReplaceService(PermalinkServiceMock.Object);
    }

    public override async Task BeforeEachTest()
    {
        await base.BeforeEachTest();

        PermalinkServiceMock.Reset();
    }
}

[CollectionDefinition(nameof(PermalinkControllerTestsFixture))]
public class PermalinkControllerTestsCollection : ICollectionFixture<PermalinkControllerTestsFixture>;

[Collection(nameof(PermalinkControllerTestsFixture))]
public class PermalinkControllerTests(PermalinkControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    [Fact]
    public async Task CreatePermalink()
    {
        var createRequest = new PermalinkCreateRequest
        {
            Query = new FullTableQueryRequest
            {
                LocationIds = new List<Guid> { Guid.NewGuid() },
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2000,
                    StartCode = TimeIdentifier.AcademicYear,
                    EndYear = 2001,
                    EndCode = TimeIdentifier.AcademicYear,
                },
                Indicators = new List<Guid> { Guid.NewGuid() },
            },
        };

        var expectedResult = new PermalinkViewModel { Id = Guid.NewGuid() };

        fixture
            .PermalinkServiceMock.Setup(s =>
                s.CreatePermalink(
                    It.Is<PermalinkCreateRequest>(r => r.IsDeepEqualTo(createRequest, null)),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        var response = await fixture
            .CreateClient()
            .PostAsync(requestUri: "/api/permalink", content: new JsonNetContent(createRequest));

        MockUtils.VerifyAllMocks(fixture.PermalinkServiceMock);

        response.AssertCreated(expectedResult, $"http://localhost/api/permalink/{expectedResult.Id}");
    }

    [Fact]
    public async Task GetPermalink()
    {
        var permalinkId = Guid.NewGuid();
        var permalink = new PermalinkViewModel { Id = permalinkId };

        fixture
            .PermalinkServiceMock.Setup(s => s.GetPermalink(permalinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permalink);

        var response = await fixture
            .CreateClient()
            .GetAsync(
                uri: $"/api/permalink/{permalinkId}",
                headers: new Dictionary<string, string> { { HeaderNames.Accept, "application/json" } }
            );

        MockUtils.VerifyAllMocks(fixture.PermalinkServiceMock);

        response.AssertOk(permalink);
    }

    [Fact]
    public async Task GetPermalink_NotFound()
    {
        var permalinkId = Guid.NewGuid();

        fixture
            .PermalinkServiceMock.Setup(s => s.GetPermalink(permalinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        var response = await fixture
            .CreateClient()
            .GetAsync(
                uri: $"/api/permalink/{permalinkId}",
                headers: new Dictionary<string, string> { { HeaderNames.Accept, "application/json" } }
            );

        MockUtils.VerifyAllMocks(fixture.PermalinkServiceMock);

        response.AssertNotFound();
    }

    [Fact]
    public async Task GetPermalink_Csv()
    {
        var permalinkId = Guid.NewGuid();

        fixture
            .PermalinkServiceMock.Setup(s => s.GetCsvDownloadStream(permalinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test csv".ToStream());

        var response = await fixture
            .CreateClient()
            .GetAsync(
                uri: $"/api/permalink/{permalinkId}",
                headers: new Dictionary<string, string> { { HeaderNames.Accept, "text/csv, application/json" } }
            );

        MockUtils.VerifyAllMocks(fixture.PermalinkServiceMock);

        response.AssertOk("Test csv");
    }

    [Fact]
    public async Task GetPermalink_Csv_NotFound()
    {
        var permalinkId = Guid.NewGuid();

        fixture
            .PermalinkServiceMock.Setup(s => s.GetCsvDownloadStream(permalinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFoundResult());

        var response = await fixture
            .CreateClient()
            .GetAsync(
                uri: $"/api/permalink/{permalinkId}",
                headers: new Dictionary<string, string> { { HeaderNames.Accept, "text/csv, application/json" } }
            );

        MockUtils.VerifyAllMocks(fixture.PermalinkServiceMock);

        response.AssertNotFound();
    }

    [Fact]
    public async Task GetPermalink_InvalidIdReturnsNotFound()
    {
        var response = await fixture.CreateClient().GetAsync("/api/permalink/not-a-guid");

        response.AssertNotFound();
    }
}
