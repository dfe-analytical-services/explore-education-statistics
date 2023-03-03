#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class DataGuidanceSubjectServiceTests
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
                    new()
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
                    new()
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
                DataGuidance = "Subject 1 Guidance"
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = subject2,
                DataGuidance = "Subject 2 Guidance"
            };

            var subject1Observation1 = new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.Country
                },
                Subject = subject1,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ3
            };

            var subject1Observation2 = new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority
                },
                Subject = subject1,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ4
            };

            var subject1Observation3 = new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthorityDistrict
                },
                Subject = subject1,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.AcademicYearQ1
            };

            var subject2Observation1 = new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.Country
                },
                Subject = subject2,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.SummerTerm
            };

            var subject2Observation2 = new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.Region
                },
                Subject = subject2,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.AutumnTerm
            };

            var subject2Observation3 = new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.Region
                },
                Subject = subject2,
                Year = 2021,
                TimeIdentifier = TimeIdentifier.SpringTerm
            };

            var subject1Footnote1 = new SubjectFootnote
            {
                Subject = subject1,
                Footnote = new Footnote
                {
                    Content = "Subject 1 Footnote 1",
                    Order = 0
                }
            };
            var subject1Footnote2 = new FilterFootnote
            {
                Filter = subject1Filter,
                Footnote = new Footnote
                {
                    Content = "Subject 1 Footnote 2",
                    Order = 1
                }
            };
            var subject1Footnote3 = new FilterGroupFootnote
            {
                FilterGroup = new FilterGroup
                {
                    Filter = subject1Filter
                },
                Footnote = new Footnote
                {
                    Content = "Subject 1 Footnote 3",
                    Order = 2
                }
            };

            var subject2Footnote1 = new FilterItemFootnote
            {
                FilterItem = new FilterItem
                {
                    FilterGroup = new FilterGroup
                    {
                        Filter = subject2Filter
                    }
                },
                Footnote = new Footnote
                {
                    Content = "Subject 2 Footnote 1",
                    Order = 0
                }
            };
            var subject2Footnote2 = new IndicatorFootnote
            {
                Indicator = subject2IndicatorGroup.Indicators[0],
                Footnote = new Footnote
                {
                    Content = "Subject 2 Footnote 2",
                    Order = 1
                }
            };

            var releaseFootnote1 = new ReleaseFootnote
            {
                Footnote = subject1Footnote1.Footnote,
                Release = release
            };
            var releaseFootnote2 = new ReleaseFootnote
            {
                Footnote = subject1Footnote2.Footnote,
                Release = release
            };
            var releaseFootnote3 = new ReleaseFootnote
            {
                Footnote = subject1Footnote3.Footnote,
                Release = release
            };
            var releaseFootnote4 = new ReleaseFootnote
            {
                Footnote = subject2Footnote1.Footnote,
                Release = release
            };
            var releaseFootnote5 = new ReleaseFootnote
            {
                Footnote = subject2Footnote2.Footnote,
                Release = release
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.AddRangeAsync(subject1Filter, subject2Filter);
                await statisticsDbContext.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
                await statisticsDbContext.AddRangeAsync(
                    subject1Observation1,
                    subject1Observation2,
                    subject1Observation3,
                    subject2Observation1,
                    subject2Observation2,
                    subject2Observation3
                );
                await statisticsDbContext.AddRangeAsync(subject1Footnote1, subject1Footnote2, subject1Footnote3);
                await statisticsDbContext.AddRangeAsync(subject2Footnote1, subject2Footnote2);
                await statisticsDbContext.AddRangeAsync(
                    releaseFootnote1,
                    releaseFootnote2,
                    releaseFootnote3,
                    releaseFootnote4,
                    releaseFootnote5
                );
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
                var service = SetupService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext);

                var result = (await service.GetSubjects(release.Id)).AssertRight();

                // Assert there are two Subjects with the correct content

                Assert.Equal(2, result.Count);

                Assert.Equal(subject1.Id, result[0].Id);
                Assert.Equal("Subject 1 Guidance", result[0].Content);
                Assert.Equal("file1.csv", result[0].Filename);
                Assert.Equal("Subject 1", result[0].Name);

                Assert.Equal("2020/21 Q3", result[0].TimePeriods.From);
                Assert.Equal("2021/22 Q1", result[0].TimePeriods.To);
                Assert.Equal(new List<string>
                {
                    "National",
                    "Local authority",
                    "Local authority district"
                }, result[0].GeographicLevels);

                Assert.Equal(2, result[0].Variables.Count);
                Assert.Equal("Subject 1 Filter - Hint", result[0].Variables[0].Label);
                Assert.Equal("subject1_filter", result[0].Variables[0].Value);
                Assert.Equal("Subject 1 Indicator", result[0].Variables[1].Label);
                Assert.Equal("subject1_indicator", result[0].Variables[1].Value);

                Assert.Equal(3, result[0].Footnotes.Count);
                Assert.Equal("Subject 1 Footnote 1", result[0].Footnotes[0].Label);
                Assert.Equal(subject1Footnote1.FootnoteId, result[0].Footnotes[0].Id);
                Assert.Equal("Subject 1 Footnote 2", result[0].Footnotes[1].Label);
                Assert.Equal(subject1Footnote2.FootnoteId, result[0].Footnotes[1].Id);
                Assert.Equal("Subject 1 Footnote 3", result[0].Footnotes[2].Label);
                Assert.Equal(subject1Footnote3.FootnoteId, result[0].Footnotes[2].Id);

                Assert.Equal(subject2.Id, result[1].Id);
                Assert.Equal("Subject 2 Guidance", result[1].Content);
                Assert.Equal("file2.csv", result[1].Filename);
                Assert.Equal("Subject 2", result[1].Name);

                Assert.Equal("2020/21 Summer term", result[1].TimePeriods.From);
                Assert.Equal("2021/22 Spring term", result[1].TimePeriods.To);
                Assert.Equal(new List<string>
                {
                    "National",
                    "Regional"
                }, result[1].GeographicLevels);

                Assert.Equal(2, result[1].Variables.Count);
                Assert.Equal("Subject 2 Filter", result[1].Variables[0].Label);
                Assert.Equal("subject2_filter", result[1].Variables[0].Value);
                Assert.Equal("Subject 2 Indicator", result[1].Variables[1].Label);
                Assert.Equal("subject2_indicator", result[1].Variables[1].Value);

                Assert.Equal(2, result[1].Footnotes.Count);
                Assert.Equal("Subject 2 Footnote 1", result[1].Footnotes[0].Label);
                Assert.Equal(subject2Footnote1.FootnoteId, result[1].Footnotes[0].Id);
                Assert.Equal("Subject 2 Footnote 2", result[1].Footnotes[1].Label);
                Assert.Equal(subject2Footnote2.FootnoteId, result[1].Footnotes[1].Id);
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
                DataGuidance = "Subject 1 Guidance"
            };
            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                DataGuidance = "Subject 2 Guidance"
            };
            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                DataGuidance = "Subject 3 Guidance"
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
                var service = SetupService(
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
                DataGuidance = "Subject 1 Guidance"
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                DataGuidance = "Subject 2 Guidance"
            };

            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject(),
                DataGuidance = "Subject 3 Guidance"
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
                var service = SetupService(
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
                Assert.Equal("Subject 1 Guidance", result.Right[0].Content);
                Assert.Equal("file1.csv", result.Right[0].Filename);
                Assert.Equal("Subject 1", result.Right[0].Name);
                Assert.Empty(result.Right[0].TimePeriods.From);
                Assert.Empty(result.Right[0].TimePeriods.To);
                Assert.Empty(result.Right[0].GeographicLevels);
                Assert.Empty(result.Right[0].Variables);

                Assert.Equal(releaseSubject3.SubjectId, result.Right[1].Id);
                Assert.Equal("Subject 3 Guidance", result.Right[1].Content);
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
                var service = SetupService(statisticsDbContext: statisticsDbContext);

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
                var service = SetupService(statisticsDbContext: statisticsDbContext);

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
                Id = Guid.NewGuid()
            };

            var releaseVersion2 = new Release
            {
                Id = Guid.NewGuid()
            };

            var subject1 = new Subject();
            var subject2 = new Subject();

            // Version 1 has one Subject, Version 2 adds another Subject

            var releaseVersion1Subject1 = new ReleaseSubject
            {
                Release = releaseVersion1,
                Subject = subject1,
                DataGuidance = "Version 1 Subject 1 Guidance"
            };

            var releaseVersion2Subject1 = new ReleaseSubject
            {
                Release = releaseVersion2,
                Subject = subject1,
                DataGuidance = "Version 2 Subject 1 Guidance"
            };

            var releaseVersion2Subject2 = new ReleaseSubject
            {
                Release = releaseVersion2,
                Subject = subject2,
                DataGuidance = "Version 2 Subject 2 Guidance"
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
                var service = SetupService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext);

                var version1Result = await service.GetSubjects(releaseVersion1.Id);

                // Assert version 1 has one Subject with the correct content
                Assert.True(version1Result.IsRight);

                Assert.Single(version1Result.Right);

                Assert.Equal(subject1.Id, version1Result.Right[0].Id);
                Assert.Equal("Version 1 Subject 1 Guidance", version1Result.Right[0].Content);
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
                Assert.Equal("Version 2 Subject 1 Guidance", version2Result.Right[0].Content);
                Assert.Equal("file1.csv", version2Result.Right[0].Filename);
                Assert.Equal("Subject 1", version2Result.Right[0].Name);
                Assert.Empty(version2Result.Right[0].TimePeriods.From);
                Assert.Empty(version2Result.Right[0].TimePeriods.To);
                Assert.Empty(version2Result.Right[0].GeographicLevels);

                Assert.Equal(subject2.Id, version2Result.Right[1].Id);
                Assert.Equal("Version 2 Subject 2 Guidance", version2Result.Right[1].Content);
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
                var service = SetupService(statisticsDbContext: statisticsDbContext);

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
                var service = SetupService(statisticsDbContext: statisticsDbContext);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
                Assert.True(result.Right);
            }
        }

        [Fact]
        public async Task Validate_DataGuidancePopulated()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                DataGuidance = "Subject 1 Guidance",
                Release = release,
                Subject = new Subject()
            };

            var releaseSubject2 = new ReleaseSubject
            {
                DataGuidance = "Subject 2 Guidance",
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
                var service = SetupService(statisticsDbContext: statisticsDbContext);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
                Assert.True(result.Right);
            }
        }

        [Fact]
        public async Task Validate_DataGuidanceNotPopulated()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                DataGuidance = "Subject 1 Guidance",
                Release = release,
                Subject = new Subject()
            };

            // Guidance is not populated for Subject 2
            var releaseSubject2 = new ReleaseSubject
            {
                DataGuidance = null,
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
                var service = SetupService(statisticsDbContext: statisticsDbContext);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
                Assert.False(result.Right);
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
                DataGuidance = "Subject 1 Guidance"
            };

            var subjectObservation1 = new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.Country
                },
                Subject = subject,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ3
            };

            var subjectObservation2 = new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthority
                },
                Subject = subject,
                Year = 2020,
                TimeIdentifier = TimeIdentifier.AcademicYearQ4
            };

            var subjectObservation3 = new Observation
            {
                Location = new Location
                {
                    GeographicLevel = GeographicLevel.LocalAuthorityDistrict
                },
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
                var service = SetupService(statisticsDbContext: statisticsDbContext);

                var result = await service.GetGeographicLevels(subject.Id);

                Assert.Equal(3, result.Count);
                Assert.Equal("National", result[0]);
                Assert.Equal("Local authority", result[1]);
                Assert.Equal("Local authority district", result[2]);
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
                DataGuidance = "Subject 1 Guidance"
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
                var service = SetupService(statisticsDbContext: statisticsDbContext);

                var result = await service.GetGeographicLevels(subject.Id);

                Assert.Empty(result);
            }
        }

        private static DataGuidanceSubjectService SetupService(
            StatisticsDbContext statisticsDbContext,
            IIndicatorRepository? indicatorRepository = null,
            IPersistenceHelper<StatisticsDbContext>? persistenceHelper = null,
            ContentDbContext? contentDbContext = null,
            IFootnoteRepository? footnoteRepository = null,
            ITimePeriodService? timePeriodService = null)
        {
            return new (
                indicatorRepository ?? new IndicatorRepository(statisticsDbContext),
                statisticsDbContext,
                persistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                contentDbContext != null
                    ? new ReleaseDataFileRepository(contentDbContext)
                    : Mock.Of<IReleaseDataFileRepository>(),
                footnoteRepository ?? new FootnoteRepository(statisticsDbContext),
                timePeriodService ?? new TimePeriodService(statisticsDbContext)
            );
        }
    }
}
