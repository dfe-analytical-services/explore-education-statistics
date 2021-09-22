using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class ReleaseServiceTests
    {
        private static readonly Contact Contact = new Contact
        {
            Id = Guid.NewGuid()
        };

        private static readonly Theme Theme = new Theme
        {
            Id = Guid.NewGuid(),
            Title = "Theme A",
            Slug = "theme-a",
            Summary = "The first theme"
        };

        private static readonly Topic Topic = new Topic
        {
            Id = Guid.NewGuid(),
            Title = "Topic A",
            ThemeId = Theme.Id,
            Slug = "topic-a",
        };

        private static readonly Publication PublicationA = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication A",
            Contact = Contact,
            Slug = "publication-a",
            Topic = Topic
        };

        private static readonly Publication PublicationB = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication B",
            Contact = Contact,
            Slug = "publication-b",
            Topic = Topic
        };

        private static readonly List<Publication> Publications = new List<Publication>
        {
            PublicationA, PublicationB
        };

        private static readonly Release PublicationARelease1V0 = new Release
        {
            Id = new Guid("36725e6b-8682-480b-a04a-0564253b7160"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            MetaGuidance = "Release 1 v0 Guidance",
            RelatedInformation = new List<Link>
            {
                new Link
                {
                    Id = new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"),
                    Description = "Related Information",
                    Url = "https://example.com"
                }
            },
            Slug = "2018-19-q1",
            Published = new DateTime(2019, 1, 1),
            ApprovalStatus = Approved,
            Version = 0,
            PreviousVersionId = null,
            SoftDeleted = false
        };

        private static readonly Release PublicationARelease1V1 = new Release
        {
            Id = new Guid("de6dc6ad-dc75-435c-9cf5-1ed4fe49c0cc"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            MetaGuidance = "Release 1 v1 Guidance",
            RelatedInformation = new List<Link>
            {
                new Link
                {
                    Id = new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"),
                    Description = "Related Information",
                    Url = "https://example.com"
                }
            },
            Slug = "2018-19-q1",
            Published = new DateTime(2019, 2, 1),
            ApprovalStatus = Approved,
            Updates = new List<Update>
            {
                new Update
                {
                    Id = Guid.NewGuid(),
                    On =  new DateTime(2020, 1, 1),
                    Reason = "First update"
                }
            },
            Version = 1,
            PreviousVersionId = new Guid("36725e6b-8682-480b-a04a-0564253b7160"),
            SoftDeleted = false,
        };

        private static readonly Release PublicationARelease1V2Deleted = new Release
        {
            Id = new Guid("6ac10729-e83f-4ed4-abc6-8d0efa62ccd2"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            MetaGuidance = "Release 1 v2 Guidance",
            RelatedInformation = new List<Link>
            {
                new Link
                {
                    Id = new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"),
                    Description = "Related Information",
                    Url = "https://example.com"
                }
            },
            Slug = "2018-19-q1",
            Published = null,
            ApprovalStatus = Approved,
            Updates = new List<Update>
            {
                new Update
                {
                    Id = Guid.NewGuid(),
                    On =  new DateTime(2020, 1, 1),
                    Reason = "First update"
                }
            },
            Version = 2,
            PreviousVersionId = new Guid("de6dc6ad-dc75-435c-9cf5-1ed4fe49c0cc"),
            SoftDeleted = true
        };

        private static readonly Release PublicationARelease1V3NotPublished = new Release
        {
            Id = new Guid("f5cbff36-4aa8-4e59-9621-b069ae8dee3e"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            MetaGuidance = "Release 1 v3 Guidance",
            RelatedInformation = new List<Link>(),
            Slug = "2018-19-q1",
            Published = null,
            ApprovalStatus = Approved,
            Updates = new List<Update>
            {
                new Update
                {
                    Id = Guid.NewGuid(),
                    On =  new DateTime(2020, 1, 1),
                    Reason = "First update"
                },
                new Update
                {
                    Id = Guid.NewGuid(),
                    On =  new DateTime(2020, 2, 1),
                    Reason = "Second update"
                }
            },
            Version = 3,
            PreviousVersionId = new Guid("de6dc6ad-dc75-435c-9cf5-1ed4fe49c0cc"),
            SoftDeleted = false
        };

        private static readonly Release PublicationARelease2 = new Release
        {
            Id = new Guid("e7e1aae3-a0a1-44b7-bdf3-3df4a363ce20"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ2,
            MetaGuidance = "Release 2 Guidance",
            RelatedInformation = new List<Link>
            {
                new Link
                {
                    Id = new Guid("a0855237-b2f1-4dae-b2fc-027bb2802ba3"),
                    Description = "Related Information",
                    Url = "https://example.com"
                }
            },
            Slug = "2018-19-q2",
            Published = new DateTime(2019, 3, 1),
            ApprovalStatus = Approved,
            Version = 0,
            PreviousVersionId = null
        };

        private static readonly Release PublicationARelease3 = new Release
        {
            Id = new Guid("2286f83d-c567-40f0-a7bd-f7cc5ca266ea"),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ3,
            MetaGuidance = "Release 3 Guidance",
            RelatedInformation = new List<Link>(),
            Slug = "2018-19-q3",
            Published = null,
            ApprovalStatus = Approved,
            Version = 0,
            PreviousVersionId = null
        };

        private static readonly List<Release> Releases = new List<Release>
        {
            PublicationARelease1V0,
            PublicationARelease1V1,
            PublicationARelease1V2Deleted,
            PublicationARelease1V3NotPublished,
            PublicationARelease2,
            PublicationARelease3,
            new Release
            {
                Id = new Guid("b647f4cd-4aba-47d9-ab8d-82ece32dca86"),
                PublicationId = PublicationA.Id,
                ReleaseName = "2017",
                TimePeriodCoverage = AcademicYearQ4,
                Slug = "2017-18-q4",
                Published = new DateTime(2019, 4, 1),
                ApprovalStatus = Approved,
                Version = 0,
                PreviousVersionId = null
            },
            new Release
            {
                Id = new Guid("21109205-6362-4746-bc37-0e6db2838173"),
                PublicationId = PublicationA.Id,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ4,
                Slug = "2018-19-q4",
                Published = null,
                ApprovalStatus = Draft,
                Version = 0,
                PreviousVersionId = null
            }
        };

        private static readonly ContentSection Release1SummarySection = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 1,
            Heading = "Release 1 summary section",
            Caption = "",
            Type = ContentSectionType.ReleaseSummary
        };

        private static readonly ContentSection Release1Section1 = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 2,
            Heading = "Release 1 section 1 order 2",
            Caption = "",
            Type = ContentSectionType.Generic
        };

        private static readonly ContentSection Release1Section2 = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 0,
            Heading = "Release 1 section 2 order 0",
            Caption = "",
            Type = ContentSectionType.Generic
        };

        private static readonly ContentSection Release1Section3 = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 1,
            Heading = "Release 1 section 3 order 1",
            Caption = "",
            Type = ContentSectionType.Generic
        };

        private static readonly ContentSection Release1KeyStatsSection = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 2,
            Heading = "Release 1 key stats section",
            Caption = "",
            Type = ContentSectionType.KeyStatistics
        };

        private static readonly ContentSection Release2SummarySection = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 1,
            Heading = "Release 2 summary section",
            Caption = "",
            Type = ContentSectionType.ReleaseSummary
        };

        private static readonly ContentSection Release2Section1 = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 2,
            Heading = "Release 2 section 1 order 2",
            Caption = "",
            Type = ContentSectionType.Generic
        };

        private static readonly ContentSection Release2Section2 = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 0,
            Heading = "Release 2 section 2 order 0",
            Caption = "",
            Type = ContentSectionType.Generic
        };

        private static readonly ContentSection Release2Section3 = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 1,
            Heading = "Release 2 section 3 order 1",
            Caption = "",
            Type = ContentSectionType.Generic
        };

        private static readonly ContentSection Release2KeyStatsSection = new ContentSection
        {
            Id = Guid.NewGuid(),
            Order = 2,
            Heading = "Release 2 key stats section",
            Caption = "",
            Type = ContentSectionType.KeyStatistics
        };

        private static readonly List<ContentSection> ContentSections = new List<ContentSection>
        {
            Release1SummarySection,
            Release1Section1,
            Release1Section2,
            Release1Section3,
            Release1KeyStatsSection,
            Release2SummarySection,
            Release2Section1,
            Release2Section2,
            Release2Section3,
            Release2KeyStatsSection
        };

        private static readonly List<ReleaseContentSection> ReleaseContentSections = new List<ReleaseContentSection>
        {
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1V1.Id,
                ContentSectionId = Release1SummarySection.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1V1.Id,
                ContentSectionId = Release1Section1.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1V1.Id,
                ContentSectionId = Release1Section2.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1V1.Id,
                ContentSectionId = Release1Section3.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1V1.Id,
                ContentSectionId = Release1KeyStatsSection.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease2.Id,
                ContentSectionId = Release2SummarySection.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease2.Id,
                ContentSectionId = Release2Section1.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease2.Id,
                ContentSectionId = Release2Section2.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease2.Id,
                ContentSectionId = Release2Section3.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease2.Id,
                ContentSectionId = Release2KeyStatsSection.Id
            }
        };

        private static readonly ContentBlock Release1SummarySectionHtmlContentBlock1 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 summary 1 order 2</p>",
            Order = 2,
            ContentSectionId = Release1SummarySection.Id
        };

        private static readonly ContentBlock Release1SummarySectionHtmlContentBlock2 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 summary 2 order 0</p>",
            Order = 0,
            ContentSectionId = Release1SummarySection.Id
        };

        private static readonly ContentBlock Release1SummarySectionHtmlContentBlock3 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 summary 3 order 1</p>",
            Order = 1,
            ContentSectionId = Release1SummarySection.Id
        };

        private static readonly ContentBlock Release2SummarySectionHtmlContentBlock1 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 summary 1 order 2</p>",
            Order = 2,
            ContentSectionId = Release2SummarySection.Id
        };

        private static readonly ContentBlock Release2SummarySectionHtmlContentBlock2 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 summary 2 order 0</p>",
            Order = 0,
            ContentSectionId = Release2SummarySection.Id
        };

        private static readonly ContentBlock Release2SummarySectionHtmlContentBlock3 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 summary 3 order 1</p>",
            Order = 1,
            ContentSectionId = Release2SummarySection.Id
        };

        private static readonly ContentBlock Release1Section1HtmlContentBlock1 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 section 1 order 2</p>",
            Order = 2,
            ContentSectionId = Release1Section1.Id
        };

        private static readonly ContentBlock Release1Section1HtmlContentBlock2 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 section 1 order 0</p>",
            Order = 0,
            ContentSectionId = Release1Section1.Id
        };

        private static readonly ContentBlock Release1Section1HtmlContentBlock3 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 section 1 order 1</p>",
            Order = 1,
            ContentSectionId = Release1Section1.Id
        };

        private static readonly ContentBlock Release2Section1HtmlContentBlock1 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 section 1 order 2</p>",
            Order = 2,
            ContentSectionId = Release2Section1.Id
        };

        private static readonly ContentBlock Release2Section1HtmlContentBlock2 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 section 1 order 0</p>",
            Order = 0,
            ContentSectionId = Release2Section1.Id
        };

        private static readonly ContentBlock Release2Section1HtmlContentBlock3 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 section 1 order 1</p>",
            Order = 1,
            ContentSectionId = Release2Section1.Id
        };

        private static readonly ContentBlock Release1KeyStatsDataBlock = new DataBlock
        {
            Id = Guid.NewGuid(),
            ContentSectionId = Release1KeyStatsSection.Id
        };

        private static readonly ContentBlock Release2KeyStatsDataBlock = new DataBlock
        {
            Id = Guid.NewGuid(),
            ContentSectionId = Release2KeyStatsSection.Id
        };

        private static readonly List<ContentBlock> ContentBlocks = new List<ContentBlock>
        {
            Release1SummarySectionHtmlContentBlock1,
            Release1SummarySectionHtmlContentBlock2,
            Release1SummarySectionHtmlContentBlock3,
            Release1Section1HtmlContentBlock1,
            Release1Section1HtmlContentBlock2,
            Release1Section1HtmlContentBlock3,
            Release1KeyStatsDataBlock,
            Release2SummarySectionHtmlContentBlock1,
            Release2SummarySectionHtmlContentBlock2,
            Release2SummarySectionHtmlContentBlock3,
            Release2Section1HtmlContentBlock1,
            Release2Section1HtmlContentBlock2,
            Release2Section1HtmlContentBlock3,
            Release2KeyStatsDataBlock
        };

        private static readonly ReleaseFile PublicationARelease2AncillaryReleaseFile = new ReleaseFile
        {
            Release = PublicationARelease2,
            Name = "Ancillary Test File",
            File = new File
            {
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        private static readonly ReleaseFile PublicationARelease2ChartReleaseFile = new ReleaseFile
        {
            Release = PublicationARelease2,
            File = new File
            {
                Filename = "chart.png",
                Type = Chart
            }
        };

        private static readonly ReleaseFile PublicationARelease2DataReleaseFile = new ReleaseFile
        {
            Release = PublicationARelease2,
            Name = "Data Test File",
            File = new File
            {
                Filename = "data.csv",
                Type = FileType.Data
            }
        };

        private static readonly List<ReleaseFile> ReleaseFiles = new List<ReleaseFile>
        {
            PublicationARelease2AncillaryReleaseFile,
            PublicationARelease2ChartReleaseFile,
            PublicationARelease2DataReleaseFile,
            new ReleaseFile
            {
                Release = PublicationARelease2,
                File = new File
                {
                    Filename = "data.meta.csv",
                    Type = Metadata
                }
            }
        };

        [Fact]
        public async Task GetDownloadFiles()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(Publications);
                await contentDbContext.AddRangeAsync(Releases);
                await contentDbContext.AddRangeAsync(ReleaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                publicBlobStorageService.Setup(s =>
                        s.CheckBlobExists(PublicReleaseFiles,
                            PublicationARelease2AncillaryReleaseFile.PublicPath()))
                    .ReturnsAsync(true);

                publicBlobStorageService.Setup(s =>
                        s.CheckBlobExists(PublicReleaseFiles,
                            PublicationARelease2DataReleaseFile.PublicPath()))
                    .ReturnsAsync(true);

                publicBlobStorageService.Setup(s =>
                        s.GetBlob(PublicReleaseFiles,
                            PublicationARelease2AncillaryReleaseFile.PublicPath()))
                    .ReturnsAsync(new BlobInfo
                    (
                        path: PublicationARelease2AncillaryReleaseFile.PublicPath(),
                        size: "15 Kb",
                        contentType: "application/pdf",
                        contentLength: 0L,
                        meta: new Dictionary<string, string>(),
                        created: null
                    ));

                publicBlobStorageService.Setup(s =>
                        s.GetBlob(PublicReleaseFiles,
                            PublicationARelease2DataReleaseFile.PublicPath()))
                    .ReturnsAsync(new BlobInfo
                    (
                        path: PublicationARelease2DataReleaseFile.PublicPath(),
                        size: "10 Mb",
                        contentType: "text/csv",
                        contentLength: 0L,
                        meta: GetDataFileMetaValues(
                            metaFileName: "data.meta.csv",
                            numberOfRows: 200),
                        created: null
                    ));


                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = await service.GetDownloadFiles(PublicationARelease2);

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(2, result.Count);
                Assert.Equal(PublicationARelease2AncillaryReleaseFile.File.Id, result[0].Id);
                Assert.Equal("pdf", result[0].Extension);
                Assert.Equal("ancillary.pdf", result[0].FileName);
                Assert.Equal("Ancillary Test File", result[0].Name);
                Assert.Equal("15 Kb", result[0].Size);
                Assert.Equal(Ancillary, result[0].Type);
                Assert.Equal(PublicationARelease2DataReleaseFile.File.Id, result[1].Id);
                Assert.Equal("csv", result[1].Extension);
                Assert.Equal("data.csv", result[1].FileName);
                Assert.Equal("Data Test File", result[1].Name);
                Assert.Equal("10 Mb", result[1].Size);
                Assert.Equal(FileType.Data, result[1].Type);
            }
        }

        [Fact]
        public async Task GetFiles()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(Publications);
                await contentDbContext.AddRangeAsync(Releases);
                await contentDbContext.AddRangeAsync(ReleaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext);

                var result = await service.GetFiles(PublicationARelease2.Id,
                    Ancillary,
                    Chart);

                Assert.Equal(2, result.Count);
                Assert.Equal(PublicationARelease2AncillaryReleaseFile.File.Id, result[0].Id);
                Assert.Equal(PublicationARelease2ChartReleaseFile.File.Id, result[1].Id);
            }
        }

        [Fact]
        public async Task GetLatestRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(Publications);
                await contentDbContext.AddRangeAsync(Releases);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext);

                var result = await service.GetLatestRelease(PublicationA.Id, Enumerable.Empty<Guid>());

                Assert.Equal(PublicationARelease2.Id, result.Id);
                Assert.Equal("Academic Year Q2 2018/19", result.Title);
            }
        }

        [Fact]
        public async Task GetLatestReleaseViewModel()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(Publications);
                await contentDbContext.AddRangeAsync(Releases);
                await contentDbContext.AddRangeAsync(ReleaseFiles);
                await contentDbContext.AddRangeAsync(ContentSections);
                await contentDbContext.AddRangeAsync(ReleaseContentSections);
                await contentDbContext.AddRangeAsync(ContentBlocks);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                publicBlobStorageService.Setup(s =>
                        s.CheckBlobExists(PublicReleaseFiles,
                            PublicationARelease2AncillaryReleaseFile.PublicPath()))
                    .ReturnsAsync(true);

                publicBlobStorageService.Setup(s =>
                        s.CheckBlobExists(PublicReleaseFiles,
                            PublicationARelease2DataReleaseFile.PublicPath()))
                    .ReturnsAsync(true);

                publicBlobStorageService.Setup(s =>
                        s.GetBlob(PublicReleaseFiles,
                            PublicationARelease2AncillaryReleaseFile.PublicPath()))
                    .ReturnsAsync(new BlobInfo
                    (
                        path: PublicationARelease2AncillaryReleaseFile.PublicPath(),
                        size: "15 Kb",
                        contentType: "application/pdf",
                        contentLength: 0L,
                        meta: new Dictionary<string, string>(),
                        created: null
                    ));

                publicBlobStorageService.Setup(s =>
                        s.GetBlob(PublicReleaseFiles,
                            PublicationARelease2DataReleaseFile.PublicPath()))
                    .ReturnsAsync(new BlobInfo
                    (
                        path: PublicationARelease2DataReleaseFile.PublicPath(),
                        size: "10 Mb",
                        contentType: "text/csv",
                        contentLength: 0L,
                        meta: GetDataFileMetaValues(
                            metaFileName: "data.meta.csv",
                            numberOfRows: 200),
                        created: null
                    ));

                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result =
                    await service.GetLatestReleaseViewModel(PublicationA.Id, Enumerable.Empty<Guid>(),
                        PublishContext());

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(PublicationARelease2.Id, result.Id);
                Assert.Equal("Academic Year Q2 2018/19", result.Title);
                Assert.Equal(PublicationARelease2.Published, result.Published);

                var keyStatisticsSection = result.KeyStatisticsSection;
                Assert.NotNull(keyStatisticsSection);
                Assert.Equal(Release2KeyStatsSection.Id, keyStatisticsSection.Id);
                var keyStatisticsSectionContent = keyStatisticsSection.Content;
                Assert.Single(keyStatisticsSectionContent);
                Assert.Equal(Release2KeyStatsDataBlock.Id, keyStatisticsSectionContent[0].Id);

                var summarySection = result.SummarySection;
                Assert.NotNull(summarySection);
                Assert.Equal(Release2SummarySection.Id, summarySection.Id);
                var summarySectionContent = summarySection.Content;
                Assert.NotNull(summarySectionContent);
                Assert.Equal(3, summarySectionContent.Count);
                Assert.Equal(Release2SummarySectionHtmlContentBlock2.Id, summarySectionContent[0].Id);
                Assert.Equal("<p>Release 2 summary 2 order 0</p>",
                    (summarySectionContent[0] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release2SummarySectionHtmlContentBlock3.Id, summarySectionContent[1].Id);
                Assert.Equal("<p>Release 2 summary 3 order 1</p>",
                    (summarySectionContent[1] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release2SummarySectionHtmlContentBlock1.Id, summarySectionContent[2].Id);
                Assert.Equal("<p>Release 2 summary 1 order 2</p>",
                    (summarySectionContent[2] as HtmlBlockViewModel)?.Body);

                var content = result.Content;

                Assert.NotNull(content);
                Assert.Equal(3, content.Count);

                Assert.Equal(Release2Section2.Id, content[0].Id);
                Assert.Equal("Release 2 section 2 order 0", content[0].Heading);
                Assert.Empty(content[0].Content);

                Assert.Equal(Release2Section3.Id, content[1].Id);
                Assert.Equal("Release 2 section 3 order 1", content[1].Heading);
                Assert.Empty(content[1].Content);

                Assert.Equal(Release2Section1.Id, content[2].Id);
                Assert.Equal("Release 2 section 1 order 2", content[2].Heading);
                Assert.Equal(3, content[2].Content.Count);
                Assert.Equal(Release2Section1HtmlContentBlock2.Id, content[2].Content[0].Id);
                Assert.Equal("<p>Release 2 section 1 order 0</p>", (content[2].Content[0] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release2Section1HtmlContentBlock3.Id, content[2].Content[1].Id);
                Assert.Equal("<p>Release 2 section 1 order 1</p>", (content[2].Content[1] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release2Section1HtmlContentBlock1.Id, content[2].Content[2].Id);
                Assert.Equal("<p>Release 2 section 1 order 2</p>", (content[2].Content[2] as HtmlBlockViewModel)?.Body);

                Assert.Equal(2, result.DownloadFiles.Count);
                Assert.Equal(PublicationARelease2AncillaryReleaseFile.File.Id, result.DownloadFiles[0].Id);
                Assert.Equal("pdf", result.DownloadFiles[0].Extension);
                Assert.Equal("ancillary.pdf", result.DownloadFiles[0].FileName);
                Assert.Equal("Ancillary Test File", result.DownloadFiles[0].Name);
                Assert.Equal("15 Kb", result.DownloadFiles[0].Size);
                Assert.Equal(Ancillary, result.DownloadFiles[0].Type);
                Assert.Equal(PublicationARelease2DataReleaseFile.File.Id, result.DownloadFiles[1].Id);
                Assert.Equal("csv", result.DownloadFiles[1].Extension);
                Assert.Equal("data.csv", result.DownloadFiles[1].FileName);
                Assert.Equal("Data Test File", result.DownloadFiles[1].Name);
                Assert.Equal("10 Mb", result.DownloadFiles[1].Size);
                Assert.Equal(FileType.Data, result.DownloadFiles[1].Type);

                Assert.Equal("Release 2 Guidance", result.MetaGuidance);

                Assert.Single(result.RelatedInformation);
                Assert.Equal(new Guid("a0855237-b2f1-4dae-b2fc-027bb2802ba3"), result.RelatedInformation[0].Id);
            }
        }

        [Fact]
        public async Task GetReleaseViewModel()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(Publications);
                await contentDbContext.AddRangeAsync(Releases);
                await contentDbContext.AddRangeAsync(ReleaseFiles);
                await contentDbContext.AddRangeAsync(ContentSections);
                await contentDbContext.AddRangeAsync(ReleaseContentSections);
                await contentDbContext.AddRangeAsync(ContentBlocks);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = await service.GetReleaseViewModel(PublicationARelease1V1.Id, PublishContext());

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(PublicationARelease1V1.Id, result.Id);
                Assert.Equal("Academic Year Q1 2018/19", result.Title);
                Assert.Equal(PublicationARelease1V1.Published, result.Published);

                var keyStatisticsSection = result.KeyStatisticsSection;
                Assert.NotNull(keyStatisticsSection);
                Assert.Equal(Release1KeyStatsSection.Id, keyStatisticsSection.Id);
                var keyStatisticsSectionContent = keyStatisticsSection.Content;
                Assert.Single(keyStatisticsSectionContent);
                Assert.Equal(Release1KeyStatsDataBlock.Id, keyStatisticsSectionContent[0].Id);

                var summarySection = result.SummarySection;
                Assert.NotNull(summarySection);
                Assert.Equal(Release1SummarySection.Id, summarySection.Id);
                var summarySectionContent = summarySection.Content;
                Assert.NotNull(summarySectionContent);
                Assert.Equal(3, summarySectionContent.Count);
                Assert.Equal(Release1SummarySectionHtmlContentBlock2.Id, summarySectionContent[0].Id);
                Assert.Equal("<p>Release 1 summary 2 order 0</p>",
                    (summarySectionContent[0] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release1SummarySectionHtmlContentBlock3.Id, summarySectionContent[1].Id);
                Assert.Equal("<p>Release 1 summary 3 order 1</p>",
                    (summarySectionContent[1] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release1SummarySectionHtmlContentBlock1.Id, summarySectionContent[2].Id);
                Assert.Equal("<p>Release 1 summary 1 order 2</p>",
                    (summarySectionContent[2] as HtmlBlockViewModel)?.Body);

                var content = result.Content;
                Assert.NotNull(content);
                Assert.Equal(3, content.Count);

                Assert.Equal(Release1Section2.Id, content[0].Id);
                Assert.Equal("Release 1 section 2 order 0", content[0].Heading);
                Assert.Empty(content[0].Content);

                Assert.Equal(Release1Section3.Id, content[1].Id);
                Assert.Equal("Release 1 section 3 order 1", content[1].Heading);
                Assert.Empty(content[1].Content);

                Assert.Equal(Release1Section1.Id, content[2].Id);
                Assert.Equal("Release 1 section 1 order 2", content[2].Heading);
                Assert.Equal(3, content[2].Content.Count);
                Assert.Equal(Release1Section1HtmlContentBlock2.Id, content[2].Content[0].Id);
                Assert.Equal("<p>Release 1 section 1 order 0</p>", (content[2].Content[0] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release1Section1HtmlContentBlock3.Id, content[2].Content[1].Id);
                Assert.Equal("<p>Release 1 section 1 order 1</p>", (content[2].Content[1] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release1Section1HtmlContentBlock1.Id, content[2].Content[2].Id);
                Assert.Equal("<p>Release 1 section 1 order 2</p>", (content[2].Content[2] as HtmlBlockViewModel)?.Body);

                Assert.Single(result.Updates);
                Assert.Equal(new DateTime(2020, 1, 1), result.Updates[0].On);
                Assert.Equal("First update", result.Updates[0].Reason);

                Assert.Empty(result.DownloadFiles);

                Assert.Equal("Release 1 v1 Guidance", result.MetaGuidance);

                Assert.Single(result.RelatedInformation);
                Assert.Equal(new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"), result.RelatedInformation[0].Id);
            }
        }

        [Fact]
        public async Task GetReleaseViewModel_NotYetPublished()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(Publications);
                await contentDbContext.AddRangeAsync(Releases);
                await contentDbContext.AddRangeAsync(ReleaseFiles);
                await contentDbContext.AddRangeAsync(ContentSections);
                await contentDbContext.AddRangeAsync(ReleaseContentSections);
                await contentDbContext.AddRangeAsync(ContentBlocks);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var context = PublishContext();
                var result = await service.GetReleaseViewModel(PublicationARelease3.Id, context);

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(PublicationARelease3.Id, result.Id);
                Assert.Equal("Academic Year Q3 2018/19", result.Title);
                // Published date in the view model should match the date set up in the publishing context
                Assert.Equal(context.Published, result.Published);
                Assert.Null(result.KeyStatisticsSection);
                Assert.Null(result.SummarySection);
                Assert.Empty(result.Content);

                Assert.Empty(result.DownloadFiles);

                Assert.Equal("Release 3 Guidance", result.MetaGuidance);

                Assert.Empty(result.RelatedInformation);
            }
        }

        [Fact]
        public async Task GetReleaseViewModel_AmendedReleaseNotYetPublishedHasPublishedDateOfPreviousVersion()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(Publications);
                await contentDbContext.AddRangeAsync(Releases);
                await contentDbContext.AddRangeAsync(ReleaseFiles);
                await contentDbContext.AddRangeAsync(ContentSections);
                await contentDbContext.AddRangeAsync(ReleaseContentSections);
                await contentDbContext.AddRangeAsync(ContentBlocks);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var context = PublishContext();
                var result = await service.GetReleaseViewModel(PublicationARelease1V3NotPublished.Id, context);

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(PublicationARelease1V3NotPublished.Id, result.Id);
                Assert.Equal("Academic Year Q1 2018/19", result.Title);
                // Published date in the view model should match the published date of the previous version
                Assert.Equal(PublicationARelease1V1.Published, result.Published);
                Assert.Null(result.KeyStatisticsSection);
                Assert.Null(result.SummarySection);
                Assert.Empty(result.Content);

                Assert.Empty(result.DownloadFiles);

                Assert.Equal("Release 1 v3 Guidance", result.MetaGuidance);

                Assert.Empty(result.RelatedInformation);
            }
        }

        [Fact]
        public async Task GetReleaseViewModel_AmendedReleaseNotYetPublishedHasUpdatesInDescendingOrder()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(Publications);
                await contentDbContext.AddRangeAsync(Releases);
                await contentDbContext.AddRangeAsync(ReleaseFiles);
                await contentDbContext.AddRangeAsync(ContentSections);
                await contentDbContext.AddRangeAsync(ReleaseContentSections);
                await contentDbContext.AddRangeAsync(ContentBlocks);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var context = PublishContext();
                var result = await service.GetReleaseViewModel(PublicationARelease1V3NotPublished.Id, context);

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(PublicationARelease1V3NotPublished.Id, result.Id);
                Assert.Equal("Academic Year Q1 2018/19", result.Title);

                Assert.Equal(2, result.Updates.Count);
                Assert.Equal(new DateTime(2020, 2, 1), result.Updates[0].On);
                Assert.Equal("Second update", result.Updates[0].Reason);

                Assert.Equal(new DateTime(2020, 1, 1), result.Updates[1].On);
                Assert.Equal("First update", result.Updates[1].Reason);
            }
        }

        [Fact]
        public async Task SetPublishedDates()
        {
            var contentRelease = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                Publication = new Publication(),
                Version = 0
            };

            var statisticsRelease = new Data.Model.Release
            {
                Id = contentRelease.Id
            };

            var published = DateTime.UtcNow;

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryPublicStatisticsDbContext(statisticsDbContextId))
            {
                await contentDbContext.Releases.AddAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.Release.AddAsync(statisticsRelease);
                await statisticsDbContext.SaveChangesAsync();
            }

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock =>
                    mock.SetPublishedDatesByPublication(contentRelease.PublicationId, published))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryPublicStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    methodologyService: methodologyService.Object);

                await service.SetPublishedDates(contentRelease.Id, published);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryPublicStatisticsDbContext(statisticsDbContextId))
            {
                var actualContentRelease = await contentDbContext.Releases
                    .Include(r => r.Publication)
                    .SingleAsync(r => r.Id == contentRelease.Id);
                var actualStatisticsRelease = await statisticsDbContext.Release.FindAsync(contentRelease.Id);

                Assert.NotNull(actualContentRelease);
                Assert.NotNull(actualStatisticsRelease);

                Assert.Equal(published, actualContentRelease.Published);
                Assert.Equal(published, actualContentRelease.Publication.Published);
                Assert.Equal(published, actualStatisticsRelease.Published);

                Assert.True(actualContentRelease.DataLastPublished.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(actualContentRelease.DataLastPublished.Value).Milliseconds, 0,
                    1500);
            }

            MockUtils.VerifyAllMocks(methodologyService);
        }

        [Fact] public async Task SetPublishedDates_ReleaseHasNoStatisticsData()
        {
            var contentRelease = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                Publication = new Publication(),
                Version = 0
            };

            var published = DateTime.UtcNow;

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock =>
                    mock.SetPublishedDatesByPublication(contentRelease.PublicationId, published))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryPublicStatisticsDbContext())
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    methodologyService: methodologyService.Object);

                await service.SetPublishedDates(contentRelease.Id, published);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var actualContentRelease = await contentDbContext.Releases
                    .Include(r => r.Publication)
                    .SingleAsync(r => r.Id == contentRelease.Id);

                Assert.NotNull(actualContentRelease);

                Assert.Equal(published, actualContentRelease.Published);
                Assert.Equal(published, actualContentRelease.Publication.Published);

                Assert.True(actualContentRelease.DataLastPublished.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(actualContentRelease.DataLastPublished.Value).Milliseconds, 0,
                    1500);
            }

            MockUtils.VerifyAllMocks(methodologyService);
        }

        [Fact]
        public async Task SetPublishedDates_AmendedReleaseHasPublishedDateOfPreviousVersion()
        {
            var publication = new Publication();

            var previousContentRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddDays(-1),
                PreviousVersionId = null,
                Version = 0
            };

            var contentRelease = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousContentRelease.Id,
                Publication = publication,
                Version = 1
            };

            var statisticsRelease = new Data.Model.Release
            {
                Id = contentRelease.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryPublicStatisticsDbContext(statisticsDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(previousContentRelease, contentRelease);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.Release.AddAsync(statisticsRelease);
                await statisticsDbContext.SaveChangesAsync();
            }

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);

            methodologyService.Setup(mock =>
                    mock.SetPublishedDatesByPublication(contentRelease.PublicationId, previousContentRelease.Published.Value))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryPublicStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    methodologyService: methodologyService.Object);

                await service.SetPublishedDates(contentRelease.Id, DateTime.UtcNow);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryPublicStatisticsDbContext(statisticsDbContextId))
            {
                var actualContentRelease = await contentDbContext.Releases
                    .Include(r => r.Publication)
                    .SingleAsync(r => r.Id == contentRelease.Id);
                var actualStatisticsRelease = await statisticsDbContext.Release.FindAsync(contentRelease.Id);

                Assert.NotNull(actualContentRelease);
                Assert.NotNull(actualStatisticsRelease);

                Assert.Equal(previousContentRelease.Published.Value, actualContentRelease.Published);
                Assert.Equal(previousContentRelease.Published.Value, actualContentRelease.Publication.Published);
                Assert.Equal(previousContentRelease.Published.Value, actualStatisticsRelease.Published);

                Assert.True(actualContentRelease.DataLastPublished.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(actualContentRelease.DataLastPublished.Value).Milliseconds, 0,
                    1500);
            }

            MockUtils.VerifyAllMocks(methodologyService);
        }

        private static PublishContext PublishContext()
        {
            var published = DateTime.Today.Add(new TimeSpan(9, 30, 0));
            return new PublishContext(published, true);
        }

        private static ReleaseService BuildReleaseService(
            ContentDbContext contentDbContext,
            PublicStatisticsDbContext statisticsDbContext = null,
            IBlobStorageService publicBlobStorageService = null,
            IMethodologyService methodologyService = null,
            IReleaseSubjectRepository releaseSubjectRepository = null)
        {
            return new ReleaseService(
                contentDbContext,
                statisticsDbContext ?? new Mock<PublicStatisticsDbContext>().Object,
                publicBlobStorageService ?? new Mock<IBlobStorageService>().Object,
                methodologyService ?? new Mock<IMethodologyService>().Object,
                releaseSubjectRepository ?? new Mock<IReleaseSubjectRepository>().Object,
                new Mock<ILogger<ReleaseService>>().Object,
                MapperForProfile<MappingProfiles>());
        }
    }
}
