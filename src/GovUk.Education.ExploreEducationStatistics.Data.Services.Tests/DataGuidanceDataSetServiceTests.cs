#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class DataGuidanceDataSetServiceTests
    {
        [Fact]
        public async Task ListDataSets()
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
                Subject = subject1
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = subject2
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
                await statisticsDbContext.Release.AddRangeAsync(release);
                await statisticsDbContext.Subject.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.Filter.AddRangeAsync(subject1Filter, subject2Filter);
                await statisticsDbContext.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup, subject2IndicatorGroup);
                await statisticsDbContext.Observation.AddRangeAsync(
                    subject1Observation1,
                    subject1Observation2,
                    subject1Observation3,
                    subject2Observation1,
                    subject2Observation2,
                    subject2Observation3
                );
                await statisticsDbContext.SubjectFootnote.AddRangeAsync(subject1Footnote1);
                await statisticsDbContext.FilterFootnote.AddRangeAsync(subject1Footnote2);
                await statisticsDbContext.FilterGroupFootnote.AddRangeAsync(subject1Footnote3);
                await statisticsDbContext.FilterItemFootnote.AddRangeAsync(subject2Footnote1);
                await statisticsDbContext.IndicatorFootnote.AddRangeAsync(subject2Footnote2);
                await statisticsDbContext.AddRangeAsync(
                    releaseFootnote1,
                    releaseFootnote2,
                    releaseFootnote3,
                    releaseFootnote4,
                    releaseFootnote5
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Content.Model.Release
            {
                Id = release.Id
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    SubjectId = subject1.Id,
                    Filename = "file1.csv",
                    Type = FileType.Data,
                },
                Summary = "Data set 1 guidance"
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 2",
                File = new File
                {
                    SubjectId = subject2.Id,
                    Filename = "file2.csv",
                    Type = FileType.Data,
                },
                Summary = "Data set 2 guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext);

                var result = (await service.ListDataSets(release.Id)).AssertRight();

                // Assert there are two data sets with the correct content

                Assert.Equal(2, result.Count);

                Assert.Equal(releaseFile1.FileId, result[0].FileId);
                Assert.Equal("Data set 1 guidance", result[0].Content);
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

                Assert.Equal(releaseFile2.FileId, result[1].FileId);
                Assert.Equal("Data set 2 guidance", result[1].Content);
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
        public async Task ListDataSets_OrderedByName()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Release.AddRangeAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1,
                    releaseSubject2,
                    releaseSubject3);
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
                Name = "Data set 1",
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
                Name = "Data set 2",
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
                Name = "Data set 3",
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
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

                var result = await service.ListDataSets(release.Id);

                var viewModels = result.AssertRight();

                Assert.Equal(3, viewModels.Count);

                Assert.Equal("Data set 1", viewModels[0].Name);
                Assert.Equal("Data set 2", viewModels[1].Name);
                Assert.Equal("Data set 3", viewModels[2].Name);
            }
        }

        [Fact]
        public async Task ListDataSets_FilterDataSetsByFileId()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Release.AddRangeAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1,
                    releaseSubject2,
                    releaseSubject3);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Content.Model.Release
            {
                Id = release.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Data set 1",
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
                Name = "Data set 2",
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
                Name = "Data set 3",
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
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    releaseFile1, releaseFile2, releaseFile3);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext);

                var result = await service.ListDataSets(release.Id, new List<Guid>
                {
                    releaseFile1.FileId, releaseFile3.FileId
                });

                // Assert only the specified data sets are returned
                var viewModels = result.AssertRight();

                Assert.Equal(2, viewModels.Count);

                Assert.Equal(releaseFile1.FileId, viewModels[0].FileId);
                Assert.Equal(releaseFile3.FileId, viewModels[1].FileId);
            }
        }

        [Fact]
        public async Task ListDataSets_ReplacementInProgressIsIgnored()
        {
            var release = new Release();

            var originalSubject = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var replacementSubject = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Release.AddRangeAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(originalSubject,
                    replacementSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Content.Model.Release
            {
                Id = release.Id
            };

            var originalFile = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = originalSubject.SubjectId
                }
            };

            var replacementFile = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = replacementSubject.SubjectId,
                    Replacing = originalFile.File
                }
            };

           originalFile.File.ReplacedBy = replacementFile.File;

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.ReleaseFiles.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                var result = await service.ListDataSets(contentRelease.Id);

                var viewModels = result.AssertRight();

                // Replacement data sets should not be included when the replacement is still in progress
                Assert.Single(viewModels);
                Assert.Equal(originalFile.FileId, viewModels[0].FileId);
            }
        }

        [Fact]
        public async Task ListDataSets_NoRelease()
        {
            var service = SetupService();

            var result = await service.ListDataSets(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task ListDataSets_NoDataSets()
        {
            var release = new Content.Model.Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext);

                var result = await service.ListDataSets(release.Id);

                var viewModels = result.AssertRight();
                Assert.Empty(viewModels);
            }
        }

        [Fact]
        public async Task ListDataSets_AmendedRelease()
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

            // Version 1 has one data set, Version 2 adds another data set

            var releaseVersion1Subject1 = new ReleaseSubject
            {
                Release = releaseVersion1,
                Subject = subject1
            };

            var releaseVersion2Subject1 = new ReleaseSubject
            {
                Release = releaseVersion2,
                Subject = subject1
            };

            var releaseVersion2Subject2 = new ReleaseSubject
            {
                Release = releaseVersion2,
                Subject = subject2
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Release.AddRangeAsync(releaseVersion1, releaseVersion2);
                await statisticsDbContext.Subject.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseVersion1Subject1, releaseVersion2Subject1,
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
                Name = "Version 1 data set 1",
                Summary = "Version 1 data set 1 guidance"
            };

            var releaseVersion2File1 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = file1,
                Name = "Version 2 data set 1",
                Summary = "Version 2 data set 1 guidance"
            };

            var releaseVersion2File2 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = file2,
                Name = "Version 2 data set 2",
                Summary = "Version 2 data set 2 guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(
                    contentReleaseVersion1,
                    contentReleaseVersion2);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
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

                var version1Result = await service.ListDataSets(releaseVersion1.Id);
                var version1ViewModels = version1Result.AssertRight();

                // Assert version 1 has one data set with the correct content
                Assert.Single(version1ViewModels);

                Assert.Equal(releaseVersion1File1.FileId, version1ViewModels[0].FileId);
                Assert.Equal("Version 1 data set 1 guidance", version1ViewModels[0].Content);
                Assert.Equal("file1.csv", version1ViewModels[0].Filename);
                Assert.Equal("Version 1 data set 1", version1ViewModels[0].Name);
                Assert.Empty(version1ViewModels[0].TimePeriods.From);
                Assert.Empty(version1ViewModels[0].TimePeriods.To);
                Assert.Empty(version1ViewModels[0].GeographicLevels);
                Assert.Empty(version1ViewModels[0].Variables);

                var version2Result = await service.ListDataSets(releaseVersion2.Id);
                var version2ViewModels = version2Result.AssertRight();

                // Assert version 2 has two data sets with the correct content
                Assert.Equal(2, version2ViewModels.Count);

                Assert.Equal(releaseVersion2File1.FileId, version2ViewModels[0].FileId);
                Assert.Equal("Version 2 data set 1 guidance", version2ViewModels[0].Content);
                Assert.Equal("file1.csv", version2ViewModels[0].Filename);
                Assert.Equal("Version 2 data set 1", version2ViewModels[0].Name);
                Assert.Empty(version2ViewModels[0].TimePeriods.From);
                Assert.Empty(version2ViewModels[0].TimePeriods.To);
                Assert.Empty(version2ViewModels[0].GeographicLevels);
                Assert.Empty(version2ViewModels[0].Variables);

                Assert.Equal(releaseVersion2File2.FileId, version2ViewModels[1].FileId);
                Assert.Equal("Version 2 data set 2 guidance", version2ViewModels[1].Content);
                Assert.Equal("file2.csv", version2ViewModels[1].Filename);
                Assert.Equal("Version 2 data set 2", version2ViewModels[1].Name);
                Assert.Empty(version2ViewModels[1].TimePeriods.From);
                Assert.Empty(version2ViewModels[1].TimePeriods.To);
                Assert.Empty(version2ViewModels[1].GeographicLevels);
                Assert.Empty(version2ViewModels[1].Variables);
            }
        }

        [Fact]
        public async Task ListGeographicLevels()
        {
            var release = new Release();

            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
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
                await statisticsDbContext.Release.AddRangeAsync(release);
                await statisticsDbContext.Subject.AddRangeAsync(subject);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.Observation.AddRangeAsync(
                    subjectObservation1,
                    subjectObservation2,
                    subjectObservation3);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(statisticsDbContext: statisticsDbContext);

                var result = await service.ListGeographicLevels(subject.Id);

                Assert.Equal(3, result.Count);
                Assert.Equal("National", result[0]);
                Assert.Equal("Local authority", result[1]);
                Assert.Equal("Local authority district", result[2]);
            }
        }

        [Fact]
        public async Task ListGeographicLevels_NoObservations()
        {
            var release = new Release();

            var subject = new Subject();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Release.AddRangeAsync(release);
                await statisticsDbContext.Subject.AddRangeAsync(subject);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(statisticsDbContext: statisticsDbContext);

                var result = await service.ListGeographicLevels(subject.Id);

                Assert.Empty(result);
            }
        }

        private static DataGuidanceDataSetService SetupService(
            StatisticsDbContext? statisticsDbContext = null,
            ContentDbContext? contentDbContext = null,
            IIndicatorRepository? indicatorRepository = null,
            IFootnoteRepository? footnoteRepository = null,
            ITimePeriodService? timePeriodService = null)
        {
            statisticsDbContext ??= InMemoryStatisticsDbContext();
            contentDbContext ??= InMemoryContentDbContext();

            return new(
                statisticsDbContext,
                contentDbContext,
                indicatorRepository ?? new IndicatorRepository(statisticsDbContext),
                footnoteRepository ?? new FootnoteRepository(statisticsDbContext),
                timePeriodService ?? new TimePeriodService(statisticsDbContext)
            );
        }
    }
}
