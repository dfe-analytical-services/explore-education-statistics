using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Mappings;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class TableBuilderSubjectMetaServiceTests
    {
        [Fact]
        public void GetSubjectMeta_SubjectIdNotFound()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var (boundaryLevelService, filterService, filterItemService, geoJsonService, indicatorGroupService, locationService, observationService, timePeriodService, userService) = Mocks();

                var service = new SubjectMetaService(boundaryLevelService.Object,
                    filterService.Object,
                    filterItemService.Object,
                    geoJsonService.Object,
                    indicatorGroupService.Object,
                    locationService.Object,
                    new Mock<ILogger<SubjectMetaService>>().Object,
                    observationService.Object,
                    new PersistenceHelper<StatisticsDbContext>(context),
                    timePeriodService.Object,
                    userService.Object);

                var result = service.GetSubjectMeta(Guid.NewGuid());
                Assert.True(result.Result.IsLeft);
                Assert.IsAssignableFrom<NotFoundResult>(result.Result.Left);
            }
        }

        [Fact]
        public void GetSubjectMeta_SubjectNoAccess()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                };

                var subject = new Subject
                {
                    Id = Guid.NewGuid()
                };

                var releaseSubjectLink = new ReleaseSubject
                {
                    ReleaseId = release.Id,
                    SubjectId = subject.Id
                };

                context.Add(release);
                context.Add(subject);
                context.Add(releaseSubjectLink);

                context.SaveChanges();

                var (boundaryLevelService,filterService, filterItemService, geoJsonService, indicatorGroupService, locationService, observationService, timePeriodService, userService) = Mocks();

                userService.Setup(s => s.MatchesPolicy(release, CanViewSubjectData)).ReturnsAsync(false);

                var service = new SubjectMetaService(boundaryLevelService.Object,
                    filterService.Object,
                    filterItemService.Object,
                    geoJsonService.Object,
                    indicatorGroupService.Object,
                    locationService.Object,
                    new Mock<ILogger<SubjectMetaService>>().Object,
                    observationService.Object,
                    new PersistenceHelper<StatisticsDbContext>(context),
                    timePeriodService.Object,
                    userService.Object);

                var result = service.GetSubjectMetaRestricted(subject.Id);
                Assert.True(result.Result.IsLeft);
                Assert.IsAssignableFrom<ForbidResult>(result.Result.Left);
            }
        }
        
        [Fact]
        public void GetSubjectMeta_EmptyModelReturnedForSubject()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                };

                var subject = new Subject
                {
                    Id = Guid.NewGuid()
                };

                var releaseSubjectLink = new ReleaseSubject
                {
                    ReleaseId = release.Id,
                    SubjectId = subject.Id
                };

                context.Add(release);
                context.Add(subject);
                context.Add(releaseSubjectLink);

                context.SaveChanges();

                var (boundaryLevelService,filterService, filterItemService, geoJsonService, indicatorGroupService, locationService, observationService, timePeriodService, userService) = Mocks();
                
                filterService.Setup(s => s.GetFiltersIncludingItems(subject.Id)).Returns(Enumerable.Empty<Filter>());
                indicatorGroupService.Setup(s => s.GetIndicatorGroups(subject.Id)).Returns(Enumerable.Empty<IndicatorGroup>());
                locationService.Setup(s => s.GetObservationalUnits(subject.Id)).Returns(new Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>>());
                timePeriodService.Setup(s => s.GetTimePeriods(subject.Id)).Returns(Enumerable.Empty<(int Year, TimeIdentifier TimeIdentifier)>());

                var service = new SubjectMetaService(boundaryLevelService.Object,
                    filterService.Object,
                    filterItemService.Object,
                    geoJsonService.Object,
                    indicatorGroupService.Object,
                    locationService.Object,
                    new Mock<ILogger<SubjectMetaService>>().Object,
                    observationService.Object,
                    new PersistenceHelper<StatisticsDbContext>(context),
                    timePeriodService.Object,
                    userService.Object);

                var result = service.GetSubjectMeta(subject.Id);
                Assert.True(result.Result.IsRight);
                Assert.IsAssignableFrom<SubjectMetaViewModel>(result.Result.Right);
            }
        }

        private static (Mock<IBoundaryLevelService>,
            Mock<IFilterService>,
            Mock<IFilterItemService>,
            Mock<IGeoJsonService>,
            Mock<IIndicatorGroupService>,
            Mock<ILocationService>,
            Mock<IObservationService>,
            Mock<ITimePeriodService>,
            Mock<IUserService>) Mocks()
        {
            return (new Mock<IBoundaryLevelService>(),
                new Mock<IFilterService>(),
                new Mock<IFilterItemService>(),
                new Mock<IGeoJsonService>(), 
                new Mock<IIndicatorGroupService>(),
                new Mock<ILocationService>(),
                new Mock<IObservationService>(),
                new Mock<ITimePeriodService>(),
                new Mock<IUserService>());
        }
    }
}
