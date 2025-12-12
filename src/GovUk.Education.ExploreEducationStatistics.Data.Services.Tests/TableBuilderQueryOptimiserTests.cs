#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class TableBuilderQueryOptimiserTests
{
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly Mock<IFilterItemRepository> _filterItemRepository;
    private readonly IOptions<TableBuilderOptions> _options;
    private readonly TableBuilderQueryOptimiser _optimiser;

    public TableBuilderQueryOptimiserTests()
    {
        _statisticsDbContext = new StatisticsDbContext();
        _filterItemRepository = new Mock<IFilterItemRepository>(Strict);
        _options = new TableBuilderOptions { CroppedTableMaxRows = 5, MaxTableCellsAllowed = 20 }.ToOptionsWrapper();
        _optimiser = new TableBuilderQueryOptimiser(_statisticsDbContext, _filterItemRepository.Object, _options);
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

        _filterItemRepository
            .Setup(mock => mock.CountFilterItemsByFilter(It.IsAny<IEnumerable<Guid>>()))
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

        _filterItemRepository
            .Setup(mock => mock.CountFilterItemsByFilter(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(filtersCounts);

        var query = new FullTableQueryBuilder().WithEndYear(2020).Build();
        var optimiser = BuildTableBuilderQueryOptimiser(filterItemRepository: _filterItemRepository.Object);

        // Act
        var result = await optimiser.CropQuery(query, default);

        // Assert
        _filterItemRepository.Verify();

        Assert.NotNull(result.TimePeriod);
        Assert.Equal(2004, result.TimePeriod?.EndYear);
    }

    [Fact]
    public async Task CropQuery_ExcessiveLocations_ReturnsFewerLocations()
    {
        // Arrange
        var filtersCounts = new Dictionary<Guid, int>() { { Guid.NewGuid(), 1 } };

        _filterItemRepository
            .Setup(mock => mock.CountFilterItemsByFilter(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(filtersCounts);

        var locations = Enumerable.Range(0, 5).Select(_ => LocationMockBuilder.Build()).ToList();
        var query = new FullTableQueryBuilder()
            .WithLocationIds([.. locations.Select(l => l.Id)])
            .WithEndYear(2020)
            .Build();

        var contextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            statisticsDbContext.Location.AddRange(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var optimiser = BuildTableBuilderQueryOptimiser(
                statisticsDbContext,
                filterItemRepository: _filterItemRepository.Object
            );

            // Act
            var result = await optimiser.CropQuery(query, default);

            // Assert
            _filterItemRepository.Verify();

            Assert.NotNull(result.TimePeriod);
            Assert.Equal(2004, result.TimePeriod?.EndYear);
            Assert.Single(result.LocationIds);
        }
    }

    private static IOptions<TableBuilderOptions> DefaultTableBuilderOptions()
    {
        return new TableBuilderOptions { CroppedTableMaxRows = 5, MaxTableCellsAllowed = 20 }.ToOptionsWrapper();
    }

    private static TableBuilderQueryOptimiser BuildTableBuilderQueryOptimiser(
        StatisticsDbContext? statisticsDbContext = null,
        IFilterItemRepository? filterItemRepository = null,
        IOptions<TableBuilderOptions>? tableBuilderOptions = null
    )
    {
        return new(
            statisticsDbContext ?? StatisticsDbUtils.InMemoryStatisticsDbContext(),
            filterItemRepository ?? Mock.Of<IFilterItemRepository>(Strict),
            tableBuilderOptions ?? DefaultTableBuilderOptions()
        );
    }
}
