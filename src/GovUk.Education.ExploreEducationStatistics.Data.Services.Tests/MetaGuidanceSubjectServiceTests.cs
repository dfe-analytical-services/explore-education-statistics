#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class MetaGuidanceSubjectServiceTests
    {
        [Fact]
        public async Task Get()
        {
            var release = new Release();

            var subject1 = new Subject();

            var subject2 = new Subject();

            var subject1Filter = new Filter
            {
                Subject = subject1,
                Name = "subject1_filter",
                Label = "Subject 1 Filter",
                Hint = "Hint"
            };

            var subject2Filter = new Filter
            {
                Subject = subject2,
                Name = "subject2_filter",
                Label = "Subject 2 Filter",
                Hint = null
            };

            var subject1IndicatorGroup = new IndicatorGroup
            {
                Subject = subject1,
                Indicators = new List<Indicator>
                {
                    new Indicator
                    {
                        Name = "subject1_indicator",
                        Label = "Subject 1 Indicator"
                    }
                }
            };

            var subject2IndicatorGroup = new IndicatorGroup
            {
                Subject = subject2,
                Indicators = new List<Indicator>
                {
                    new Indicator
                    {
                        Name = "subject2_indicator",
                        Label = "Subject 2 Indicator"
                    }
                }
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = subject1,
                MetaGuidance = "Subject 1 Meta Guidance"
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = subject2,
                MetaGuidance = "Subject 2 Meta Guidance"
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

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.AddRangeAsync(subject1Filter, subject2Filter);
                await statisticsDbContext.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
                await statisticsDbContext.AddRangeAsync(subject1Observation1, subject1Observation2,
                    subject1Observation3, subject2Observation1, subject2Observation2, subject2Observation3);
                await statisticsDbContext.SaveChangesAsync();
            }

            var releaseFile1 = new ReleaseFile
            {
                ReleaseId = release.Id,
                Name = "Subject 1",
                File = new File
                {
                    SubjectId = subject1.Id,
                    Filename = "file1.csv",
                    Type = FileType.Data,
                },
            };

            var releaseFile2 = new ReleaseFile
            {
                ReleaseId = release.Id,
                Name = "Subject 2",
                File = new File
                {
                    SubjectId = subject2.Id,
                    Filename = "file2.csv",
                    Type = FileType.Data,
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.GetSubjects(release.Id);

                // Assert there are two Subjects with the correct content
                Assert.True(result.IsRight);

                Assert.Equal(2, result.Right.Count);

                Assert.Equal(subject1.Id, result.Right[0].Id);
                Assert.Equal("Subject 1 Meta Guidance", result.Right[0].Content);
                Assert.Equal("file1.csv", result.Right[0].Filename);
                Assert.Equal("Subject 1", result.Right[0].Name);
                Assert.Equal("2020/21 Q3", result.Right[0].TimePeriods.From);
                Assert.Equal("2021/22 Q1", result.Right[0].TimePeriods.To);
                Assert.Equal(new List<string>
                {
                    "National", "Local Authority", "Local Authority District"
                }, result.Right[0].GeographicLevels);
                Assert.Equal(2, result.Right[0].Variables.Count);
                Assert.Equal("Subject 1 Filter - Hint", result.Right[0].Variables[0].Label);
                Assert.Equal("subject1_filter", result.Right[0].Variables[0].Value);
                Assert.Equal("Subject 1 Indicator", result.Right[0].Variables[1].Label);
                Assert.Equal("subject1_indicator", result.Right[0].Variables[1].Value);

                Assert.Equal(subject2.Id, result.Right[1].Id);
                Assert.Equal("Subject 2 Meta Guidance", result.Right[1].Content);
                Assert.Equal("file2.csv", result.Right[1].Filename);
                Assert.Equal("Subject 2", result.Right[1].Name);
                Assert.Equal("2020/21 Summer Term", result.Right[1].TimePeriods.From);
                Assert.Equal("2021/22 Spring Term", result.Right[1].TimePeriods.To);
                Assert.Equal(new List<string>
                {
                    "National", "Regional"
                }, result.Right[1].GeographicLevels);
                Assert.Equal(2, result.Right[1].Variables.Count);
                Assert.Equal("Subject 2 Filter", result.Right[1].Variables[0].Label);
                Assert.Equal("subject2_filter", result.Right[1].Variables[0].Value);
                Assert.Equal("Subject 2 Indicator", result.Right[1].Variables[1].Label);
                Assert.Equal("subject2_indicator", result.Right[1].Variables[1].Value);
            }
        }

        [Fact]
        public async Task Get_OrderedByName()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                MetaGuidance = "Subject 1 Meta Guidance"
            };
            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                MetaGuidance = "Subject 2 Meta Guidance"
            };
            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                MetaGuidance = "Subject 3 Meta Guidance"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2, releaseSubject3);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Content.Model.Release
            {
                Id = release.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    SubjectId = releaseSubject1.SubjectId,
                    Filename = "file1.csv",
                    Type = FileType.Data,
                },
                Name = "Subject 1",
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    SubjectId = releaseSubject2.SubjectId,
                    Filename = "file2.csv",
                    Type = FileType.Data,
                },
                Name = "Subject 2",
            };
            var releaseFile3 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    SubjectId = releaseSubject3.SubjectId,
                    Filename = "file3.csv",
                    Type = FileType.Data,
                },
                Name = "Subject 3",
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    // Saved in random order
                    releaseFile3,
                    releaseFile1,
                    releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.GetSubjects(release.Id);

                Assert.True(result.IsRight);

                Assert.Equal(3, result.Right.Count);

                Assert.Equal("Subject 1", result.Right[0].Name);
                Assert.Equal("Subject 2", result.Right[1].Name);
                Assert.Equal("Subject 3", result.Right[2].Name);
            }
        }

        [Fact]
        public async Task Get_SpecificSubjects()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                MetaGuidance = "Subject 1 Meta Guidance"
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                MetaGuidance = "Subject 2 Meta Guidance"
            };

            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                MetaGuidance = "Subject 3 Meta Guidance"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2, releaseSubject3);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Content.Model.Release
            {
                Id = release.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    SubjectId = releaseSubject1.SubjectId,
                    Filename = "file1.csv",
                    Type = FileType.Data,
                }
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 2",
                File = new File
                {
                    SubjectId = releaseSubject2.SubjectId,
                    Filename = "file2.csv",
                    Type = FileType.Data,
                }
            };

            var releaseFile3 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 3",
                File = new File
                {
                    SubjectId = releaseSubject3.SubjectId,
                    Filename = "file3.csv",
                    Type = FileType.Data,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    releaseFile1, releaseFile2, releaseFile3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.GetSubjects(release.Id, new List<Guid>
                {
                    releaseSubject1.SubjectId, releaseSubject3.SubjectId
                });

                // Assert only the specified Subjects are returned
                Assert.True(result.IsRight);

                Assert.Equal(2, result.Right.Count);

                Assert.Equal(releaseSubject1.SubjectId, result.Right[0].Id);
                Assert.Equal("Subject 1 Meta Guidance", result.Right[0].Content);
                Assert.Equal("file1.csv", result.Right[0].Filename);
                Assert.Equal("Subject 1", result.Right[0].Name);
                Assert.Empty(result.Right[0].TimePeriods.From);
                Assert.Empty(result.Right[0].TimePeriods.To);
                Assert.Empty(result.Right[0].GeographicLevels);
                Assert.Empty(result.Right[0].Variables);

                Assert.Equal(releaseSubject3.SubjectId, result.Right[1].Id);
                Assert.Equal("Subject 3 Meta Guidance", result.Right[1].Content);
                Assert.Equal("file3.csv", result.Right[1].Filename);
                Assert.Equal("Subject 3", result.Right[1].Name);
                Assert.Empty(result.Right[1].TimePeriods.From);
                Assert.Empty(result.Right[1].TimePeriods.To);
                Assert.Empty(result.Right[1].GeographicLevels);
                Assert.Empty(result.Right[1].Variables);
            }
        }

        [Fact]
        public async Task Get_NoRelease()
        {
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.GetSubjects(Guid.NewGuid());

                Assert.True(result.IsRight);
                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task Get_NoSubjects()
        {
            var release = new Release();

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.GetSubjects(release.Id);

                Assert.True(result.IsRight);
                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task Get_AmendedRelease()
        {
            var releaseVersion1 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var releaseVersion2 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = releaseVersion1.Id
            };

            var subject1 = new Subject();
            var subject2 = new Subject();

            // Version 1 has one Subject, Version 2 adds another Subject

            var releaseVersion1Subject1 = new ReleaseSubject
            {
                Release = releaseVersion1,
                Subject = subject1,
                MetaGuidance = "Version 1 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject1 = new ReleaseSubject
            {
                Release = releaseVersion2,
                Subject = subject1,
                MetaGuidance = "Version 2 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject2 = new ReleaseSubject
            {
                Release = releaseVersion2,
                Subject = subject2,
                MetaGuidance = "Version 2 Subject 2 Meta Guidance"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseVersion1, releaseVersion2);
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseVersion1Subject1, releaseVersion2Subject1,
                    releaseVersion2Subject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var file1 = new File
            {
                SubjectId = subject1.Id,
                Filename = "file1.csv",
                Type = FileType.Data,
            };

            var file2 = new File
            {
                SubjectId = subject2.Id,
                Filename = "file2.csv",
                Type = FileType.Data,
            };

            var contentReleaseVersion1 = new Content.Model.Release
            {
                Id = releaseVersion1.Id,
                PreviousVersionId = null,
            };

            var contentReleaseVersion2 = new Content.Model.Release
            {
                Id = releaseVersion2.Id,
                PreviousVersionId = releaseVersion1.Id,
            };

            var releaseVersion1File1 = new ReleaseFile
            {
                Release = contentReleaseVersion1,
                File = file1,
                Name = "Subject 1",
            };

            var releaseVersion2File1 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = file1,
                Name = "Subject 1",
            };

            var releaseVersion2File2 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = file2,
                Name = "Subject 2",
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    releaseVersion1File1,
                    releaseVersion2File1,
                    releaseVersion2File2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext);

                var version1Result = await service.GetSubjects(releaseVersion1.Id);

                // Assert version 1 has one Subject with the correct content
                Assert.True(version1Result.IsRight);

                Assert.Single(version1Result.Right);

                Assert.Equal(subject1.Id, version1Result.Right[0].Id);
                Assert.Equal("Version 1 Subject 1 Meta Guidance", version1Result.Right[0].Content);
                Assert.Equal("file1.csv", version1Result.Right[0].Filename);
                Assert.Equal("Subject 1", version1Result.Right[0].Name);
                Assert.Empty(version1Result.Right[0].TimePeriods.From);
                Assert.Empty(version1Result.Right[0].TimePeriods.To);
                Assert.Empty(version1Result.Right[0].GeographicLevels);
                Assert.Empty(version1Result.Right[0].Variables);

                var version2Result = await service.GetSubjects(releaseVersion2.Id);

                // Assert version 2 has two Subjects with the correct content
                Assert.True(version2Result.IsRight);

                Assert.Equal(2, version2Result.Right.Count);

                Assert.Equal(subject1.Id, version2Result.Right[0].Id);
                Assert.Equal("Version 2 Subject 1 Meta Guidance", version2Result.Right[0].Content);
                Assert.Equal("file1.csv", version2Result.Right[0].Filename);
                Assert.Equal("Subject 1", version2Result.Right[0].Name);
                Assert.Empty(version2Result.Right[0].TimePeriods.From);
                Assert.Empty(version2Result.Right[0].TimePeriods.To);
                Assert.Empty(version2Result.Right[0].GeographicLevels);

                Assert.Equal(subject2.Id, version2Result.Right[1].Id);
                Assert.Equal("Version 2 Subject 2 Meta Guidance", version2Result.Right[1].Content);
                Assert.Equal("file2.csv", version2Result.Right[1].Filename);
                Assert.Equal("Subject 2", version2Result.Right[1].Name);
                Assert.Empty(version2Result.Right[1].TimePeriods.From);
                Assert.Empty(version2Result.Right[1].TimePeriods.To);
                Assert.Empty(version2Result.Right[1].GeographicLevels);
                Assert.Empty(version2Result.Right[1].Variables);
            }
        }

        [Fact]
        public async Task Validate_NoRelease()
        {
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.Validate(Guid.NewGuid());

                Assert.True(result.IsRight);
                Assert.True(result.Right);
            }
        }

        [Fact]
        public async Task Validate_NoSubjects()
        {
            var release = new Release();

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
                Assert.True(result.Right);
            }
        }

        [Fact]
        public async Task Validate_MetaGuidancePopulated()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                MetaGuidance = "Subject 1 Meta Guidance",
                Release = release,
                Subject = new Subject()
            };

            var releaseSubject2 = new ReleaseSubject
            {
                MetaGuidance = "Subject 2 Meta Guidance",
                Release = release,
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
                Assert.True(result.Right);
            }
        }

        [Fact]
        public async Task Validate_MetaGuidanceNotPopulated()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                MetaGuidance = "Subject 1 Meta Guidance",
                Release = release,
                Subject = new Subject()
            };

            // Meta guidance is not populated for Subject 2
            var releaseSubject2 = new ReleaseSubject
            {
                MetaGuidance = null,
                Release = release,
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
                Assert.False(result.Right);
            }
        }

        [Fact]
        public async Task GetTimePeriods()
        {
            var release = new Release();

            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
                MetaGuidance = "Subject 1 Meta Guidance"
            };

            var subjectObservation1 = new Observation
            {
                GeographicLevel = GeographicLevel.Country,
                Subject = subject,
                Year = 2030,
                TimeIdentifier = TimeIdentifier.AcademicYearQ3
            };

            var subjectObservation2 = new Observation
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Subject = subject,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ4
            };

            var subjectObservation3 = new Observation
            {
                GeographicLevel = GeographicLevel.LocalAuthorityDistrict,
                Subject = subject,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.AcademicYearQ1
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.AddRangeAsync(
                    subjectObservation1,
                    subjectObservation2,
                    subjectObservation3);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.GetTimePeriods(subject.Id);

                Assert.Equal("2020/21 Q4", result.From);
                Assert.Equal("2030/31 Q3", result.To);
            }
        }

        [Fact]
        public async Task GetTimePeriods_NoObservations()
        {
            var release = new Release();

            var subject = new Subject();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
                MetaGuidance = "Subject 1 Meta Guidance"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.AddAsync(releaseSubject1);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.GetTimePeriods(subject.Id);

                Assert.Empty(result.From);
                Assert.Empty(result.To);
            }
        }

        [Fact]
        public async Task GetGeographicLevels()
        {
            var release = new Release();

            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
                MetaGuidance = "Subject 1 Meta Guidance"
            };

            var subjectObservation1 = new Observation
            {
                GeographicLevel = GeographicLevel.Country,
                Subject = subject,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ3
            };

            var subjectObservation2 = new Observation
            {
                GeographicLevel = GeographicLevel.LocalAuthority,
                Subject = subject,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ4
            };

            var subjectObservation3 = new Observation
            {
                GeographicLevel = GeographicLevel.LocalAuthorityDistrict,
                Subject = subject,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.AcademicYearQ1
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.AddRangeAsync(
                    subjectObservation1,
                    subjectObservation2,
                    subjectObservation3);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.GetGeographicLevels(subject.Id);

                Assert.Equal(3, result.Count);
                Assert.Equal("National", result[0]);
                Assert.Equal("Local Authority", result[1]);
                Assert.Equal("Local Authority District", result[2]);
            }
        }

        [Fact]
        public async Task GetGeographicLevels_NoObservations()
        {
            var release = new Release();

            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
                MetaGuidance = "Subject 1 Meta Guidance"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddAsync(subject);
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceSubjectService(statisticsDbContext: statisticsDbContext);

                var result = await service.GetGeographicLevels(subject.Id);

                Assert.Empty(result);
            }
        }

        private static MetaGuidanceSubjectService SetupMetaGuidanceSubjectService(
            StatisticsDbContext statisticsDbContext,
            IFilterService? filterService = null,
            IIndicatorService? indicatorService = null,
            IPersistenceHelper<StatisticsDbContext>? persistenceHelper = null,
            ContentDbContext? contentDbContext = null)
        {
            return new MetaGuidanceSubjectService(
                filterService ?? new FilterService(statisticsDbContext, new Mock<ILogger<FilterService>>().Object),
                indicatorService ?? new IndicatorService(statisticsDbContext, new Mock<ILogger<IndicatorService>>().Object),
                statisticsDbContext,
                persistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                contentDbContext != null
                    ? new ReleaseDataFileRepository(contentDbContext)
                    : new Mock<IReleaseDataFileRepository>().Object
            );
        }
    }
}
