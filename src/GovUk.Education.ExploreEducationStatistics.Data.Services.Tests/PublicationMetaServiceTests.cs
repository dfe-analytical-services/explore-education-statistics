using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
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
                Published = DateTime.UtcNow,
                TimeIdentifier = AcademicYearQ1,
                Year = 2018
            };

            var publicationRelease2 = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id,
                Published = DateTime.UtcNow,
                TimeIdentifier = AcademicYearQ4,
                Year = 2017
            };

            var publicationRelease3 = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id,
                Published = null,
                TimeIdentifier = AcademicYearQ2,
                Year = 2018
            };
            
            var publicationRelease1Subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 1 subject 1",
            };
            
            var publicationRelease1Subject1Link = new ReleaseSubject
            {
                ReleaseId = publicationRelease1.Id,
                SubjectId = publicationRelease1Subject1.Id
            };
            
            var publicationRelease1Subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 1 subject 2"
            };
            
            var publicationRelease1Subject2Link = new ReleaseSubject
            {
                ReleaseId = publicationRelease1.Id,
                SubjectId = publicationRelease1Subject2.Id
            };

            var publicationRelease2Subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 2 subject"
            };
            
            var publicationRelease2SubjectLink = new ReleaseSubject
            {
                ReleaseId = publicationRelease2.Id,
                SubjectId = publicationRelease2Subject.Id
            };
            
            var publicationRelease3Subject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Release 3 subject"
            };
            
            var publicationRelease3SubjectLink = new ReleaseSubject
            {
                ReleaseId = publicationRelease3.Id,
                SubjectId = publicationRelease3Subject.Id
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

                context.AddRange(new List<ReleaseSubject>
                {
                    publicationRelease1Subject1Link,
                    publicationRelease1Subject2Link,
                    publicationRelease2SubjectLink,
                    publicationRelease3SubjectLink
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
    }
}