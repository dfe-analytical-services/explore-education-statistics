using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public class TestControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    [Fact]
    public async Task Test()
    {
        DataSet dataSet = DataFixture.DefaultDataSet();

        await TestApp.AddTestData<PublicDataDbContext>(context =>
        {
            context.DataSets.Add(dataSet);
        });

        DataSetVersion oldDataSetVersion = DataFixture
            .DefaultDataSetVersion(
                filters: 4,
                indicators: 4,
                locations: 5,
                timePeriods: 5)
            .WithVersionNumber(1, 0)
            .WithDataSetId(dataSet.Id);


        await TestApp.AddTestData<PublicDataDbContext>(context =>
        {
            context.DataSetVersions.Add(oldDataSetVersion);
        });

        DataSetVersion dataSetVersion = DataFixture
            .DefaultDataSetVersion(
                filters: 4,
                indicators: 4,
                locations: 5,
                timePeriods: 5)
            .WithVersionNumber(1, 1)
            .WithDataSetId(dataSet.Id);

        await TestApp.AddTestData<PublicDataDbContext>(context =>
        {
            context.DataSetVersions.Add(dataSetVersion);
        });

        var filterMetaChanges = DataFixture
            .DefaultFilterMetaChange()
            .WithDataSetVersionId(dataSetVersion.Id)
            .ForIndex(0, s => s.SetCurrentStateId(dataSetVersion.FilterMetas[0].Id))
            .ForIndex(1, s => s.SetCurrentStateId(dataSetVersion.FilterMetas[1].Id))
            .GenerateList();

        var filterOptionMetaChanges = DataFixture
            .DefaultFilterOptionMetaChange()
            .WithDataSetVersionId(dataSetVersion.Id)
            .ForIndex(0, s => s
                .SetMetaId(dataSetVersion.FilterMetas[0].Id)
                .SetCurrentStateId(dataSetVersion.FilterMetas[0].Options[0].Id)
                .SetPreviousStateId(oldDataSetVersion.FilterMetas[0].Options[0].Id))
            .ForIndex(1, s => s
                .SetMetaId(dataSetVersion.FilterMetas[0].Id)
                .SetCurrentStateId(dataSetVersion.FilterMetas[0].Options[1].Id)
                .SetPreviousStateId(oldDataSetVersion.FilterMetas[0].Options[1].Id))
            .GenerateList();

        var geographicLevelMetaChanges = DataFixture
            .DefaultGeographicLevelMetaChange()
            .WithDataSetVersionId(dataSetVersion.Id)
            .WithPreviousStateId(oldDataSetVersion.GeographicLevelMeta!.Id)
            .WithCurrentStateId(dataSetVersion.GeographicLevelMeta!.Id)
            .GenerateList(1);

        var indicatorMetaChanges = DataFixture
            .DefaultIndicatorMetaChange()
            .WithDataSetVersionId(dataSetVersion.Id)
            .ForIndex(0, s => s
                .SetCurrentStateId(dataSetVersion.IndicatorMetas[0].Id)
                .SetPreviousStateId(oldDataSetVersion.IndicatorMetas[0].Id))
            .ForIndex(1, s => s
                .SetCurrentStateId(dataSetVersion.IndicatorMetas[1].Id)
                .SetPreviousStateId(oldDataSetVersion.IndicatorMetas[1].Id))
            .GenerateList();

        var locationMetaChanges = DataFixture
            .DefaultLocationMetaChange()
            .WithDataSetVersionId(dataSetVersion.Id)
            .ForIndex(0, s => s
                .SetCurrentStateId(dataSetVersion.LocationMetas[0].Id)
                .SetPreviousStateId(oldDataSetVersion.LocationMetas[0].Id))
            .ForIndex(1, s => s
                .SetCurrentStateId(dataSetVersion.LocationMetas[0].Id))
            .GenerateList();

        var locationOptionMetaChanges = DataFixture
            .DefaultLocationOptionMetaChange()
            .WithDataSetVersionId(dataSetVersion.Id)
            .ForIndex(0, s => s
                .SetMetaId(dataSetVersion.LocationMetas[0].Id)
                .SetCurrentStateId(dataSetVersion.LocationMetas[0].Options[0].Id)
                .SetPreviousStateId(oldDataSetVersion.LocationMetas[0].Options[0].Id))
            .ForIndex(1, s => s
                .SetMetaId(dataSetVersion.LocationMetas[1].Id)
                .SetCurrentStateId(dataSetVersion.LocationMetas[1].Options[0].Id))
            .GenerateList();

        var timePeriodMetaChanges = DataFixture
            .DefaultTimePeriodMetaChange()
            .WithDataSetVersionId(dataSetVersion.Id)
            .ForIndex(0, s => s
                .SetCurrentStateId(dataSetVersion.TimePeriodMetas[0].Id))
            .ForIndex(1, s => s
                .SetCurrentStateId(dataSetVersion.TimePeriodMetas[1].Id))
            .ForIndex(2, s => s
                .SetCurrentStateId(dataSetVersion.TimePeriodMetas[2].Id))
            .GenerateList();

        await TestApp.AddTestData<PublicDataDbContext>(context =>
        {
            context.FilterMetaChanges.AddRange(filterMetaChanges);
            context.FilterOptionMetaChanges.AddRange(filterOptionMetaChanges);
            context.GeographicLevelMetaChanges.AddRange(geographicLevelMetaChanges);
            context.IndicatorMetaChanges.AddRange(indicatorMetaChanges);
            context.LocationMetaChanges.AddRange(locationMetaChanges);
            context.LocationOptionMetaChanges.AddRange(locationOptionMetaChanges);
            context.TimePeriodMetaChanges.AddRange(timePeriodMetaChanges);
        });

        await using var context = TestApp.GetDbContext<PublicDataDbContext>();

        var dbDataSetVersion = await context.DataSetVersions
            .AsSplitQuery()
            .Include(dsv => dsv.FilterMetaChanges)
            .Include(dsv => dsv.FilterOptionMetaChanges)
            .Include(dsv => dsv.GeographicLevelMetaChanges)
            .Include(dsv => dsv.IndicatorMetaChanges)
            .Include(dsv => dsv.LocationMetaChanges)
            .Include(dsv => dsv.LocationOptionMetaChanges)
            .Include(dsv => dsv.TimePeriodMetaChanges)
            .FirstAsync(dsv => dsv.Id == dataSetVersion.Id);

        Assert.NotNull(dbDataSetVersion);
        Assert.NotEmpty(dbDataSetVersion.FilterMetaChanges);
        Assert.NotEmpty(dbDataSetVersion.FilterOptionMetaChanges);
        Assert.NotEmpty(dbDataSetVersion.GeographicLevelMetaChanges);
        Assert.NotEmpty(dbDataSetVersion.IndicatorMetaChanges);
        Assert.NotEmpty(dbDataSetVersion.LocationMetaChanges);
        Assert.NotEmpty(dbDataSetVersion.LocationOptionMetaChanges);
        Assert.NotEmpty(dbDataSetVersion.TimePeriodMetaChanges);
    }
}
