using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
            var contentReleaseVersion1 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                MetaGuidance = "Version 1 Release Meta Guidance"
            };

            var contentReleaseVersion2 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id,
                MetaGuidance = "Version 2 Release Meta Guidance"
            };

            var statsReleaseVersion1 = new Data.Model.Release
            {
                Id = contentReleaseVersion1.Id,
                PreviousVersionId = contentReleaseVersion1.PreviousVersionId
            };

            var statsReleaseVersion2 = new Data.Model.Release
            {
                Id = contentReleaseVersion2.Id,
                PreviousVersionId = contentReleaseVersion2.PreviousVersionId
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

            // Version 1 has one Subject, Version 2 adds another Subject

            var releaseVersion1Subject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion1,
                Subject = subject1,
                MetaGuidance = "Version 1 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject1,
                MetaGuidance = "Version 2 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject2 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject2,
                MetaGuidance = "Version 2 Subject 2 Meta Guidance"
            };

            var file1 = new ReleaseFileReference
            {
                Filename = "file1.csv",
                Release = contentReleaseVersion1,
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject1.Id
            };

            var file2 = new ReleaseFileReference
            {
                Filename = "file2.csv",
                Release = contentReleaseVersion1,
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject2.Id
            };

            var releaseVersion1File1 = new ReleaseFile
            {
                Release = contentReleaseVersion1,
                ReleaseFileReference = file1
            };

            var releaseVersion2File1 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                ReleaseFileReference = file1
            };

            var releaseVersion2File2 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
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
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(file1, file2);
                await contentDbContext.AddRangeAsync(releaseVersion1File1, releaseVersion2File1, releaseVersion2File2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseVersion1Subject1, releaseVersion2Subject1,
                    releaseVersion2Subject2);
                await statisticsDbContext.AddRangeAsync(subject1Observation1, subject1Observation2,
                    subject1Observation3, subject2Observation1, subject2Observation2, subject2Observation3);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                // Assert version 1 has one Subject with the correct content
                var version1Result = await service.Get(contentReleaseVersion1.Id);

                Assert.True(version1Result.IsRight);

                Assert.Equal(contentReleaseVersion1.Id, version1Result.Right.Id);
                Assert.Equal("Version 1 Release Meta Guidance", version1Result.Right.Content);
                Assert.Single(version1Result.Right.Subjects);

                Assert.Equal(subject1.Id, version1Result.Right.Subjects[0].Id);
                Assert.Equal("Version 1 Subject 1 Meta Guidance", version1Result.Right.Subjects[0].Content);
                Assert.Equal("file1.csv", version1Result.Right.Subjects[0].Filename);
                Assert.Equal("Subject 1", version1Result.Right.Subjects[0].Name);
                Assert.Equal("2020_AYQ3", version1Result.Right.Subjects[0].TimePeriods.From);
                Assert.Equal("2021_AYQ1", version1Result.Right.Subjects[0].TimePeriods.To);
                Assert.Equal(new List<GeographicLevel>
                {
                    GeographicLevel.Country, GeographicLevel.LocalAuthority, GeographicLevel.LocalAuthorityDistrict
                }, version1Result.Right.Subjects[0].GeographicLevels);

                // Assert version 2 has two Subjects with the correct content
                var version2Result = await service.Get(contentReleaseVersion2.Id);

                Assert.True(version2Result.IsRight);

                Assert.Equal(contentReleaseVersion2.Id, version2Result.Right.Id);
                Assert.Equal("Version 2 Release Meta Guidance", version2Result.Right.Content);
                Assert.Equal(2, version2Result.Right.Subjects.Count);

                Assert.Equal(subject1.Id, version2Result.Right.Subjects[0].Id);
                Assert.Equal("Version 2 Subject 1 Meta Guidance", version2Result.Right.Subjects[0].Content);
                Assert.Equal("file1.csv", version2Result.Right.Subjects[0].Filename);
                Assert.Equal("Subject 1", version2Result.Right.Subjects[0].Name);
                Assert.Equal("2020_AYQ3", version2Result.Right.Subjects[0].TimePeriods.From);
                Assert.Equal("2021_AYQ1", version2Result.Right.Subjects[0].TimePeriods.To);
                Assert.Equal(new List<GeographicLevel>
                {
                    GeographicLevel.Country, GeographicLevel.LocalAuthority, GeographicLevel.LocalAuthorityDistrict
                }, version1Result.Right.Subjects[0].GeographicLevels);

                Assert.Equal(subject2.Id, version2Result.Right.Subjects[1].Id);
                Assert.Equal("Version 2 Subject 2 Meta Guidance", version2Result.Right.Subjects[1].Content);
                Assert.Equal("file2.csv", version2Result.Right.Subjects[1].Filename);
                Assert.Equal("Subject 2", version2Result.Right.Subjects[1].Name);
                Assert.Equal("2020_T3", version2Result.Right.Subjects[1].TimePeriods.From);
                Assert.Equal("2021_T2", version2Result.Right.Subjects[1].TimePeriods.To);
                Assert.Equal(new List<GeographicLevel>
                {
                    GeographicLevel.Country, GeographicLevel.Region
                }, version2Result.Right.Subjects[1].GeographicLevels);
            }
        }

        [Fact]
        public async Task UpdateRelease()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                MetaGuidance = "Release Meta Guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                var result = await service.UpdateRelease(release.Id, new MetaGuidanceUpdateReleaseViewModel
                {
                    Content = "Updated Release Meta Guidance"
                });

                Assert.True(result.IsRight);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Updated Release Meta Guidance", result.Right.Content);
                Assert.Empty(result.Right.Subjects);
            }
        }

        [Fact]
        public async Task UpdateSubject()
        {
            var contentReleaseVersion1 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                MetaGuidance = "Version 1 Release Meta Guidance"
            };

            var contentReleaseVersion2 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id,
                MetaGuidance = "Version 2 Release Meta Guidance"
            };

            var statsReleaseVersion1 = new Data.Model.Release
            {
                Id = contentReleaseVersion1.Id,
                PreviousVersionId = contentReleaseVersion1.PreviousVersionId
            };

            var statsReleaseVersion2 = new Data.Model.Release
            {
                Id = contentReleaseVersion2.Id,
                PreviousVersionId = contentReleaseVersion2.PreviousVersionId
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

            // Version 1 has one Subject, version 2 adds another Subject

            var releaseVersion1Subject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion1,
                Subject = subject1,
                MetaGuidance = "Version 1 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject1,
                MetaGuidance = "Version 2 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject2 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject2,
                MetaGuidance = "Version 2 Subject 2 Meta Guidance"
            };

            var file1 = new ReleaseFileReference
            {
                Filename = "file1.csv",
                Release = contentReleaseVersion1,
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject1.Id
            };

            var file2 = new ReleaseFileReference
            {
                Filename = "file2.csv",
                Release = contentReleaseVersion1,
                ReleaseFileType = ReleaseFileTypes.Data,
                SubjectId = subject2.Id
            };

            var releaseVersion1File1 = new ReleaseFile
            {
                Release = contentReleaseVersion1,
                ReleaseFileReference = file1
            };

            var releaseVersion2File1 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                ReleaseFileReference = file1
            };

            var releaseVersion2File2 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                ReleaseFileReference = file2
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(file1, file2);
                await contentDbContext.AddRangeAsync(releaseVersion1File1, releaseVersion2File1, releaseVersion2File2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseVersion1Subject1, releaseVersion2Subject1,
                    releaseVersion2Subject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                // Update Subject 1 on version 2
                var updateResult = await service.UpdateSubject(
                    contentReleaseVersion2.Id, subject1.Id, new MetaGuidanceUpdateSubjectViewModel
                {
                    Content = "Version 2 Subject 1 Meta Guidance Updated"
                });
                
                // Assert only one Subject has been updated

                Assert.True(updateResult.IsRight);

                Assert.Equal(contentReleaseVersion2.Id, updateResult.Right.Id);
                Assert.Equal("Version 2 Release Meta Guidance", updateResult.Right.Content);
                Assert.Equal(2, updateResult.Right.Subjects.Count);

                Assert.Equal(subject1.Id, updateResult.Right.Subjects[0].Id);
                Assert.Equal("Version 2 Subject 1 Meta Guidance Updated", updateResult.Right.Subjects[0].Content);
                Assert.Equal("file1.csv", updateResult.Right.Subjects[0].Filename);
                Assert.Equal("Subject 1", updateResult.Right.Subjects[0].Name);

                Assert.Equal(subject2.Id, updateResult.Right.Subjects[1].Id);
                Assert.Equal("Version 2 Subject 2 Meta Guidance", updateResult.Right.Subjects[1].Content);
                Assert.Equal("file2.csv", updateResult.Right.Subjects[1].Filename);
                Assert.Equal("Subject 2", updateResult.Right.Subjects[1].Name);

                // Assert the same Subject on version 1 hasn't been affected
                var version1Result = await service.Get(contentReleaseVersion1.Id);

                Assert.True(version1Result.IsRight);

                Assert.Equal(contentReleaseVersion1.Id, version1Result.Right.Id);
                Assert.Equal("Version 1 Release Meta Guidance", version1Result.Right.Content);
                Assert.Single(version1Result.Right.Subjects);

                Assert.Equal(subject1.Id, version1Result.Right.Subjects[0].Id);
                Assert.Equal("Version 1 Subject 1 Meta Guidance", version1Result.Right.Subjects[0].Content);
                Assert.Equal("file1.csv", version1Result.Right.Subjects[0].Filename);
                Assert.Equal("Subject 1", version1Result.Right.Subjects[0].Name);
            }
        }

        private static MetaGuidanceService SetupMetaGuidanceService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper = null,
            IUserService userService = null)
        {
            return new MetaGuidanceService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                statisticsDbContext,
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}