#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using Moq;
using Xunit;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class TableBuilderQueryOptimiserTests
{
    private readonly Mock<IStorageDataSet> _dataSet;
    private readonly TableBuilderQueryOptimiser _optimiser;

    public TableBuilderQueryOptimiserTests()
    {
        _dataSet = new Mock<IStorageDataSet>(Strict);
        _optimiser = BuildTableBuilderQueryOptimiser(maxTableCellsAllowed: 20);
    }

    [Fact]
    public async Task IsCroppingRequired_No_ReturnsFalse()
    {
        // Arrange
        var query = new FullTableQuery
        {
            TimePeriod = new TimePeriodQuery
            {
                StartYear = 2010,
                EndYear = 2011,
                EndCode = TimeIdentifier.AcademicYear,
                StartCode = TimeIdentifier.AcademicYear,
            },
        };

        // Act
        var result = await _optimiser.IsCroppingRequired(query);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsCroppingRequired_Yes_ReturnsTrue()
    {
        // Arrange
        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();

        SetupListFilterItems(query, BuildFilterItems(query.GetFilterItemIds()));

        // Act
        var result = await _optimiser.IsCroppingRequired(query);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(240, false)]
    [InlineData(239, true)]
    public async Task IsCroppingRequired_FilterItemsAcrossFilters_MultipliesCountsPerFilter(
        int maxTableCellsAllowed,
        bool expectedCroppingRequired
    )
    {
        // Arrange
        var query = new FullTableQueryBuilder().Build();
        var filterItemIds = query.GetFilterItemIds();

        // Filter A has 5 items across two groups, filter B has 3 and filter C has 2:
        // 2 indicators x 2 locations x 2 time periods x (5 x 3 x 2) filter items = 240
        var filterAId = Guid.NewGuid();
        var filterAGroup1 = new FilterGroup { FilterId = filterAId };
        var filterAGroup2 = new FilterGroup { FilterId = filterAId };
        var filterBGroup = new FilterGroup { FilterId = Guid.NewGuid() };
        var filterCGroup = new FilterGroup { FilterId = Guid.NewGuid() };

        var filterGroups = new[]
        {
            filterAGroup1,
            filterAGroup1,
            filterAGroup1,
            filterAGroup2,
            filterAGroup2,
            filterBGroup,
            filterBGroup,
            filterBGroup,
            filterCGroup,
            filterCGroup,
        };

        SetupListFilterItems(
            query,
            filterItemIds.Zip(filterGroups, (id, group) => new FilterItem { Id = id, FilterGroup = group }).ToList()
        );

        var optimiser = BuildTableBuilderQueryOptimiser(maxTableCellsAllowed);

        // Act
        var result = await optimiser.IsCroppingRequired(query);

        // Assert
        Assert.Equal(expectedCroppingRequired, result);
    }

    [Fact]
    public async Task IsCroppingRequired_FilterItemsNotFound_Throws()
    {
        // Arrange
        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();

        // The data set only knows about some of the filter items the query specifies
        SetupListFilterItems(query, BuildFilterItems(query.GetFilterItemIds().Skip(1)));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _optimiser.IsCroppingRequired(query));
    }

    [Fact]
    public async Task CropQuery_ExcessiveTimePeriods_ReturnsFewerTimePeriods()
    {
        // Arrange
        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();

        SetupListFilterItems(query, BuildFilterItems(query.GetFilterItemIds()));

        // Cropping the time periods alone should bring the query within the limit:
        // 2 indicators x 2 locations x 5 time periods x 10 filter items
        var optimiser = BuildTableBuilderQueryOptimiser(maxTableCellsAllowed: 200);

        // Act
        var result = await optimiser.CropQuery(query, default);

        // Assert
        _dataSet.Verify();

        Assert.NotNull(result.TimePeriod);
        Assert.Equal(2016, result.TimePeriod?.StartYear);
        Assert.Equal(2020, result.TimePeriod?.EndYear);
    }

    [Fact]
    public async Task CropQuery_ExcessiveLocations_ReturnsFewerLocations()
    {
        // Arrange
        var locations = Enumerable.Range(0, 5).Select(_ => LocationMockBuilder.Build()).ToList();
        var query = new FullTableQueryBuilder()
            .WithLocationIds([.. locations.Select(l => l.Id)])
            .WithEndYear(2020)
            .Build();

        SetupListFilterItems(query, BuildFilterItems(query.GetFilterItemIds()));

        _dataSet
            .Setup(mock =>
                mock.ListLocations(
                    It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(query.LocationIds)),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(locations);

        // Act
        var result = await _optimiser.CropQuery(query, default);

        // Assert
        _dataSet.Verify();

        Assert.NotNull(result.TimePeriod);
        Assert.Equal(2016, result.TimePeriod?.StartYear);
        Assert.Equal(2020, result.TimePeriod?.EndYear);
        Assert.Single(result.LocationIds);
    }

    [Fact]
    public async Task CropQuery_FilterItemsNotFound_Throws()
    {
        // Arrange
        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();

        SetupListFilterItems(query, BuildFilterItems(query.GetFilterItemIds().Skip(1)));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _optimiser.CropQuery(query, default));
    }

    private TableBuilderQueryOptimiser BuildTableBuilderQueryOptimiser(int maxTableCellsAllowed)
    {
        var storageDataSetResolver = new Mock<IStorageDataSetResolver>(Strict);
        storageDataSetResolver
            .Setup(mock => mock.Resolve(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dataSet.Object);

        return new TableBuilderQueryOptimiser(
            storageDataSetResolver.Object,
            new TableBuilderOptions { MaxTableCellsAllowed = maxTableCellsAllowed }.ToOptionsWrapper()
        );
    }

    private void SetupListFilterItems(FullTableQuery query, List<FilterItem> filterItems)
    {
        _dataSet
            .Setup(mock =>
                mock.ListFilterItems(
                    It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(query.GetFilterItemIds())),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(filterItems);
    }

    private static List<FilterItem> BuildFilterItems(IEnumerable<Guid> filterItemIds)
    {
        var filterGroup = new FilterGroup { FilterId = Guid.NewGuid() };
        return filterItemIds.Select(id => new FilterItem { Id = id, FilterGroup = filterGroup }).ToList();
    }
}
