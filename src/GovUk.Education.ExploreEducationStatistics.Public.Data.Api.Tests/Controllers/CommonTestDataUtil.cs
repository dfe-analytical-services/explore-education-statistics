using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public static class CommonTestDataUtil
{
    private static readonly DataFixture DataFixture = new(new Random().Next());

    public static async Task<(DataSet, List<DataSetVersion>)> SetupDataSetWithSpecifiedVersionStatuses(
        DataSetVersionStatus versionStatus,
        PublicDataDbContext publicDataDbContext
    )
    {
        DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

        await publicDataDbContext.AddTestData(context => context.DataSets.Add(dataSet));

        var dataSetVersion = DataFixture
            .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
            .WithStatusPublished()
            .WithDataSetId(dataSet.Id)
            .WithMetaSummary(
                DataFixture
                    .DefaultDataSetVersionMetaSummary()
                    .WithGeographicLevels([
                        GeographicLevel.Country,
                        GeographicLevel.LocalAuthority,
                        GeographicLevel.Region,
                        GeographicLevel.School,
                    ])
            )
            .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
            .ForIndex(1, dsv => dsv.SetVersionNumber(1, 1))
            .ForIndex(2, dsv => dsv.SetVersionNumber(1, 2))
            .ForIndex(
                3,
                dsv =>
                {
                    dsv.SetStatus(versionStatus);
                    dsv.SetVersionNumber(2, 0);
                }
            )
            .ForIndex(
                4,
                dsv =>
                {
                    dsv.SetStatus(versionStatus);
                    dsv.SetVersionNumber(2, 1);
                }
            )
            .GenerateList();

        var version = versionStatus == DataSetVersionStatus.Published ? "2.1" : "1.2";

        dataSet.LatestLiveVersion = dataSetVersion.FirstOrDefault(dsv => dsv.PublicVersion == version);
        dataSet.Versions = dataSetVersion;

        await publicDataDbContext.AddTestData(context =>
        {
            context.DataSetVersions.AddRange(dataSetVersion);
            context.DataSets.Update(dataSet);
        });

        return (dataSet, dataSetVersion);
    }

    public static async Task<DataSetVersion> SetupDefaultDataSetVersion(
        PublicDataDbContext publicDataDbContext,
        DataSetVersionStatus versionStatus = DataSetVersionStatus.Published
    )
    {
        DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

        await publicDataDbContext.AddTestData(context => context.DataSets.Add(dataSet));

        DataSetVersion dataSetVersion = DataFixture
            .DefaultDataSetVersion()
            .WithDataSet(dataSet)
            .WithMetaSummary(
                DataFixture
                    .DefaultDataSetVersionMetaSummary()
                    .WithGeographicLevels([
                        GeographicLevel.Country,
                        GeographicLevel.LocalAuthority,
                        GeographicLevel.Region,
                        GeographicLevel.School,
                    ])
            )
            .WithStatus(versionStatus);

        dataSet.LatestLiveVersion = dataSetVersion;

        await publicDataDbContext.AddTestData(context =>
        {
            context.DataSetVersions.Add(dataSetVersion);
            context.DataSets.Update(dataSet);
        });

        return dataSetVersion;
    }
}
