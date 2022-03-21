#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Snapshooter.Xunit;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = System.IO.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class DataGuidanceFileWriterTests : IDisposable
    {
        private const string TestDataGuidance = @"
            <h2>Description</h2>
            <p>
                This document describes the data included in the ‘Children looked after in England 
                including adoptions: 2019 to 2020’ National Statistics release’s underlying data files. 
                This data is released under the terms of the Open Government License and is intended to 
                meet at least three stars for Open Data.
            </p>
            <h2>Coverage</h2>
            <p>
                This data is based upon information collected in the SSDA903 (CLA) data collection. It is a 
                child level dataset collected from local authorities in England annually. The data includes 
                information on:
            </p>
            <ul>
                <li>children looked after at 31 March in each year, including unaccompanied asylum seeking children</li>
                <li>children looked after at any time</li>
                <li>children who started to be looked after</li>
                <li>children who ceased to be looked after</li>
            </ul>
            ";

        private const string TestBasicDataGuidance = @"
            <p>
                This document describes the data included in the ‘Children looked after in England'
            </p>
        ";

        private readonly List<string> _filePaths = new();

        public void Dispose()
        {
            // Cleanup any files that have been
            // written to the filesystem.
            _filePaths.ForEach(File.Delete);
        }

        [Fact]
        public async Task WriteToStream_NoSubjects()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestDataGuidance
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(new NotFoundResult());

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                var path = GenerateFilePath();
                await using var stream = File.OpenWrite(path);

                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    async () => { await writer.WriteToStream(stream, release); }
                );

                Assert.Equal($"Could not find subjects for release: {release.Id}", exception.Message);
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_MultipleDataFiles()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestDataGuidance
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = @"
                        <p>
                            Local authority level data on care leavers aged 17 to 21, by accommodation type (as 
                            measured on or around their birthday).
                        </p>",
                    GeographicLevels = new List<string>
                    {
                        "Local Authority",
                        "National",
                        "Regional"
                    },
                    TimePeriods = new TimePeriodLabels("2018", "2020"),
                    Variables = new List<LabelValue>
                    {
                        new("Accommodation type", "accommodation_type"),
                        new("Age at end", "age_end"),
                        new("Number of leavers", "number_of_leavers"),
                        new("Percentage of leavers by accommodation type", "percentage_of_leavers"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new(Guid.NewGuid(), "Subject 1 Footnote 1"),
                        new(Guid.NewGuid(), "Subject 1 Footnote 2"),
                    }
                },
                new()
                {
                    Filename = "test-2.csv",
                    Name = "Test data 2",
                    Content = @"
                        <p>
                            Number and proportion of population participating in education, training and employment 
                            by age, gender and labour market status.
                        </p>",
                    GeographicLevels = new List<string>
                    {
                        "National",
                    },
                    TimePeriods = new TimePeriodLabels("2018", "2018"),
                    Variables = new List<LabelValue>
                    {
                        new("Academic age", "age"),
                        new("Activity", "category"),
                        new("Gender", "gender"),
                        new("Labour market status", "labour_market_status"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new(Guid.NewGuid(), "Subject 2 Footnote 1"),
                        new(Guid.NewGuid(), "Subject 2 Footnote 2"),
                        new(Guid.NewGuid(), "Subject 2 Footnote 3"),
                        new(Guid.NewGuid(), "Subject 2 Footnote 4"),
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_SingleDataFile()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestDataGuidance
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = @"
                        <p>
                            Local authority level data on care leavers aged 17 to 21, by accommodation type (as 
                            measured on or around their birthday).
                        </p>",
                    GeographicLevels = new List<string>
                    {
                        "Local Authority",
                        "National",
                        "Regional"
                    },
                    TimePeriods = new TimePeriodLabels("2018", "2020"),
                    Variables = new List<LabelValue>
                    {
                        new("Accommodation type", "accommodation_type"),
                        new("Age at end", "age_end"),
                        new("Number of leavers", "number_of_leavers"),
                        new("Percentage of leavers by accommodation type", "percentage_of_leavers"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new(Guid.NewGuid(), "Footnote 1"),
                        new(Guid.NewGuid(), "Footnote 2"),
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_NoDataGuidance()
        {
            // Release has no data guidance (aka data guidance)
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = @"
                        <p>
                            Local authority level data on care leavers aged 17 to 21, by accommodation type (as 
                            measured on or around their birthday).
                        </p>",
                    GeographicLevels = new List<string>
                    {
                        "Local Authority",
                        "National",
                        "Regional"
                    },
                    TimePeriods = new TimePeriodLabels("2018", "2020"),
                    Variables = new List<LabelValue>
                    {
                        new("Accommodation type", "accommodation_type"),
                        new("Age at end", "age_end"),
                        new("Number of leavers", "number_of_leavers"),
                        new("Percentage of leavers by accommodation type", "percentage_of_leavers"),
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_NoDataFiles()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestDataGuidance
            };


            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(new List<DataGuidanceSubjectViewModel>());

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_FileWithSingleProperties()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestBasicDataGuidance
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = "<p>Test file content</p>",
                    GeographicLevels = new List<string>
                    {
                        "Local Authority",
                    },
                    TimePeriods = new TimePeriodLabels("2018", "2018"),
                    Variables = new List<LabelValue>
                    {
                        new("Accommodation type", "accommodation_type"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new(Guid.NewGuid(), "Footnote 1")
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_FileWithEmptyProperties()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestBasicDataGuidance
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_FileWithEmptyTimePeriodStart()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestBasicDataGuidance
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = "",
                    GeographicLevels = new List<string>(),
                    TimePeriods = new TimePeriodLabels("", "2019"),
                    Variables = new List<LabelValue>()
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_FileWithEmptyTimePeriodEnd()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestBasicDataGuidance
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    TimePeriods = new TimePeriodLabels("2018", ""),
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_FileWithEmptyVariable()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestBasicDataGuidance
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    TimePeriods = new TimePeriodLabels("2018", ""),
                    Variables = new List<LabelValue>
                    {
                        new("", "")
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_FileWithOverTenFootnotesAndMultiline()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestBasicDataGuidance
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = "",
                    GeographicLevels = new List<string>(),
                    TimePeriods = new TimePeriodLabels("2018", ""),
                    Variables = new List<LabelValue>
                    {
                        new("Accommodation type", "accommodation_type"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new(Guid.NewGuid(), "Footnote 1"),
                        new(Guid.NewGuid(), "Footnote 2"),
                        new(Guid.NewGuid(), "Footnote 3"),
                        new(Guid.NewGuid(), "Footnote 4"),
                        new(Guid.NewGuid(), "Footnote 5"),
                        new(Guid.NewGuid(), "Footnote 6"),
                        new(Guid.NewGuid(), "Footnote 7"),
                        new(Guid.NewGuid(), "Footnote 8"),
                        new(
                            Guid.NewGuid(),
                            string.Join("\n", "Footnote 9", "over some", "lines.")
                        ),
                        new(Guid.NewGuid(), "Footnote 10"),
                        new(
                            Guid.NewGuid(),
                            string.Join("\n", "Footnote 11", "over some", "other lines.")
                        ),
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_FileWithEmptyFootnote()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestBasicDataGuidance
            };

            var subjects = new List<DataGuidanceSubjectViewModel>
            {
                new()
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = "",
                    GeographicLevels = new List<string>(),
                    TimePeriods = new TimePeriodLabels("2018", ""),
                    Variables = new List<LabelValue>
                    {
                        new("Accommodation type", "accommodation_type"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new(Guid.NewGuid(), "")
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                await using var stream = new MemoryStream();
                await writer.WriteToStream(stream, release);

                Snapshot.Match(stream.ReadToEnd());
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteToStream_FileStream()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2020",
                TimePeriodCoverage = TimeIdentifier.ReportingYear,
                Publication = new Publication
                {
                    Title = "Test publication"
                },
                DataGuidance = TestBasicDataGuidance
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>();

            dataGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(new List<DataGuidanceSubjectViewModel>());

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.Entry(release).ReloadAsync();

                var writer = BuildDataGuidanceFileWriter(
                    contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object
                );

                var path = GenerateFilePath();
                await using var stream = File.OpenWrite(path);

                await writer.WriteToStream(stream, release);

                var text = await File.ReadAllTextAsync(path);

                Assert.Contains("Test publication", text);
            }

            MockUtils.VerifyAllMocks(dataGuidanceSubjectService);
        }

        private string GenerateFilePath()
        {
            var path = Path.GetTempPath() + Guid.NewGuid() + ".txt";
            _filePaths.Add(path);

            return path;
        }

        private static DataGuidanceFileWriter BuildDataGuidanceFileWriter(
            ContentDbContext contentDbContext,
            IDataGuidanceSubjectService? dataGuidanceSubjectService = null)
        {
            return new DataGuidanceFileWriter(
                contentDbContext,
                dataGuidanceSubjectService ?? Mock.Of<IDataGuidanceSubjectService>()
            );
        }
    }
}
