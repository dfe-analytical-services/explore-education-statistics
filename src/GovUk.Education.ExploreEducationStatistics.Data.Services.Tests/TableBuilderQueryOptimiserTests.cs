#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.ValidationErrorMessages;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class TableBuilderQueryOptimiserTests
{
    private readonly Mock<IStorageDataSet> _dataSet;
    private readonly TableBuilderQueryOptimiser _optimiser;

    public TableBuilderQueryOptimiserTests()
    {
        _dataSet = new Mock<IStorageDataSet>(Strict);
        _optimiser = BuildOptimiser(maxTableCellsAllowed: 20);
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
        Assert.False(result.AssertRight());
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
        Assert.True(result.AssertRight());
    }

    [Fact]
    public async Task IsCroppingRequired_FilterItemsNotFound_ReturnsValidationError()
    {
        // Arrange
        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();

        // The data set only knows about some of the filter items the query specifies
        SetupListFilterItems(query, BuildFilterItems(query.GetFilterItemIds().Skip(1)));

        // Act
        var result = await _optimiser.IsCroppingRequired(query);

        // Assert
        result.AssertBadRequest(FilterItemsNotFound);
    }

    [Fact]
    public async Task CropQuery_ExcessiveTimePeriods_ReturnsFewerTimePeriods()
    {
        // Arrange
        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();

        SetupListFilterItems(query, BuildFilterItems(query.GetFilterItemIds()));

        // Cropping the time periods alone should bring the query within the limit:
        // 2 indicators x 2 locations x 5 time periods x 10 filter items
        var optimiser = BuildOptimiser(maxTableCellsAllowed: 200);

        // Act
        var result = await optimiser.CropQuery(query, default);

        // Assert
        _dataSet.Verify();

        var croppedQuery = result.AssertRight();
        Assert.NotNull(croppedQuery.TimePeriod);
        Assert.Equal(2016, croppedQuery.TimePeriod?.StartYear);
        Assert.Equal(2020, croppedQuery.TimePeriod?.EndYear);
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

        var croppedQuery = result.AssertRight();
        Assert.NotNull(croppedQuery.TimePeriod);
        Assert.Equal(2016, croppedQuery.TimePeriod?.StartYear);
        Assert.Equal(2020, croppedQuery.TimePeriod?.EndYear);
        Assert.Single(croppedQuery.LocationIds);
    }

    [Fact]
    public async Task CropQuery_FilterItemsNotFound_ReturnsValidationError()
    {
        // Arrange
        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();

        SetupListFilterItems(query, BuildFilterItems(query.GetFilterItemIds().Skip(1)));

        // Act
        var result = await _optimiser.CropQuery(query, default);

        // Assert
        result.AssertBadRequest(FilterItemsNotFound);
    }

    private TableBuilderQueryOptimiser BuildOptimiser(int maxTableCellsAllowed)
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
