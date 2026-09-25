#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class TableBuilderQueryOptimiserTests
{
    private readonly Mock<IStorageDataSet> _dataSet;
    private readonly Mock<IStorageDataSetResolver> _storageDataSetResolver;
    private readonly IOptions<TableBuilderOptions> _options;
    private readonly TableBuilderQueryOptimiser _optimiser;

    public TableBuilderQueryOptimiserTests()
    {
        _dataSet = new Mock<IStorageDataSet>(Strict);
        _storageDataSetResolver = new Mock<IStorageDataSetResolver>(Strict);
        _storageDataSetResolver
            .Setup(mock => mock.Resolve(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dataSet.Object);
        _options = new TableBuilderOptions { MaxTableCellsAllowed = 20 }.ToOptionsWrapper();
        _optimiser = new TableBuilderQueryOptimiser(_storageDataSetResolver.Object, _options);
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
        var filtersCounts = new Dictionary<Guid, int>() { { Guid.NewGuid(), 1 } };

        _dataSet
            .Setup(mock => mock.CountFilterItemsByFilter(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filtersCounts);

        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();

        // Act
        var result = await _optimiser.IsCroppingRequired(query);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CropQuery_ExcessiveTimePeriods_ReturnsFewerTimePeriods()
    {
        // Arrange
        var filtersCounts = new Dictionary<Guid, int>() { { Guid.NewGuid(), 1 } };

        _dataSet
            .Setup(mock => mock.CountFilterItemsByFilter(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filtersCounts);

        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();

        // Act
        var result = await _optimiser.CropQuery(query, default);

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
        var filtersCounts = new Dictionary<Guid, int>() { { Guid.NewGuid(), 1 } };

        _dataSet
            .Setup(mock => mock.CountFilterItemsByFilter(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(filtersCounts);

        var locations = Enumerable.Range(0, 5).Select(_ => LocationMockBuilder.Build()).ToList();
        var query = new FullTableQueryBuilder()
            .WithLocationIds([.. locations.Select(l => l.Id)])
            .WithEndYear(2020)
            .Build();

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
}
