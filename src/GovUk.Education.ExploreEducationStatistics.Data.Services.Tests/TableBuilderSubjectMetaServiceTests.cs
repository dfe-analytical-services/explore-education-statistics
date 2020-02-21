using System;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
                var (filterService, filterItemService, indicatorGroupService, locationService,
                    observationService, releaseService, timePeriodService) = Mocks();

                var subjectService = new SubjectService(context, new Mock<ILogger<SubjectService>>().Object,
                    releaseService.Object);

                var service = new TableBuilderSubjectMetaService(filterService.Object,
                    filterItemService.Object,
                    indicatorGroupService.Object,
                    locationService.Object,
                    new Mock<ILogger<TableBuilderSubjectMetaService>>().Object,
                    MapperUtils.MapperForProfile<DataServiceMappingProfiles>(),
                    observationService.Object,
                    subjectService,
                    timePeriodService.Object);

                Assert.Throws<ArgumentException>(() => service.GetSubjectMeta(Guid.NewGuid()));
            }
        }

        [Fact]
        public void GetSubjectMeta_SubjectNotPublished()
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    ReleaseDate = DateTime.UtcNow.AddDays(1)
                };
                
                var subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    ReleaseId =  release.Id
                };

                context.Add(release);
                context.Add(subject);

                context.SaveChanges();

                var (filterService, filterItemService, indicatorGroupService, locationService,
                    observationService, releaseService, timePeriodService) = Mocks();

                var subjectService = new SubjectService(context, new Mock<ILogger<SubjectService>>().Object,
                    releaseService.Object);

                var service = new TableBuilderSubjectMetaService(filterService.Object,
                    filterItemService.Object,
                    indicatorGroupService.Object,
                    locationService.Object,
                    new Mock<ILogger<TableBuilderSubjectMetaService>>().Object,
                    MapperUtils.MapperForProfile<DataServiceMappingProfiles>(),
                    observationService.Object,
                    subjectService,
                    timePeriodService.Object);

                Assert.Throws<ArgumentException>(() => service.GetSubjectMeta(subject.Id));
            }
        }

        private static (Mock<IFilterService>,
            Mock<IFilterItemService>,
            Mock<IIndicatorGroupService>,
            Mock<ILocationService>,
            Mock<IObservationService>,
            Mock<IReleaseService>,
            Mock<ITimePeriodService>) Mocks()
        {
            return (new Mock<IFilterService>(),
                new Mock<IFilterItemService>(),
                new Mock<IIndicatorGroupService>(),
                new Mock<ILocationService>(),
                new Mock<IObservationService>(),
                new Mock<IReleaseService>(),
                new Mock<ITimePeriodService>());
        }
    }
}