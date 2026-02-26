#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Moq;
using Moq.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetVersionMappingServiceTests
{
    private readonly Mock<PublicDataDbContext> _publicDataDbContextMock = new();

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(true, true, true, true)]
    public async Task IsMajorVersionUpdateDeletions(
        bool deletedLocations,
        bool deletedFilters,
        bool deletedIndicators,
        bool expectedMajorVersion
    )
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();

        SetupDbContext(targetDataSetVersionId, deletedLocations, deletedFilters, deletedIndicators);

        var mockMappingTypesRepository = SetupMockMappingTypes(targetDataSetVersionId: targetDataSetVersionId);

        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);

        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object,
            contentDbContext: contentDbContext,
            mockMappingTypesRepository: mockMappingTypesRepository.Object
        );

        // Act
        var result = await service.GetMajorChangesStatus(targetDataSetVersionId, [], [], [], CancellationToken.None);

        // Assert
        Assert.Equal(expectedMajorVersion, result.IsMajorVersionUpdate);
    }

    [Theory]
    [InlineData("AutoMapped", "AutoMapped", false)]
    [InlineData("AutoNone", "AutoNone", true)]
    [InlineData("AutoMapped", "AutoNone", true)]
    [InlineData("ManualNone", "ManualNone", true)]
    [InlineData("AutoMapped", "ManualNone", true)]
    public async Task IsMajorVersionUpdateLocations(
        string locationMappingTypesLevel,
        string locationMappingTypesOption,
        bool expectedMajorVersion
    )
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();

        var locationMappingTypes = new List<LocationMappingTypes>
        {
            new() { LocationLevelRaw = locationMappingTypesLevel, LocationOptionRaw = locationMappingTypesOption },
        };

        SetupDbContext(
            targetDataSetVersionId,
            hasDeletedIndicators: false,
            hasDeletedGeographicLevels: false,
            hasDeletedTimePeriods: false
        );

        var mockMappingTypesRepository = SetupMockMappingTypes(
            targetDataSetVersionId: targetDataSetVersionId,
            locationMappingTypesLevel: locationMappingTypesLevel,
            locationMappingTypesOption: locationMappingTypesOption
        );

        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);

        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object,
            contentDbContext: contentDbContext,
            mockMappingTypesRepository: mockMappingTypesRepository.Object
        );

        // Act
        var result = await service.GetMajorChangesStatus(
            dataSetVersionId: targetDataSetVersionId,
            locationMappingTypes: locationMappingTypes,
            filterMappingTypes: [],
            indicatorMappingTypes: [],
            CancellationToken.None
        );

        // Assert
        Assert.Equal(expectedMajorVersion, result.IsMajorVersionUpdate);
    }

    [Theory]
    [InlineData("AutoMapped", false)]
    [InlineData("ManualMapped", false)]
    [InlineData("AutoNone", true)]
    [InlineData("ManualNone", true)]
    public async Task IsMajorVersionUpdateIndicators(string indicatorMappingType, bool expectedMajorVersion)
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();

        var indicatorMappingTypes = new List<IndicatorMappingTypes> { new() { IndicatorRaw = indicatorMappingType } };

        SetupDbContext(
            targetDataSetVersionId,
            hasDeletedIndicators: false,
            hasDeletedGeographicLevels: false,
            hasDeletedTimePeriods: false
        );

        var mockMappingTypesRepository = SetupMockMappingTypes(
            targetDataSetVersionId: targetDataSetVersionId,
            indicatorMappingTypes: indicatorMappingType
        );

        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);

        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object,
            contentDbContext: contentDbContext,
            mockMappingTypesRepository: mockMappingTypesRepository.Object
        );

        // Act
        var result = await service.GetMajorChangesStatus(
            dataSetVersionId: targetDataSetVersionId,
            locationMappingTypes: [],
            filterMappingTypes: [],
            indicatorMappingTypes: indicatorMappingTypes,
            CancellationToken.None
        );

        // Assert
        Assert.Equal(expectedMajorVersion, result.IsMajorVersionUpdate);
    }

    [Theory]
    [InlineData("AutoNone", "AutoNone", true)]
    [InlineData("AutoMapped", "AutoNone", true)]
    [InlineData("ManualNone", "ManualNone", true)]
    [InlineData("AutoMapped", "ManualNone", true)]
    [InlineData("AutoMapped", "AutoMapped", false)]
    public async Task GetMappingStatusLocations(
        string locationMappingTypesOption,
        string locationMappingTypesLevel,
        bool majorVersionInLocations
    )
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();

        SetupDbContext(targetDataSetVersionId);

        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);

        var mockMappingTypesRepository = SetupMockMappingTypes(
            targetDataSetVersionId: targetDataSetVersionId,
            locationMappingTypesLevel: locationMappingTypesLevel,
            locationMappingTypesOption: locationMappingTypesOption
        );

        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object,
            contentDbContext: contentDbContext,
            mockMappingTypesRepository: mockMappingTypesRepository.Object
        );

        // Act
        var result = await service.GetMappingStatus(targetDataSetVersionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(majorVersionInLocations, result.LocationsHaveMajorChange);
        Assert.False(result.FiltersHaveMajorChange);
        Assert.False(result.IndicatorsHaveMajorChange);
    }

    [Theory]
    [InlineData("AutoMapped", "AutoMapped", false)]
    [InlineData("AutoNone", "AutoNone", true)]
    [InlineData("AutoMapped", "AutoNone", true)]
    [InlineData("ManualNone", "ManualNone", true)]
    public async Task GetMappingStatusFilters(
        string filterMappingTypesOption,
        string filterMappingTypesFilter,
        bool majorVersionInFilters
    )
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();

        SetupDbContext(targetDataSetVersionId);

        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);

        var mockMappingTypesRepository = SetupMockMappingTypes(
            targetDataSetVersionId: targetDataSetVersionId,
            filterMappingTypesFilter: filterMappingTypesFilter,
            filterMappingTypesOption: filterMappingTypesOption
        );

        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object,
            contentDbContext: contentDbContext,
            mockMappingTypesRepository: mockMappingTypesRepository.Object
        );

        // Act
        var result = await service.GetMappingStatus(targetDataSetVersionId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.LocationsHaveMajorChange);
        Assert.Equal(majorVersionInFilters, result.FiltersHaveMajorChange);
        Assert.False(result.IndicatorsHaveMajorChange);
    }

    [Theory]
    [InlineData("AutoMapped", false)]
    [InlineData("ManualMapped", false)]
    [InlineData("AutoNone", true)]
    [InlineData("ManualNone", true)]
    public async Task GetMappingStatusIndicators(string indicatorMappingTypes, bool majorVersionInIndicators)
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();

        SetupDbContext(targetDataSetVersionId);

        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);

        var mockMappingTypesRepository = SetupMockMappingTypes(
            targetDataSetVersionId: targetDataSetVersionId,
            indicatorMappingTypes: indicatorMappingTypes
        );

        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object,
            contentDbContext: contentDbContext,
            mockMappingTypesRepository: mockMappingTypesRepository.Object
        );

        // Act

        var result = await service.GetMappingStatus(targetDataSetVersionId);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.LocationsHaveMajorChange);
        Assert.False(result.FiltersHaveMajorChange);
        Assert.Equal(majorVersionInIndicators, result.IndicatorsHaveMajorChange);
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(true, true, true, true)]
    public async Task HasDeletionChanges(
        bool deletedIndicators,
        bool deletedGeographicLevels,
        bool deletedTimePeriods,
        bool expected
    )
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();
        SetupDbContext(targetDataSetVersionId, deletedIndicators, deletedGeographicLevels, deletedTimePeriods);

        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);

        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object,
            contentDbContext: contentDbContext
        );

        // Act
        var result = await service.HasDeletionChanges(targetDataSetVersionId);

        // Assert
        Assert.Equal(expected, result);
    }

    private void SetupDbContext(
        Guid targetDataSetVersionId,
        bool hasDeletedIndicators = false,
        bool hasDeletedGeographicLevels = false,
        bool hasDeletedTimePeriods = false
    )
    {
        var data = new List<DataSetVersionMapping>
        {
            new()
            {
                TargetDataSetVersionId = targetDataSetVersionId,
                HasDeletedIndicators = hasDeletedIndicators,
                HasDeletedGeographicLevels = hasDeletedGeographicLevels,
                HasDeletedTimePeriods = hasDeletedTimePeriods,
                SourceDataSetVersionId = Guid.NewGuid(),
            },
        };

        _publicDataDbContextMock.Setup(db => db.DataSetVersionMappings).ReturnsDbSet(data);
    }

    private static Mock<IMappingTypesRepository> SetupMockMappingTypes(
        Guid targetDataSetVersionId,
        string? locationMappingTypesLevel = "AutoMapped",
        string? locationMappingTypesOption = "AutoMapped",
        string? filterMappingTypesFilter = "AutoMapped",
        string? filterMappingTypesOption = "AutoMapped",
        string? indicatorMappingTypes = "AutoMapped"
    )
    {
        var mockMappingTypesRepository = new Mock<IMappingTypesRepository>();

        mockMappingTypesRepository
            .Setup(m => m.GetLocationOptionMappingTypes(targetDataSetVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<LocationMappingTypes>
                {
                    new()
                    {
                        LocationLevelRaw = locationMappingTypesLevel,
                        LocationOptionRaw = locationMappingTypesOption,
                    },
                }
            );

        mockMappingTypesRepository
            .Setup(m => m.GetFilterOptionMappingTypes(targetDataSetVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<FilterMappingTypes>
                {
                    new() { FilterRaw = filterMappingTypesFilter, FilterOptionRaw = filterMappingTypesOption },
                }
            );

        mockMappingTypesRepository
            .Setup(m => m.GetIndicatorMappingTypes(targetDataSetVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<IndicatorMappingTypes> { new() { IndicatorRaw = indicatorMappingTypes } });

        return mockMappingTypesRepository;
    }

    private DataSetVersionMappingService CreateService(
        PublicDataDbContext publicDataDbContext,
        ContentDbContext contentDbContext,
        IPostgreSqlRepository? mockPostgreSqlRepository = null,
        IUserService? mockUserService = null,
        IMappingTypesRepository? mockMappingTypesRepository = null
    )
    {
        return new DataSetVersionMappingService(
            mockPostgreSqlRepository ?? Mock.Of<IPostgreSqlRepository>(behavior: MockBehavior.Strict),
            mockUserService ?? Mock.Of<IUserService>(behavior: MockBehavior.Strict),
            publicDataDbContext,
            contentDbContext,
            mockMappingTypesRepository ?? Mock.Of<IMappingTypesRepository>(behavior: MockBehavior.Strict)
        );
    }
}
