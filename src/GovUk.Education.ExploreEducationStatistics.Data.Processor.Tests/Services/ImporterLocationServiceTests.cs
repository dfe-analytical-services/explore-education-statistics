#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services
{
    public class ImporterLocationServiceTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Country _wales = new("W92000004", "Wales");
        
        [Fact]
        public async Task Get()
        {
            var location = new Location
            {
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(importerLocationCache: importerLocationCache);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Add(location);
                await statisticsDbContext.SaveChangesAsync();
                
                // Fill the ImporterLocationCache with all existing Locations on "startup" of the Importer.
                // Note that this occurs in Startup.cs.
                importerLocationCache.LoadLocations(statisticsDbContext);
            }

            var result = service.Get(location);

            Assert.NotNull(result);
            Assert.Equal(location.Id, result!.Id);
        }
        
        [Fact]
        public async Task Get_NonExistingLocation()
        {
            var nonMatchingLocation = new Location
            {
                GeographicLevel = GeographicLevel.Institution,
                Country = _england
            };

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(importerLocationCache: importerLocationCache);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.Add(nonMatchingLocation);
                await statisticsDbContext.SaveChangesAsync();
                
                // Fill the ImporterLocationCache with all existing Locations on "startup" of the Importer.
                // Note that this occurs in Startup.cs.
                importerLocationCache.LoadLocations(statisticsDbContext);
            }

            Assert.Throws<KeyNotFoundException>(() => service.Get(
                new Location
                {
                    GeographicLevel = GeographicLevel.Country, // different level
                    Country = nonMatchingLocation.Country   
                }));
        }

        [Fact]
        public async Task CreateAndCache_NewLocation()
        {
            var locationId = Guid.NewGuid();

            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.Setup(mock => mock.NewGuid())
                .Returns(locationId);

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(
                guidGenerator.Object,
                importerLocationCache);

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var result = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location
                    {
                        GeographicLevel = GeographicLevel.Country,
                        Country = _england
                    });

                MockUtils.VerifyAllMocks(guidGenerator);

                await statisticsDbContext.SaveChangesAsync();

                Assert.Equal(locationId, result.Id);
                Assert.Equal(GeographicLevel.Country, result.GeographicLevel);
                Assert.NotNull(result.Country);
                Assert.Equal(_england.Code, result.Country!.Code);
                Assert.Equal(_england.Name, result.Country!.Name);
                
                Assert.Same(result, importerLocationCache.Get(result));
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var location = Assert.Single(statisticsDbContext.Location);

                Assert.NotNull(location);
                Assert.Equal(locationId, location!.Id);
                Assert.Equal(GeographicLevel.Country, location.GeographicLevel);
                Assert.NotNull(location.Country);
                Assert.Equal(_england.Code, location.Country!.Code);
                Assert.Equal(_england.Name, location.Country!.Name);
            }
        }
        
        [Fact]
        public async Task CreateAndCache_MissingLocalAuthorityCode()
        {
            var locationId = Guid.NewGuid();
        
            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.Setup(mock => mock.NewGuid())
                .Returns(locationId);

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(
                guidGenerator.Object,
                importerLocationCache);
        
            var statisticsDbContextId = Guid.NewGuid().ToString();
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Test creating a Local Authority with no code, e.g. Bedfordshire pre LGR 2009 only has an old code
                var result = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.LocalAuthority,
                        Country = _england,
                        LocalAuthority = new LocalAuthority(null, "820", "Bedfordshire")
                    });
        
                MockUtils.VerifyAllMocks(guidGenerator);
        
                await statisticsDbContext.SaveChangesAsync();
        
                Assert.Equal(locationId, result.Id);
                Assert.Equal(GeographicLevel.LocalAuthority, result.GeographicLevel);
                Assert.NotNull(result.Country);
                Assert.Equal(_england.Code, result.Country!.Code);
                Assert.Equal(_england.Name, result.Country!.Name);
                Assert.NotNull(result.LocalAuthority);
                Assert.Null(result.LocalAuthority!.Code);
                Assert.Equal("820", result.LocalAuthority!.OldCode);
                Assert.Equal("Bedfordshire", result.LocalAuthority!.Name);
                
                Assert.Same(result, importerLocationCache.Get(result));
            }
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var location = Assert.Single(statisticsDbContext.Location);
        
                Assert.NotNull(location);
                Assert.Equal(locationId, location!.Id);
                Assert.Equal(GeographicLevel.LocalAuthority, location.GeographicLevel);
                Assert.NotNull(location.Country);
                Assert.Equal(_england.Code, location.Country!.Code);
                Assert.Equal(_england.Name, location.Country!.Name);
                Assert.NotNull(location.LocalAuthority);
                Assert.Null(location.LocalAuthority!.Code);
                Assert.Equal("820", location.LocalAuthority!.OldCode);
                Assert.Equal("Bedfordshire", location.LocalAuthority!.Name);
            }
        }
        
        [Fact]
        public async Task CreateAndCache_UniqueByName()
        {
            var result1Id = Guid.NewGuid();
            var result2Id = Guid.NewGuid();
        
            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.SetupSequence(mock => mock.NewGuid())
                .Returns(result1Id)
                .Returns(result2Id);

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(
                guidGenerator.Object,
                importerLocationCache);
        
            var statisticsDbContextId = Guid.NewGuid().ToString();
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Test creating two Regions with the same code but different names
                var result1 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = new Region("1", "North East")
                    });
        
                var result2 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = new Region("1", "North West")
                    });
        
                MockUtils.VerifyAllMocks(guidGenerator);
        
                await statisticsDbContext.SaveChangesAsync();
        
                Assert.Equal(result1Id, result1.Id);
                Assert.Equal(GeographicLevel.Region, result1.GeographicLevel);
                Assert.NotNull(result1.Country);
                Assert.Equal(_england.Code, result1.Country!.Code);
                Assert.Equal(_england.Name, result1.Country!.Name);
                Assert.NotNull(result1.Region);
                Assert.Equal("1", result1.Region!.Code);
                Assert.Equal("North East", result1.Region!.Name);
        
                Assert.Equal(result2Id, result2.Id);
                Assert.Equal(GeographicLevel.Region, result2.GeographicLevel);
                Assert.NotNull(result2.Country);
                Assert.Equal(_england.Code, result2.Country!.Code);
                Assert.Equal(_england.Name, result2.Country!.Name);
                Assert.NotNull(result2.Region);
                Assert.Equal("1", result2.Region!.Code);
                Assert.Equal("North West", result2.Region!.Name);
                
                Assert.Same(result1, importerLocationCache.Get(result1));
                Assert.Same(result2, importerLocationCache.Get(result2));
            }
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var locations = await statisticsDbContext.Location.ToListAsync();
        
                Assert.Equal(2, locations.Count);
        
                Assert.Equal(result1Id, locations[0].Id);
                Assert.Equal(GeographicLevel.Region, locations[0].GeographicLevel);
                Assert.NotNull(locations[0].Country);
                Assert.Equal(_england.Code, locations[0].Country!.Code);
                Assert.Equal(_england.Name, locations[0].Country!.Name);
                Assert.NotNull(locations[0].Region);
                Assert.Equal("1", locations[0].Region!.Code);
                Assert.Equal("North East", locations[0].Region!.Name);
        
                Assert.Equal(result2Id, locations[1].Id);
                Assert.Equal(GeographicLevel.Region, locations[1].GeographicLevel);
                Assert.NotNull(locations[1].Country);
                Assert.Equal(_england.Code, locations[1].Country!.Code);
                Assert.Equal(_england.Name, locations[1].Country!.Name);
                Assert.NotNull(locations[1].Region);
                Assert.Equal("1", locations[1].Region!.Code);
                Assert.Equal("North West", locations[1].Region!.Name);
            }
        }
        
        [Fact]
        public async Task CreateAndCache_UniqueByCode()
        {
            var result1Id = Guid.NewGuid();
            var result2Id = Guid.NewGuid();
        
            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.SetupSequence(mock => mock.NewGuid())
                .Returns(result1Id)
                .Returns(result2Id);

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(
                guidGenerator.Object,
                importerLocationCache);
        
            var statisticsDbContextId = Guid.NewGuid().ToString();
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Test creating two Regions with the same name but different codes
                var result1 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = new Region("1", "North East")
                    });
        
                var result2 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.Region,
                        Country = _england,
                        Region = new Region("2", "North East")
                    });
        
                MockUtils.VerifyAllMocks(guidGenerator);
        
                await statisticsDbContext.SaveChangesAsync();
        
                Assert.Equal(result1Id, result1.Id);
                Assert.Equal(GeographicLevel.Region, result1.GeographicLevel);
                Assert.NotNull(result1.Country);
                Assert.Equal(_england.Code, result1.Country!.Code);
                Assert.Equal(_england.Name, result1.Country!.Name);
                Assert.NotNull(result1.Region);
                Assert.Equal("1", result1.Region!.Code);
                Assert.Equal("North East", result1.Region!.Name);
        
                Assert.Equal(result2Id, result2.Id);
                Assert.Equal(GeographicLevel.Region, result2.GeographicLevel);
                Assert.NotNull(result2.Country);
                Assert.Equal(_england.Code, result2.Country!.Code);
                Assert.Equal(_england.Name, result2.Country!.Name);
                Assert.NotNull(result2.Region);
                Assert.Equal("2", result2.Region!.Code);
                Assert.Equal("North East", result2.Region!.Name);
                
                Assert.Same(result1, importerLocationCache.Get(result1));
                Assert.Same(result2, importerLocationCache.Get(result2));
            }
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var locations = await statisticsDbContext.Location.ToListAsync();
        
                Assert.Equal(2, locations.Count);
        
                Assert.Equal(result1Id, locations[0].Id);
                Assert.Equal(GeographicLevel.Region, locations[0].GeographicLevel);
                Assert.NotNull(locations[0].Country);
                Assert.Equal(_england.Code, locations[0].Country!.Code);
                Assert.Equal(_england.Name, locations[0].Country!.Name);
                Assert.NotNull(locations[0].Region);
                Assert.Equal("1", locations[0].Region!.Code);
                Assert.Equal("North East", locations[0].Region!.Name);
        
                Assert.Equal(result2Id, locations[1].Id);
                Assert.Equal(GeographicLevel.Region, locations[1].GeographicLevel);
                Assert.NotNull(locations[1].Country);
                Assert.Equal(_england.Code, locations[1].Country!.Code);
                Assert.Equal(_england.Name, locations[1].Country!.Name);
                Assert.NotNull(locations[1].Region);
                Assert.Equal("2", locations[1].Region!.Code);
                Assert.Equal("North East", locations[1].Region!.Name);
            }
        }
        
        [Fact]
        public async Task CreateAndCache_UniqueByLocalAuthorityOldCode()
        {
            var result1Id = Guid.NewGuid();
            var result2Id = Guid.NewGuid();
        
            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.SetupSequence(mock => mock.NewGuid())
                .Returns(result1Id)
                .Returns(result2Id);

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(
                guidGenerator.Object,
                importerLocationCache);
        
            var statisticsDbContextId = Guid.NewGuid().ToString();
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Test creating two Local Authorities which are identical except for different old codes
                var result1 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.LocalAuthority,
                        Country = _england,
                        LocalAuthority = new LocalAuthority("1", "100", "Westminster")
                    });
        
                var result2 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.LocalAuthority,
                        Country = _england,
                        LocalAuthority = new LocalAuthority("1", "101", "Westminster")
                    });
        
                MockUtils.VerifyAllMocks(guidGenerator);
        
                await statisticsDbContext.SaveChangesAsync();
        
                Assert.Equal(result1Id, result1.Id);
                Assert.Equal(GeographicLevel.LocalAuthority, result1.GeographicLevel);
                Assert.NotNull(result1.Country);
                Assert.Equal(_england.Code, result1.Country!.Code);
                Assert.Equal(_england.Name, result1.Country!.Name);
                Assert.NotNull(result1.LocalAuthority);
                Assert.Equal("1", result1.LocalAuthority!.Code);
                Assert.Equal("100", result1.LocalAuthority!.OldCode);
                Assert.Equal("Westminster", result1.LocalAuthority!.Name);
        
                Assert.Equal(result2Id, result2.Id);
                Assert.Equal(GeographicLevel.LocalAuthority, result2.GeographicLevel);
                Assert.NotNull(result2.Country);
                Assert.Equal(_england.Code, result2.Country!.Code);
                Assert.Equal(_england.Name, result2.Country!.Name);
                Assert.NotNull(result2.LocalAuthority);
                Assert.Equal("1", result2.LocalAuthority!.Code);
                Assert.Equal("101", result2.LocalAuthority!.OldCode);
                Assert.Equal("Westminster", result2.LocalAuthority!.Name);
                
                Assert.Same(result1, importerLocationCache.Get(result1));
                Assert.Same(result2, importerLocationCache.Get(result2));
            }
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var locations = await statisticsDbContext.Location.ToListAsync();
        
                Assert.Equal(2, locations.Count);
        
                Assert.Equal(result1Id, locations[0].Id);
                Assert.Equal(GeographicLevel.LocalAuthority, locations[0].GeographicLevel);
                Assert.NotNull(locations[0].Country);
                Assert.Equal(_england.Code, locations[0].Country!.Code);
                Assert.Equal(_england.Name, locations[0].Country!.Name);
                Assert.NotNull(locations[0].LocalAuthority);
                Assert.Equal("1", locations[0].LocalAuthority!.Code);
                Assert.Equal("100", locations[0].LocalAuthority!.OldCode);
                Assert.Equal("Westminster", locations[0].LocalAuthority!.Name);
        
                Assert.Equal(result2Id, locations[1].Id);
                Assert.Equal(GeographicLevel.LocalAuthority, locations[1].GeographicLevel);
                Assert.NotNull(locations[1].Country);
                Assert.Equal(_england.Code, locations[1].Country!.Code);
                Assert.Equal(_england.Name, locations[1].Country!.Name);
                Assert.NotNull(locations[1].LocalAuthority);
                Assert.Equal("1", locations[1].LocalAuthority!.Code);
                Assert.Equal("101", locations[1].LocalAuthority!.OldCode);
                Assert.Equal("Westminster", locations[1].LocalAuthority!.Name);
            }
        }
        
        [Fact]
        public async Task CreateAndCache_UniqueByGeographicLevel()
        {
            var result1Id = Guid.NewGuid();
            var result2Id = Guid.NewGuid();
        
            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.SetupSequence(mock => mock.NewGuid())
                .Returns(result1Id)
                .Returns(result2Id);

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(
                guidGenerator.Object,
                importerLocationCache);
        
            var statisticsDbContextId = Guid.NewGuid().ToString();
        
            var localAuthority = new LocalAuthority("1", "100", "Westminster");
            var localAuthorityDistrict = new LocalAuthorityDistrict("2", "Westminster");
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Test creating two locations with identical attributes except for GeographicLevel.
                // This is probably bad data if it's ever supplied but nevertheless they should be separate locations
                // treated uniquely by geographic level.
                var result1 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.LocalAuthority,
                        Country = _england,
                        LocalAuthority = localAuthority,
                        LocalAuthorityDistrict = localAuthorityDistrict
                    });
        
                var result2 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.LocalAuthorityDistrict,
                        Country = _england,
                        LocalAuthority = localAuthority,
                        LocalAuthorityDistrict = localAuthorityDistrict
                    });
        
                MockUtils.VerifyAllMocks(guidGenerator);
        
                await statisticsDbContext.SaveChangesAsync();
        
                Assert.Equal(result1Id, result1.Id);
                Assert.Equal(GeographicLevel.LocalAuthority, result1.GeographicLevel);
                Assert.NotNull(result1.Country);
                Assert.Equal(_england.Code, result1.Country!.Code);
                Assert.Equal(_england.Name, result1.Country!.Name);
                Assert.NotNull(result1.LocalAuthority);
                Assert.Equal(localAuthority.Code, result1.LocalAuthority!.Code);
                Assert.Equal(localAuthority.Name, result1.LocalAuthority!.Name);
                Assert.NotNull(result1.LocalAuthorityDistrict);
                Assert.Equal(localAuthorityDistrict.Code, result1.LocalAuthorityDistrict!.Code);
                Assert.Equal(localAuthorityDistrict.Name, result1.LocalAuthorityDistrict!.Name);
        
                Assert.Equal(result2Id, result2.Id);
                Assert.Equal(GeographicLevel.LocalAuthorityDistrict, result2.GeographicLevel);
                Assert.NotNull(result2.Country);
                Assert.Equal(_england.Code, result2.Country!.Code);
                Assert.Equal(_england.Name, result2.Country!.Name);
                Assert.NotNull(result2.LocalAuthority);
                Assert.Equal(localAuthority.Code, result2.LocalAuthority!.Code);
                Assert.Equal(localAuthority.Name, result2.LocalAuthority!.Name);
                Assert.NotNull(result2.LocalAuthorityDistrict);
                Assert.Equal(localAuthorityDistrict.Code, result2.LocalAuthorityDistrict!.Code);
                Assert.Equal(localAuthorityDistrict.Name, result2.LocalAuthorityDistrict!.Name);
                
                Assert.Same(result1, importerLocationCache.Get(result1));
                Assert.Same(result2, importerLocationCache.Get(result2));
            }
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var locations = await statisticsDbContext.Location.ToListAsync();
        
                Assert.Equal(2, locations.Count);
        
                Assert.Equal(result1Id, locations[0].Id);
                Assert.Equal(GeographicLevel.LocalAuthority, locations[0].GeographicLevel);
                Assert.NotNull(locations[0].Country);
                Assert.Equal(_england.Code, locations[0].Country!.Code);
                Assert.Equal(_england.Name, locations[0].Country!.Name);
                Assert.NotNull(locations[0].LocalAuthority);
                Assert.Equal(localAuthority.Code, locations[0].LocalAuthority!.Code);
                Assert.Equal(localAuthority.Name, locations[0].LocalAuthority!.Name);
                Assert.NotNull(locations[0].LocalAuthorityDistrict);
                Assert.Equal(localAuthorityDistrict.Code, locations[0].LocalAuthorityDistrict!.Code);
                Assert.Equal(localAuthorityDistrict.Name, locations[0].LocalAuthorityDistrict!.Name);
        
                Assert.Equal(result2Id, locations[1].Id);
                Assert.Equal(GeographicLevel.LocalAuthorityDistrict, locations[1].GeographicLevel);
                Assert.NotNull(locations[1].Country);
                Assert.Equal(_england.Code, locations[1].Country!.Code);
                Assert.Equal(_england.Name, locations[1].Country!.Name);
                Assert.NotNull(locations[1].LocalAuthority);
                Assert.Equal(localAuthority.Code, locations[1].LocalAuthority!.Code);
                Assert.Equal(localAuthority.Name, locations[1].LocalAuthority!.Name);
                Assert.NotNull(locations[1].LocalAuthorityDistrict);
                Assert.Equal(localAuthorityDistrict.Code, locations[1].LocalAuthorityDistrict!.Code);
                Assert.Equal(localAuthorityDistrict.Name, locations[1].LocalAuthorityDistrict!.Name);
            }
        }
        
        [Fact]
        public async Task CreateAndCache_UniqueByAdditionalAttributes()
        {
            var result1Id = Guid.NewGuid();
            var result2Id = Guid.NewGuid();
        
            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.SetupSequence(mock => mock.NewGuid())
                .Returns(result1Id)
                .Returns(result2Id);

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(
                guidGenerator.Object,
                importerLocationCache);
        
            var statisticsDbContextId = Guid.NewGuid().ToString();
        
            var provider = new Provider("1", "Ace Teachers");
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Test creating two Providers that are unique by some other attribute e.g. Country
                var result1 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.Provider,
                        Country = _england,
                        Provider = provider
                    });
        
                var result2 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.Provider,
                        Country = _wales,
                        Provider = provider
                    });
        
                MockUtils.VerifyAllMocks(guidGenerator);
        
                await statisticsDbContext.SaveChangesAsync();
        
                Assert.Equal(result1Id, result1.Id);
                Assert.Equal(GeographicLevel.Provider, result1.GeographicLevel);
                Assert.NotNull(result1.Country);
                Assert.Equal(_england.Code, result1.Country!.Code);
                Assert.Equal(_england.Name, result1.Country!.Name);
                Assert.NotNull(result1.Provider);
                Assert.Equal(provider.Code, result1.Provider!.Code);
                Assert.Equal(provider.Name, result1.Provider!.Name);
        
                Assert.Equal(result2Id, result2.Id);
                Assert.Equal(GeographicLevel.Provider, result2.GeographicLevel);
                Assert.NotNull(result2.Country);
                Assert.Equal(_wales.Code, result2.Country!.Code);
                Assert.Equal(_wales.Name, result2.Country!.Name);
                Assert.NotNull(result2.Provider);
                Assert.Equal(provider.Code, result2.Provider!.Code);
                Assert.Equal(provider.Name, result2.Provider!.Name);
                
                Assert.Same(result1, importerLocationCache.Get(result1));
                Assert.Same(result2, importerLocationCache.Get(result2));
            }
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var locations = await statisticsDbContext.Location.ToListAsync();
        
                Assert.Equal(2, locations.Count);
        
                Assert.Equal(result1Id, locations[0].Id);
                Assert.Equal(GeographicLevel.Provider, locations[0].GeographicLevel);
                Assert.NotNull(locations[0].Country);
                Assert.Equal(_england.Code, locations[0].Country!.Code);
                Assert.Equal(_england.Name, locations[0].Country!.Name);
                Assert.NotNull(locations[0].Provider);
                Assert.Equal(provider.Code, locations[0].Provider!.Code);
                Assert.Equal(provider.Name, locations[0].Provider!.Name);
        
                Assert.Equal(result2Id, locations[1].Id);
                Assert.Equal(GeographicLevel.Provider, locations[1].GeographicLevel);
                Assert.NotNull(locations[1].Country);
                Assert.Equal(_wales.Code, locations[1].Country!.Code);
                Assert.Equal(_wales.Name, locations[1].Country!.Name);
                Assert.NotNull(locations[1].Provider);
                Assert.Equal(provider.Code, locations[1].Provider!.Code);
                Assert.Equal(provider.Name, locations[1].Provider!.Name);
            }
        }
        
        [Fact]
        public async Task CreateAndCache_CaseSensitiveLocationName()
        {
            var result1Id = Guid.NewGuid();
            var result2Id = Guid.NewGuid();
        
            var guidGenerator = new Mock<IGuidGenerator>();
            guidGenerator.SetupSequence(mock => mock.NewGuid())
                .Returns(result1Id)
                .Returns(result2Id);

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            var service = BuildService(
                guidGenerator.Object,
                importerLocationCache);
        
            var statisticsDbContextId = Guid.NewGuid().ToString();
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var result1 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.EnglishDevolvedArea,
                        Country = _england,
                        EnglishDevolvedArea = new EnglishDevolvedArea("1", "Sheffield City Region")
                    });
                
                await statisticsDbContext.SaveChangesAsync();
        
                Assert.Equal(result1Id, result1.Id);
                Assert.Equal("Sheffield City Region", result1.EnglishDevolvedArea_Name);
                
                Assert.Same(result1, importerLocationCache.Get(result1));
            }
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var result2 = await service.CreateIfNotExistsAndCache(
                    statisticsDbContext,
                    new Location {
                        GeographicLevel = GeographicLevel.EnglishDevolvedArea,
                        Country = _england,
                        EnglishDevolvedArea = new EnglishDevolvedArea("1", "SHEFFIELD CITY REGION")
                    });
                
                await statisticsDbContext.SaveChangesAsync();
        
                Assert.Equal(result2Id, result2.Id);
                // NOTE: The below assert doesn't actual catch the error fixed by EES-2427 because the
                // in-memory DB in the EF version we're currently using is case sensitive - while a
                // MsSQL DBs are case insensitive by default: https://github.com/dotnet/efcore/issues/6153
                Assert.Equal("SHEFFIELD CITY REGION", result2.EnglishDevolvedArea_Name);
                
                Assert.Same(result2, importerLocationCache.Get(result2));
            }
        
            MockUtils.VerifyAllMocks(guidGenerator);
        
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var locations = await statisticsDbContext.Location.ToListAsync();
        
                Assert.Equal(2, locations.Count);
        
                Assert.Equal(result1Id, locations[0].Id);
                Assert.Equal("Sheffield City Region", locations[0].EnglishDevolvedArea!.Name);
        
                Assert.Equal(result2Id, locations[1].Id);
                Assert.Equal("SHEFFIELD CITY REGION", locations[1].EnglishDevolvedArea!.Name);
            }
        }

        private ImporterLocationService BuildService(
            IGuidGenerator? guidGenerator = null,
            IImporterLocationCache? importerLocationCache = null)
        {
            return new(
                guidGenerator ?? Mock.Of<IGuidGenerator>(),
                importerLocationCache ?? Mock.Of<IImporterLocationCache>(Strict),
                Mock.Of<ILogger<ImporterLocationCache>>());
        }
    }
}
