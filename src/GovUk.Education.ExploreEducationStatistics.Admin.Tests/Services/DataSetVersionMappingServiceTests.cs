#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Moq.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetVersionMappingServiceTests
{
    private readonly Mock<PublicDataDbContext> _publicDataDbContextMock = new();

    [Theory]
    [InlineData(false, "AutoMapped", "AutoMapped", "AutoMapped", "AutoMapped", false)]
    [InlineData(true, "AutoMapped", "AutoMapped", "AutoMapped", "AutoMapped", true)]
    // ExpectedMajorVersion to be true because of AutoNone or ManualNone in filter mapping:
    [InlineData(false, "AutoNone", "AutoNone", "AutoNone", "AutoNone", true)]
    [InlineData(false, "AutoMapped", "AutoNone", "AutoNone", "AutoNone", true)]
    [InlineData(false, "AutoMapped", "AutoMapped", "AutoNone", "AutoNone", true)]
    [InlineData(false, "AutoMapped", "AutoMapped", "AutoMapped", "AutoNone", true)]
    [InlineData(false, "ManualNone", "ManualNone", "ManualNone", "ManualNone", true)]
    [InlineData(false, "AutoMapped", "ManualNone", "ManualNone", "ManualNone", true)]
    [InlineData(false, "AutoMapped", "AutoMapped", "ManualNone", "ManualNone", true)]
    public async Task IsMajorVersionUpdate(
        bool majorCausedByDeletion, 
        string locationMappingTypesLevel, 
        string locationMappingTypesOption, 
        string filterMappingTypesLevel,
        string filterMappingTypesOption,
        bool expectedMajorVersion)
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();

        var locationMappingTypes = new List<LocationMappingTypes>
        {
            new()
            {
                LocationLevelRaw = locationMappingTypesLevel,
                LocationOptionRaw = locationMappingTypesOption
            }
        }; 
        
        var filterMappingTypes = new List<FilterMappingTypes>
        {
            new()
            {
                FilterRaw = filterMappingTypesLevel,
                FilterOptionRaw = filterMappingTypesOption
            }
        };
        
        SetupDbContext(
            targetDataSetVersionId, 
            majorCausedByDeletion, 
            majorCausedByDeletion, 
            majorCausedByDeletion);

        var mockMappingTypesRepository = SetupMockMappingTypes(
            locationMappingTypesLevel, 
            locationMappingTypesOption, 
            filterMappingTypesLevel, 
            filterMappingTypesOption, 
            targetDataSetVersionId);

        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);
        
        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object, 
            contentDbContext: contentDbContext,
            mockMappingTypesRepository: mockMappingTypesRepository.Object);

        // Act
        var result = await service.IsMajorVersionUpdate(
            targetDataSetVersionId,
            locationMappingTypes,
            filterMappingTypes,
            CancellationToken.None);

        // Assert
        Assert.Equal(expectedMajorVersion, result);
    }
    
    [Theory]
    [InlineData("AutoNone", "AutoNone", "AutoNone", "AutoNone", false, false)]
    [InlineData("AutoMapped", "AutoNone", "AutoNone", "AutoNone", false, false)]
    [InlineData("AutoMapped", "AutoMapped", "AutoNone", "AutoNone", true, false)]
    [InlineData("AutoMapped", "AutoMapped", "AutoMapped", "AutoNone", true, false)]
    [InlineData("ManualNone", "ManualNone", "ManualNone", "ManualNone", false, false)]
    [InlineData("AutoMapped", "ManualNone", "ManualNone", "ManualNone", false, false)]
    [InlineData("AutoMapped", "AutoMapped", "ManualNone", "ManualNone", true, false)]
    [InlineData("ManualNone", "ManualNone", "AutoMapped", "AutoMapped",  false, true)]
    
    public async Task GetMappingStatus(
        string locationMappingTypesOption, 
        string locationMappingTypesLevel,
        string filterMappingTypesOption, 
        string filterMappingTypesLevel,
        bool majorVersionInLocations,
        bool majorVersionInFilters)
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();

        SetupDbContext(targetDataSetVersionId);

        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);
        
        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object, 
            contentDbContext: contentDbContext);

        SetupMockMappingTypes(locationMappingTypesLevel, locationMappingTypesOption, filterMappingTypesLevel,
            filterMappingTypesOption, targetDataSetVersionId);
        // Act

        var result = await service.GetMappingStatus(targetDataSetVersionId);
        // Assert
        Assert.Equal(majorVersionInLocations, result.FiltersHaveMajorChange);
        Assert.Equal(majorVersionInFilters, result.LocationsHaveMajorChange);
    }

    
    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(true, true, true, true)]
    public async Task HasDeletionChanges(bool deletedIndicators, bool deletedGeographicLevels, bool deletedTimePeriods, bool expected)
    {
        // Arrange
        var targetDataSetVersionId = Guid.NewGuid();
        SetupDbContext(
            targetDataSetVersionId, 
            deletedIndicators,
            deletedGeographicLevels,
            deletedTimePeriods);
        
        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);
        
        var service = CreateService(
            publicDataDbContext: _publicDataDbContextMock.Object, 
            contentDbContext: contentDbContext);
        
        // Act
        var result = await service.HasDeletionChanges(targetDataSetVersionId);

        // Assert
        Assert.Equal(expected, result);
    }
    
    private void SetupDbContext(
        Guid targetDataSetVersionId,
        bool hasDeletedIndicators = false,
        bool hasDeletedGeographicLevels = false,
        bool hasDeletedTimePeriods = false)
    {
        var data = new List<DataSetVersionMapping>
        {
            new()
            {
                TargetDataSetVersionId = targetDataSetVersionId,
                HasDeletedIndicators = hasDeletedIndicators,
                HasDeletedGeographicLevels = hasDeletedGeographicLevels,
                HasDeletedTimePeriods = hasDeletedTimePeriods,
                SourceDataSetVersionId = Guid.NewGuid()
            }
            
        };
        
        _publicDataDbContextMock
            .Setup(db => db.DataSetVersionMappings)
            .ReturnsDbSet(data);
    }

    private static Mock<IMappingTypesRepository> SetupMockMappingTypes(
        string locationMappingTypesLevel,
        string locationMappingTypesOption,
        string filterMappingTypesLevel,
        string filterMappingTypesOption,
        Guid targetDataSetVersionId)
    {
        var mockMappingTypesRepository = new Mock<IMappingTypesRepository>();
        
        mockMappingTypesRepository
            .Setup(m => m.GetLocationOptionMappingTypes(targetDataSetVersionId, It.IsAny<CancellationToken>())
            ).ReturnsAsync(new List<LocationMappingTypes>
            {
                new()
                {
                    LocationLevelRaw = locationMappingTypesLevel,
                    LocationOptionRaw = locationMappingTypesOption
                }
            }); 
        
        mockMappingTypesRepository
            .Setup(m => m.GetFilterOptionMappingTypes(targetDataSetVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FilterMappingTypes>
            {
                new()
                {
                    FilterRaw = filterMappingTypesLevel,
                    FilterOptionRaw = filterMappingTypesOption
                }
            });

        return mockMappingTypesRepository;
    }
    
    private DataSetVersionMappingService CreateService(
        PublicDataDbContext publicDataDbContext,
        ContentDbContext contentDbContext,
        IPostgreSqlRepository? mockPostgreSqlRepository = null,
        IUserService? mockUserService = null,
        IMappingTypesRepository? mockMappingTypesRepository = null)
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
