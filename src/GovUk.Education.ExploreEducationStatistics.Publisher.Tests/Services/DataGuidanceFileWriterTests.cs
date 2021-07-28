#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Snapshooter.Xunit;
using Xunit;
using File = System.IO.File;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class DataGuidanceFileWriterTests : IDisposable
    {
        private const string TestMetaGuidance = @"
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

        private const string TestBasicMetaGuidance = @"
            <p>
                This document describes the data included in the ‘Children looked after in England'
            </p>
        ";

        private readonly List<string> _filePaths = new List<string>();

        public void Dispose()
        {
            // Cleanup any files that have been
            // written to the filesystem.
            _filePaths.ForEach(File.Delete);
        }

        [Fact]
        public async Task WriteFile_NoRelease()
        {
            var releaseId = Guid.NewGuid();

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(releaseId))
                .ThrowsAsync(new ArgumentException("Could not find release"));

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => { await writer.WriteFile(releaseId, path); }
            );

            Assert.Equal($"Could not find release", exception.Message);

            Assert.False(File.Exists(path));
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_ReleaseHasNoDataGuidance()
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
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => { await writer.WriteFile(release.Id, path); }
            );

            Assert.Equal(
                $"Cannot create data guidance file for release {release.Id} with no data guidance",
                exception.Message
            );

            Assert.False(File.Exists(path));
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_NoSubjects()
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
                MetaGuidance = TestMetaGuidance
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(new NotFoundResult());

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => { await writer.WriteFile(release.Id, path); }
            );

            Assert.Equal($"Could not find subjects for release: {release.Id}", exception.Message);

            Assert.False(File.Exists(path));
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_MultipleDataFiles()
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
                MetaGuidance = TestMetaGuidance
            };

            var subjects = new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
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
                        new LabelValue("Accommodation type", "accommodation_type"),
                        new LabelValue("Age at end", "age_end"),
                        new LabelValue("Number of leavers", "number_of_leavers"),
                        new LabelValue("Percentage of leavers by accommodation type", "percentage_of_leavers"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new FootnoteViewModel(Guid.NewGuid(), "Subject 1 Footnote 1"),
                        new FootnoteViewModel(Guid.NewGuid(), "Subject 1 Footnote 2"),
                    }
                },
                new MetaGuidanceSubjectViewModel
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
                        new LabelValue("Academic age", "age"),
                        new LabelValue("Activity", "category"),
                        new LabelValue("Gender", "gender"),
                        new LabelValue("Labour market status", "labour_market_status"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new FootnoteViewModel(Guid.NewGuid(), "Subject 2 Footnote 1"),
                        new FootnoteViewModel(Guid.NewGuid(), "Subject 2 Footnote 2"),
                        new FootnoteViewModel(Guid.NewGuid(), "Subject 2 Footnote 3"),
                        new FootnoteViewModel(Guid.NewGuid(), "Subject 2 Footnote 4"),
                    }
                }
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            await using var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_SingleDataFile()
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
                MetaGuidance = TestMetaGuidance
            };

            var subjects = new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
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
                        new LabelValue("Accommodation type", "accommodation_type"),
                        new LabelValue("Age at end", "age_end"),
                        new LabelValue("Number of leavers", "number_of_leavers"),
                        new LabelValue("Percentage of leavers by accommodation type", "percentage_of_leavers"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 1"),
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 2"),
                    }
                }
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_NoDataFiles()
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
                MetaGuidance = TestMetaGuidance
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(new List<MetaGuidanceSubjectViewModel>());

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            await using var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_FileWithSingleProperties()
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
                MetaGuidance = TestBasicMetaGuidance
            };

            var subjects = new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
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
                        new LabelValue("Accommodation type", "accommodation_type"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 1")
                    }
                }
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_FileWithEmptyProperties()
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
                MetaGuidance = TestBasicMetaGuidance
            };

            var subjects = new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                }
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_FileWithEmptyTimePeriodStart()
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
                MetaGuidance = TestBasicMetaGuidance
            };

            var subjects = new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = "",
                    GeographicLevels = new List<string>(),
                    TimePeriods = new TimePeriodLabels("", "2019"),
                    Variables = new List<LabelValue>()
                }
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_FileWithEmptyTimePeriodEnd()
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
                MetaGuidance = TestBasicMetaGuidance
            };

            var subjects = new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    TimePeriods = new TimePeriodLabels("2018", ""),
                }
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_FileWithEmptyVariable()
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
                MetaGuidance = TestBasicMetaGuidance
            };

            var subjects = new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    TimePeriods = new TimePeriodLabels("2018", ""),
                    Variables = new List<LabelValue>
                    {
                        new LabelValue("", "")
                    }
                }
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_FileWithOverTenFootnotesAndMultiline()
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
                MetaGuidance = TestBasicMetaGuidance
            };

            var subjects = new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = "",
                    GeographicLevels = new List<string>(),
                    TimePeriods = new TimePeriodLabels("2018", ""),
                    Variables = new List<LabelValue>
                    {
                        new LabelValue("Accommodation type", "accommodation_type"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 1"),
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 2"),
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 3"),
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 4"),
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 5"),
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 6"),
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 7"),
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 8"),
                        new FootnoteViewModel(
                            Guid.NewGuid(), string.Join("\n", "Footnote 9", "over some", "lines.")),
                        new FootnoteViewModel(Guid.NewGuid(), "Footnote 10"),
                        new FootnoteViewModel(
                            Guid.NewGuid(), string.Join("\n", "Footnote 11", "over some", "other lines.")),
                    }
                }
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        [Fact]
        public async Task WriteFile_FileWithEmptyFootnote()
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
                MetaGuidance = TestBasicMetaGuidance
            };

            var subjects = new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
                {
                    Filename = "test-1.csv",
                    Name = "Test data 1",
                    Content = "",
                    GeographicLevels = new List<string>(),
                    TimePeriods = new TimePeriodLabels("2018", ""),
                    Variables = new List<LabelValue>
                    {
                        new LabelValue("Accommodation type", "accommodation_type"),
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new FootnoteViewModel(Guid.NewGuid(), "")
                    }
                }
            };

            var releaseService = new Mock<IReleaseService>();

            releaseService
                .Setup(s => s.Get(release.Id))
                .ReturnsAsync(release);

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

            metaGuidanceSubjectService
                .Setup(s => s.GetSubjects(release.Id, null))
                .ReturnsAsync(subjects);

            var writer = BuildDataGuidanceFileWriter(
                releaseService: releaseService.Object,
                metaGuidanceSubjectService: metaGuidanceSubjectService.Object
            );

            var path = GenerateFilePath();
            var file = await writer.WriteFile(release.Id, path);

            using var reader = new StreamReader(file);
            var text = await File.ReadAllTextAsync(path);

            Assert.Equal(text, await reader.ReadToEndAsync());

            Snapshot.Match(text);
            MockUtils.VerifyAllMocks(releaseService, metaGuidanceSubjectService);
        }

        private string GenerateFilePath()
        {
            var path = Path.GetTempPath() + Guid.NewGuid() + ".txt";
            _filePaths.Add(path);

            return path;
        }

        private static DataGuidanceFileWriter BuildDataGuidanceFileWriter(
            IReleaseService? releaseService = null,
            IMetaGuidanceSubjectService? metaGuidanceSubjectService = null)
        {
            return new DataGuidanceFileWriter(
                releaseService ?? Mock.Of<IReleaseService>(),
                metaGuidanceSubjectService ?? Mock.Of<IMetaGuidanceSubjectService>()
            );
        }
    }
}