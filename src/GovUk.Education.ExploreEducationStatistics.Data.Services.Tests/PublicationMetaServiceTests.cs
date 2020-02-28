using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class PublicationMetaServiceTests
    {
        [Fact]
        public void GetSubjectsForLatestRelease()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
            };

            var publicationRelease1 = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id,
                ReleaseDate = DateTime.UtcNow,
                TimeIdentifier = AcademicYearQ1,
                Year = 2018
            };

            var publicationRelease2 = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id,
                ReleaseDate = DateTime.UtcNow,
                TimeIdentifier = AcademicYearQ4,
                Year = 2017
            };

            var publicationRelease3 = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id,
                ReleaseDate = DateTime.UtcNow.AddDays(1),
                TimeIdentifier = AcademicYearQ2,
                Year = 2018
            };
            
            var publicationRelease1Subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 1 subject 1",
                ReleaseId = publicationRelease1.Id
            };
            
            var publicationRelease1Subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 1 subject 2",
                ReleaseId = publicationRelease1.Id
            };

            var publicationRelease2Subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 2 subject",
                ReleaseId = publicationRelease2.Id
            };
            
            var publicationRelease3Subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 3 subject",
                ReleaseId = publicationRelease3.Id
            };
            
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                context.Add(publication);

                context.AddRange(new List<Release>
                {
                    publicationRelease1, publicationRelease2, publicationRelease3
                });

                context.AddRange(new List<Subject>
                {
                    publicationRelease1Subject1,
                    publicationRelease1Subject2,
                    publicationRelease2Subject,
                    publicationRelease3Subject
                });
                
                context.SaveChanges();

                var releaseService = new ReleaseService(context, new Mock<ILogger<ReleaseService>>().Object);
                var subjectService =
                    new SubjectService(context, new Mock<ILogger<SubjectService>>().Object, releaseService);
                var service = new PublicationMetaService(releaseService, subjectService,
                    MapperUtils.MapperForProfile<DataServiceMappingProfiles>());

                var result = service.GetSubjectsForLatestRelease(publication.Id);
                var subjects = result.Subjects.ToList();

                Assert.Equal(publication.Id, result.PublicationId);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(publicationRelease1Subject1.Id, subjects[0].Id);
                Assert.Equal(publicationRelease1Subject1.Name, subjects[0].Label);
                Assert.Equal(publicationRelease1Subject2.Id, subjects[1].Id);
                Assert.Equal(publicationRelease1Subject2.Name, subjects[1].Label);
            }
        }

        [Fact]
        public void GetSubjectsForLatestRelease_ReleaseNotFound()
        {
            var publicationId = Guid.NewGuid();
            var (releaseService,
                subjectService) = Mocks();

            var service = new PublicationMetaService(releaseService.Object, subjectService.Object,
                MapperUtils.MapperForProfile<DataServiceMappingProfiles>());

            var result = service.GetSubjectsForLatestRelease(publicationId);

            Assert.Equal(publicationId, result.PublicationId);
            Assert.Empty(result.Subjects);
        }

        private static (Mock<IReleaseService>,
            Mock<ISubjectService>) Mocks()
        {
            return (
                new Mock<IReleaseService>(),
                new Mock<ISubjectService>());
        }
    }
}