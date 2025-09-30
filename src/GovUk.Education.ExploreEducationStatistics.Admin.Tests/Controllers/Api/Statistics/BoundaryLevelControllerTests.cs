#nullable enable
using System.Net.Http.Json;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics;

public class BoundaryLevelControllerTests(TestApplicationFactory testApp)
    : IntegrationTestFixture(testApp)
{
    [Fact]
    public async Task ListBoundaryLevels_Success()
    {
        await TestApp.AddTestData<StatisticsDbContext>(context =>
        {
            context.BoundaryLevel.AddRange(
                new()
                {
                    Id = 1,
                    Created = DateTime.Now,
                    Label = "Boundary Level 1",
                    Level = GeographicLevel.Country,
                    Published = DateTime.Now,
                },
                new()
                {
                    Id = 2,
                    Created = DateTime.Now,
                    Label = "Boundary Level 2",
                    Level = GeographicLevel.Region,
                    Published = DateTime.Now,
                }
            );
        });

        var client = TestApp.SetUser(DataFixture.BauUser()).CreateClient();

        var response = await client.GetAsync("api/boundary-level");

        var result = response.AssertOk<List<BoundaryLevelViewModel>>();

        Assert.Multiple(
            () => Assert.Equal(1, result[0].Id),
            () => Assert.Equal(2, result[1].Id),
            () => Assert.Equal(GeographicLevel.Country, result[0].Level),
            () => Assert.Equal(GeographicLevel.Region, result[1].Level),
            () => Assert.Equal("Boundary Level 1", result[0].Label),
            () => Assert.Equal("Boundary Level 2", result[1].Label)
        );
    }

    [Fact]
    public async Task GetBoundaryLevel_Success()
    {
        await TestApp.AddTestData<StatisticsDbContext>(context =>
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

        var client = TestApp.SetUser(DataFixture.BauUser()).CreateClient();

        var response = await client.GetAsync("api/boundary-level/1");

        var result = response.AssertOk<BoundaryLevelViewModel>();

        Assert.Multiple(
            () => Assert.Equal(1, result.Id),
            () => Assert.Equal("Boundary Level 1", result.Label)
        );
    }

    [Fact]
    public async Task UpdateBoundaryLevel_Success()
    {
        await TestApp.AddTestData<StatisticsDbContext>(context =>
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

        var client = TestApp.SetUser(DataFixture.BauUser()).CreateClient();

        var request = new BoundaryLevelUpdateRequest { Id = 1, Label = "Updated Boundary Level 1" };

        var response = await client.PatchAsync("api/boundary-level", JsonContent.Create(request));

        response.AssertNoContent();

        await using var context = TestApp.GetDbContext<StatisticsDbContext>();

        var saved = await context.BoundaryLevel.SingleAsync(bl => bl.Id == request.Id);

        Assert.Multiple(
            () => Assert.Equal(request.Id, saved.Id),
            () => Assert.Equal(request.Label, saved.Label),
            () => Assert.NotNull(saved.Updated)
        );
    }

    [Fact]
    public async Task CreateBoundaryLevel_Success()
    {
        var client = TestApp.SetUser(DataFixture.BauUser()).CreateClient();

        var path = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources/REG - Regions England BUC 202212.geojson"
        );
        var content = new MultipartFormDataContent
        {
            { new StringContent("Region"), "level" },
            { new StringContent("Boundary Level 1"), "label" },
            {
                new StreamContent(File.OpenRead(path)),
                "file",
                "REG - Regions England BUC 202212.geojson"
            },
            { new StringContent("2020-01-01"), "published" },
        };

        var response = await client.PostAsync("api/boundary-level", content);

        response.AssertNoContent();

        await using var context = TestApp.GetDbContext<StatisticsDbContext>();

        var savedBoundaryLevel = Assert.Single(context.BoundaryLevel.Where(bl => bl.Id == 1));
        Assert.Single(context.BoundaryData.Where(bd => bd.Id == 1));
        Assert.Equal(GeographicLevel.Region, savedBoundaryLevel.Level);
    }
}
