#nullable enable
using System.Net.Http.Json;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics;

// ReSharper disable once ClassNeverInstantiated.Global
public class BoundaryLevelControllerTestsFixture()
    : OptimisedAdminCollectionFixture(capabilities: [AdminIntegrationTestCapability.UserAuth]);

[CollectionDefinition(nameof(BoundaryLevelControllerTestsFixture))]
public class BoundaryLevelControllerTestsCollection : ICollectionFixture<BoundaryLevelControllerTestsFixture>;

[Collection(nameof(BoundaryLevelControllerTestsFixture))]
public class BoundaryLevelControllerTests(BoundaryLevelControllerTestsFixture fixture)
{
    [Fact]
    public async Task ListBoundaryLevels_Success()
    {
        var boundaryLevel1 = new BoundaryLevel
        {
            Created = DateTime.Now,
            Label = "Boundary Level 1",
            Level = GeographicLevel.Country,
            Published = DateTime.Now,
        };

        var boundaryLevel2 = new BoundaryLevel
        {
            Created = DateTime.Now,
            Label = "Boundary Level 2",
            Level = GeographicLevel.Region,
            Published = DateTime.Now,
        };

        await fixture
            .GetStatisticsDbContext()
            .AddTestData(context => context.BoundaryLevel.AddRange(boundaryLevel1, boundaryLevel2));

        var client = fixture.CreateClient().WithUser(OptimisedTestUsers.Bau);

        var response = await client.GetAsync("api/boundary-level");

        var result = response.AssertOk<List<BoundaryLevelViewModel>>();

        long[] boundaryLevelIds = [boundaryLevel1.Id, boundaryLevel2.Id];

        var matchingBoundaryLevels = result.Where(b => boundaryLevelIds.Contains(b.Id)).ToList();

        Assert.Multiple(
            () => Assert.Equal(boundaryLevel1.Id, matchingBoundaryLevels[0].Id),
            () => Assert.Equal(boundaryLevel1.Level, matchingBoundaryLevels[0].Level),
            () => Assert.Equal(boundaryLevel1.Label, matchingBoundaryLevels[0].Label),
            () => Assert.Equal(boundaryLevel2.Id, matchingBoundaryLevels[1].Id),
            () => Assert.Equal(boundaryLevel2.Level, matchingBoundaryLevels[1].Level),
            () => Assert.Equal(boundaryLevel2.Label, matchingBoundaryLevels[1].Label)
        );
    }

    [Fact]
    public async Task GetBoundaryLevel_Success()
    {
        await fixture
            .GetStatisticsDbContext()
            .AddTestData(context =>
            {
                context.BoundaryLevel.Add(
                    new()
                    {
                        Id = 1,
                        Created = DateTime.Now,
                        Label = "Boundary Level 1",
                        Level = GeographicLevel.Country,
                        Published = DateTime.Now,
                    }
                );
            });

        var client = fixture.CreateClient().WithUser(OptimisedTestUsers.Bau);

        var response = await client.GetAsync("api/boundary-level/1");

        var result = response.AssertOk<BoundaryLevelViewModel>();

        Assert.Multiple(() => Assert.Equal(1, result.Id), () => Assert.Equal("Boundary Level 1", result.Label));
    }

    [Fact]
    public async Task UpdateBoundaryLevel_Success()
    {
        var boundaryLevel = new BoundaryLevel
        {
            Created = DateTime.Now,
            Label = "Boundary Level UpdateBoundaryLevel_Success",
            Level = GeographicLevel.Country,
            Published = DateTime.Now.AddDays(-1),
        };

        await fixture
            .GetStatisticsDbContext()
            .AddTestData(context =>
            {
                context.BoundaryLevel.Add(boundaryLevel);
            });

        var client = fixture.CreateClient().WithUser(OptimisedTestUsers.Bau);

        var updatedLabel = $"Updated {boundaryLevel.Label}";
        var request = new BoundaryLevelUpdateRequest { Id = boundaryLevel.Id, Label = updatedLabel };

        var response = await client.PatchAsync("api/boundary-level", JsonContent.Create(request));

        response.AssertNoContent();

        var updatedBoundaryLevel = Assert.Single(
            fixture.GetStatisticsDbContext().BoundaryLevel.Where(bl => bl.Label == updatedLabel)
        );

        Assert.Multiple(
            () => Assert.Equal(boundaryLevel.Level, updatedBoundaryLevel.Level),
            () => Assert.Equal(boundaryLevel.Published, updatedBoundaryLevel.Published),
            () => updatedBoundaryLevel.Updated.AssertUtcNow()
        );
    }

    [Fact]
    public async Task CreateBoundaryLevel_Success()
    {
        var client = fixture.CreateClient().WithUser(OptimisedTestUsers.Bau);

        var path = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources/REG - Regions England BUC 202212.geojson"
        );

        var label = "Boundary Level CreateBoundaryLevel_Success";

        var content = new MultipartFormDataContent
        {
            { new StringContent("Region"), "level" },
            { new StringContent(label), "label" },
            { new StreamContent(File.OpenRead(path)), "file", "REG - Regions England BUC 202212.geojson" },
            { new StringContent("2020-01-01"), "published" },
        };

        var response = await client.PostAsync("api/boundary-level", content);

        response.AssertNoContent();

        var savedBoundaryLevel = Assert.Single(
            fixture.GetStatisticsDbContext().BoundaryLevel.Where(bl => bl.Label == label)
        );
        Assert.Equal(GeographicLevel.Region, savedBoundaryLevel.Level);
        Assert.Equal(new DateTime(2020, 01, 01), savedBoundaryLevel.Published);
    }
}
