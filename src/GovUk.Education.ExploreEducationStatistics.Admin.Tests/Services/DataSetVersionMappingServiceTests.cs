#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetVersionMappingServiceTests
{
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
        // Setup mock for GetLocationOptionMappingTypes
        var locationTypes = new List<LocationMappingTypes>
        {
            new()
            {
                LocationLevelRaw = locationMappingTypesLevel,
                LocationOptionRaw = locationMappingTypesOption
            }
        };

        // Setup mock for GetFilterAndOptionMappingTypes
        var filterTypes = new List<FilterMappingTypes>
        {
            new()
            {
                FilterRaw = filterMappingTypesLevel,
                FilterOptionRaw = filterMappingTypesOption
            }
        };


        var mockDbSet = CreateMockDbSet(new DataSetVersionMapping
        {
            SourceDataSetVersionId = Guid.NewGuid(),
            TargetDataSetVersionId = targetDataSetVersionId
        });

        var mockDbContext = new Mock<PublicDataDbContext>();
        
        mockDbContext.Setup(db => db.DataSetVersionMappings).Returns(mockDbSet.Object);
        
        var mockMappingTypesRepository = new Mock<IMappingTypesRepository>();
        mockMappingTypesRepository
            .Setup(m => m.GetLocationOptionMappingTypes(targetDataSetVersionId, It.IsAny<CancellationToken>())
            ).ReturnsAsync(locationTypes);
        mockMappingTypesRepository
            .Setup(m => m.GetFilterOptionMappingTypes(targetDataSetVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(filterTypes);
        mockMappingTypesRepository
            .Setup(m => m.HasDeletionMajorVersionChangesAsync(targetDataSetVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(majorCausedByDeletion);
        
        var contextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryApplicationDbContext(contextId);
        
        var service = CreateService(
            publicDataDbContext: mockDbContext.Object, 
            contentDbContext: contentDbContext,
            mockMappingTypesRepository: mockMappingTypesRepository.Object);

        // Act
        var result = await service.IsMajorVersionUpdate(
            targetDataSetVersionId,
            CancellationToken.None);

        // Assert
        Assert.Equal(expectedMajorVersion, result);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(params T[] elements) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        var queryable = elements.AsQueryable();
        
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        
        return mockSet;
    }

    private DataSetVersionMappingService CreateService(
        PublicDataDbContext publicDataDbContext,
        ContentDbContext contentDbContext,
        IPostgreSqlRepository? mockPostgreSqlRepository = null,
        IUserService? mockUserService = null,
        IMappingTypesRepository? mockMappingTypesRepository = null,
        IOptions<FeatureFlags>? mockFeatureFlags = null)
    {
        return new DataSetVersionMappingService(
            mockPostgreSqlRepository ?? Mock.Of<IPostgreSqlRepository>(behavior: MockBehavior.Strict),
            mockUserService ?? Mock.Of<IUserService>(behavior: MockBehavior.Strict),
            publicDataDbContext,
            contentDbContext,
            mockMappingTypesRepository ?? Mock.Of<IMappingTypesRepository>(behavior: MockBehavior.Strict),
            mockFeatureFlags ?? Microsoft.Extensions.Options.Options.Create(new FeatureFlags
            {
                EnableReplacementOfPublicApiDataSets = false
            }));
    }
}
