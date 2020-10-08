using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class MetaGuidanceServiceTests
    {
        [Fact]
        public async Task Get()
        {
            var releaseId = Guid.NewGuid();

            var release = new Release
            {
                Id = releaseId,
                MetaGuidance = "Release Meta Guidance"
            };

            var statsRelease = new Data.Model.Release
            {
                Id = releaseId
            };

            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Subject 1",
                MetaGuidance = "Subject 1 Meta Guidance",
            };

            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Subject 2",
                MetaGuidance = "Subject 2 Meta Guidance"
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = subject1
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = subject2
            };

            var file1 = new ReleaseFileReference
            {
                Filename = "file1.csv",
                Release = release,
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject1.Id
            };

            var file2 = new ReleaseFileReference
            {
                Filename = "file2.csv",
                Release = release,
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject2.Id
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = file1
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = file2
            };

            var subject1Observation1 = new Observation
            {
                GeographicLevel = GeographicLevel.Country,
                Subject = subject1,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ3
            };

            var subject1Observation2 = new Observation
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Subject = subject1,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ4
            };

            var subject1Observation3 = new Observation
            {
                GeographicLevel = GeographicLevel.LocalAuthorityDistrict,
                Subject = subject1,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.AcademicYearQ1
            };

            var subject2Observation1 = new Observation
            {
                GeographicLevel = GeographicLevel.Country,
                Subject = subject2,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.SummerTerm
            };

            var subject2Observation2 = new Observation
            {
                GeographicLevel = GeographicLevel.Region,
                Subject = subject2,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.AutumnTerm
            };

            var subject2Observation3 = new Observation
            {
                GeographicLevel = GeographicLevel.Region,
                Subject = subject2,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.SpringTerm
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(file1, file2);
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.AddRangeAsync(subject1Observation1, subject1Observation2,
                    subject1Observation3);
                await statisticsDbContext.AddRangeAsync(subject2Observation1, subject2Observation2,
                    subject2Observation3);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                var result = await service.Get(release.Id);

                Assert.True(result.IsRight);

                Assert.Equal("Release Meta Guidance", result.Right.Content);

                Assert.Equal(2, result.Right.Subjects.Count);

                Assert.Equal("Subject 1 Meta Guidance", result.Right.Subjects[0].Content);
                Assert.Equal("file1.csv", result.Right.Subjects[0].Filename);
                Assert.Equal("Subject 1", result.Right.Subjects[0].Name);
                Assert.Equal("2020_AYQ3", result.Right.Subjects[0].Start);
                Assert.Equal("2021_AYQ1", result.Right.Subjects[0].End);
                Assert.Equal(new List<GeographicLevel>
                {
                    GeographicLevel.Country, GeographicLevel.LocalAuthority, GeographicLevel.LocalAuthorityDistrict
                }, result.Right.Subjects[0].GeographicLevels);

                Assert.Equal("Subject 2 Meta Guidance", result.Right.Subjects[1].Content);
                Assert.Equal("file2.csv", result.Right.Subjects[1].Filename);
                Assert.Equal("Subject 2", result.Right.Subjects[1].Name);
                Assert.Equal("2020_T3", result.Right.Subjects[1].Start);
                Assert.Equal("2021_T2", result.Right.Subjects[1].End);
                Assert.Equal(new List<GeographicLevel>
                {
                    GeographicLevel.Country, GeographicLevel.Region
                }, result.Right.Subjects[1].GeographicLevels);
            }
        }

        [Fact]
        public async Task Update()
        {
            // TODO
        }

        private static MetaGuidanceService SetupMetaGuidanceService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IUserService userService = null)
        {
            return new MetaGuidanceService(
                contentDbContext,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                statisticsDbContext,
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}