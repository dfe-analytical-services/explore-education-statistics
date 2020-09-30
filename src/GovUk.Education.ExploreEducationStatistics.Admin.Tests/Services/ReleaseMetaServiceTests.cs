using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseMetaServiceTests
    {
        [Fact]
        public async Task GetSubjects()
        {
            var releaseId = Guid.NewGuid();

            var contentRelease = new Release
            {
                Id = releaseId
            };

            var statisticsRelease = new Data.Model.Release
            {
                Id = releaseId
            };

            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Subject 1"
            };

            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Subject 2"
            };

            var subject2Replacement = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Subject 2 Replacement"
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject1
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject2
            };

            var releaseSubject2Replacement = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = subject2Replacement
            };

            var file1 = new ReleaseFileReference
            {
                Release = contentRelease,
                Filename = "data1.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject1.Id
            };

            var file2 = new ReleaseFileReference
            {
                Release = contentRelease,
                Filename = "data2.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject2.Id,
            };

            var file2Replacement = new ReleaseFileReference
            {
                Release = contentRelease,
                Filename = "data2_replacement.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject2Replacement.Id,
                Replacing = file2
            };

            file2.ReplacedBy = file2Replacement;

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                ReleaseFileReference = file1
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                ReleaseFileReference = file2
            };

            var releaseFile2Replacement = new ReleaseFile
            {
                Release = contentRelease,
                ReleaseFileReference = file2Replacement
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(file1, file2, file2Replacement);
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2, releaseFile2Replacement);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(subject1, subject2, subject2Replacement);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2, releaseSubject2Replacement);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReleaseMetaService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                var result = await replacementService.GetSubjects(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.ReleaseId);

                var subjects = result.Right.Subjects;

                Assert.NotNull(subjects);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(subject1.Id, subjects[0].Id);
                Assert.Equal(subject1.Name, subjects[0].Label);
                Assert.Equal(subject2.Id, subjects[1].Id);
                Assert.Equal(subject2.Name, subjects[1].Label);
            }
        }

        private static ReleaseMetaService BuildReleaseMetaService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            StatisticsDbContext statisticsDbContext = null,
            IUserService userService = null)
        {
            return new ReleaseMetaService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}