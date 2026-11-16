#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils.StorageDataSetTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

public class ImporterMetaServiceTests
{
    [Fact]
    public async Task GetSubjectMeta_IndicatorAndGroupLabelsDifferInCase_MatchesIndicator()
    {
        var subjectId = Guid.NewGuid();
        var statisticsDbContextId = Guid.NewGuid().ToString();

        var indicator = new Indicator
        {
            Id = Guid.NewGuid(),
            Label = "Indicator label",
            Name = "indicator_column",
            IndicatorGroup = new IndicatorGroup("Indicator group", subjectId),
        };

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
        {
            statisticsDbContext.Indicator.Add(indicator);
            await statisticsDbContext.SaveChangesAsync();
        }

        var dataSet = BuildStatisticsDbDataSetFactory(InMemoryStatisticsDbContext(statisticsDbContextId))
            .Create(subjectId);

        var service = new ImporterMetaService(Mock.Of<IGuidGenerator>(Strict));

        var subjectMeta = await service.GetSubjectMeta(
            metaFileCsvHeaders: ["col_name", "col_type", "label", "indicator_grouping", "indicator_unit"],
            metaFileRows:
            [
                ["indicator_column", "Indicator", "INDICATOR LABEL", "INDICATOR GROUP", ""],
            ],
            dataSet
        );

        Assert.Empty(subjectMeta.Filters);
        var (matchedIndicator, column) = Assert.Single(subjectMeta.Indicators);
        Assert.Equal(indicator.Id, matchedIndicator.Id);
        Assert.Equal("indicator_column", column);
    }
}
