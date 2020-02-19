using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests
{
    public class SubjectServiceTests
    {
        [Fact]
        public void IsSubjectForLatestRelease()
        {
            var (logger, releaseService) = Mocks();

            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var publication = new Publication
                {
                    Id = Guid.NewGuid()
                };

                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publication.Id,
                    ReleaseDate = DateTime.UtcNow
                };

                var subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    ReleaseId = release.Id
                };

                context.Add(publication);
                context.Add(release);
                context.Add(subject);

                context.SaveChanges();

                releaseService.Setup(s => s.GetLatestRelease(publication.Id)).Returns(release.Id);

                var service = new SubjectService(context, logger.Object, releaseService.Object);
                Assert.True(service.IsSubjectForLatestRelease(subject.Id));
            }
        }

        [Fact]
        public void IsSubjectForLatestRelease_SubjectNotFound()
        {
            var (logger, releaseService) = Mocks();

            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var service = new SubjectService(context, logger.Object, releaseService.Object);
                Assert.Throws<ArgumentException>(() => service.IsSubjectForLatestRelease(Guid.NewGuid()));
            }
        }

        [Fact]
        public void IsSubjectForLatestRelease_SubjectBelongsToOldRelease()
        {
            var (logger, releaseService) = Mocks();

            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var publication = new Publication
                {
                    Id = Guid.NewGuid()
                };

                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publication.Id,
                    ReleaseDate = DateTime.UtcNow
                };

                var subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    ReleaseId = release.Id
                };

                context.Add(publication);
                context.Add(release);
                context.Add(subject);

                context.SaveChanges();

                releaseService.Setup(s => s.GetLatestRelease(publication.Id)).Returns(Guid.NewGuid());

                var service = new SubjectService(context, logger.Object, releaseService.Object);
                Assert.False(service.IsSubjectForLatestRelease(subject.Id));
            }
        }

        [Fact]
        public void IsSubjectForLatestRelease_SubjectBelongsToNonLiveRelease()
        {
            var (logger, releaseService) = Mocks();

            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            var options = builder.Options;

            using (var context = new StatisticsDbContext(options, null))
            {
                var publication = new Publication
                {
                    Id = Guid.NewGuid()
                };

                var release = new Release
                {
                    Id = Guid.NewGuid(),
                    PublicationId = publication.Id,
                    ReleaseDate = DateTime.UtcNow.AddDays(1)
                };

                var subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    ReleaseId = release.Id
                };

                context.Add(publication);
                context.Add(release);
                context.Add(subject);

                context.SaveChanges();

                releaseService.Setup(s => s.GetLatestRelease(publication.Id)).Returns(Guid.NewGuid());

                var service = new SubjectService(context, logger.Object, releaseService.Object);
                Assert.Throws<ArgumentException>(() => service.IsSubjectForLatestRelease(Guid.NewGuid()));
            }
        }

        private static (Mock<ILogger<SubjectService>>,
            Mock<IReleaseService>) Mocks()
        {
            return (
                new Mock<ILogger<SubjectService>>(),
                new Mock<IReleaseService>());
        }
    }
}