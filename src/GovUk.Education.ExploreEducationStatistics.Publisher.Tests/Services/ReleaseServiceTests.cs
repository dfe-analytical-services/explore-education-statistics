using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

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
            Summary = "The first topic"
        };

        private static readonly Publication PublicationA = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication A",
            Contact = Contact,
            Topic = Topic
        };

        private static readonly Publication PublicationB = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Publication B",
            Contact = Contact,
            Topic = Topic
        };

        private static readonly List<Publication> Publications = new List<Publication>
        {
            PublicationA, PublicationB
        };

        private static readonly Release PublicationARelease1 = new Release
        {
            Id = Guid.NewGuid(),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ1,
            RelatedInformation = new List<BasicLink>
            {
                new BasicLink
                {
                    Id = new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"),
                    Description = "Related Information",
                    Url = "http://example.com"
                }
            },
            Published = new DateTime(2019, 1, 01),
            Status = Approved
        };

        private static readonly Release PublicationARelease2 = new Release
        {
            Id = Guid.NewGuid(),
            PublicationId = PublicationA.Id,
            ReleaseName = "2018",
            TimePeriodCoverage = AcademicYearQ2,
            RelatedInformation = new List<BasicLink>
            {
                new BasicLink
                {
                    Id = new Guid("a0855237-b2f1-4dae-b2fc-027bb2802ba3"),
                    Description = "Related Information",
                    Url = "http://example.com"
                }
            },
            Published = new DateTime(2019, 1, 01),
            Status = Approved
        };

        private static readonly List<Release> Releases = new List<Release>
        {
            PublicationARelease1,
            PublicationARelease2,
            new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = PublicationA.Id,
                ReleaseName = "2017",
                TimePeriodCoverage = AcademicYearQ4,
                Published = new DateTime(2019, 1, 01),
                Status = Approved
            },
            new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = PublicationA.Id,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ3,
                Published = null,
                Status = Draft
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
                ReleaseId = PublicationARelease1.Id,
                ContentSectionId = Release1SummarySection.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1.Id,
                ContentSectionId = Release1Section1.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1.Id,
                ContentSectionId = Release1Section2.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1.Id,
                ContentSectionId = Release1Section3.Id
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1.Id,
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

        private static readonly IContentBlock Release1SummarySectionHtmlContentBlock1 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 summary 1 order 2</p>",
            Order = 2,
            ContentSectionId = Release1SummarySection.Id
        };

        private static readonly IContentBlock Release1SummarySectionHtmlContentBlock2 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 summary 2 order 0</p>",
            Order = 0,
            ContentSectionId = Release1SummarySection.Id
        };

        private static readonly IContentBlock Release1SummarySectionHtmlContentBlock3 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 summary 3 order 1</p>",
            Order = 1,
            ContentSectionId = Release1SummarySection.Id
        };

        private static readonly IContentBlock Release2SummarySectionHtmlContentBlock1 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 summary 1 order 2</p>",
            Order = 2,
            ContentSectionId = Release2SummarySection.Id
        };

        private static readonly IContentBlock Release2SummarySectionHtmlContentBlock2 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 summary 2 order 0</p>",
            Order = 0,
            ContentSectionId = Release2SummarySection.Id
        };

        private static readonly IContentBlock Release2SummarySectionHtmlContentBlock3 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 summary 3 order 1</p>",
            Order = 1,
            ContentSectionId = Release2SummarySection.Id
        };

        private static readonly IContentBlock Release1Section1HtmlContentBlock1 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 section 1 order 2</p>",
            Order = 2,
            ContentSectionId = Release1Section1.Id
        };

        private static readonly IContentBlock Release1Section1HtmlContentBlock2 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 section 1 order 0</p>",
            Order = 0,
            ContentSectionId = Release1Section1.Id
        };

        private static readonly IContentBlock Release1Section1HtmlContentBlock3 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 1 section 1 order 1</p>",
            Order = 1,
            ContentSectionId = Release1Section1.Id
        };
        
        private static readonly IContentBlock Release2Section1HtmlContentBlock1 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 section 1 order 2</p>",
            Order = 2,
            ContentSectionId = Release2Section1.Id
        };

        private static readonly IContentBlock Release2Section1HtmlContentBlock2 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 section 1 order 0</p>",
            Order = 0,
            ContentSectionId = Release2Section1.Id
        };

        private static readonly IContentBlock Release2Section1HtmlContentBlock3 = new HtmlBlock
        {
            Id = Guid.NewGuid(),
            Body = "<p>Release 2 section 1 order 1</p>",
            Order = 1,
            ContentSectionId = Release2Section1.Id
        };
        
        private static readonly IContentBlock Release1KeyStatsDataBlock = new DataBlock
        {
            Id = Guid.NewGuid(),
            ContentSectionId = Release1KeyStatsSection.Id
        };

        private static readonly IContentBlock Release2KeyStatsDataBlock = new DataBlock
        {
            Id = Guid.NewGuid(),
            ContentSectionId = Release2KeyStatsSection.Id
        };

        private static readonly List<IContentBlock> ContentBlocks = new List<IContentBlock>
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

        [Fact]
        public void GetLatestRelease()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: "LatestRelease");
            var options = builder.Options;

            var fileStorageService = new Mock<IFileStorageService>();

            using (var context = new ContentDbContext(options))
            {
                context.AddRange(Publications);
                context.AddRange(Releases);
                context.SaveChanges();
            }

            using (var context = new ContentDbContext(options))
            {
                var service = new ReleaseService(context, fileStorageService.Object,
                    MapperForProfile<MappingProfiles>());

                var result = service.GetLatestRelease(PublicationA.Id, Enumerable.Empty<Guid>());

                Assert.Equal(PublicationARelease2.Id, result.Id);
                Assert.Equal("Academic Year Q2 2018/19", result.Title);
            }
        }

        [Fact]
        public void GetLatestReleaseViewModel()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: "LatestReleaseViewModel");
            var options = builder.Options;

            var fileStorageService = new Mock<IFileStorageService>();

            using (var context = new ContentDbContext(options))
            {
                context.AddRange(Publications);
                context.AddRange(Releases);
                context.AddRange(ContentSections);
                context.AddRange(ReleaseContentSections);
                context.AddRange(ContentBlocks);
                context.SaveChanges();
            }

            using (var context = new ContentDbContext(options))
            {
                var service = new ReleaseService(context, fileStorageService.Object,
                    MapperForProfile<MappingProfiles>());

                var result = service.GetLatestReleaseViewModel(PublicationA.Id, Enumerable.Empty<Guid>());

                Assert.Equal(PublicationARelease2.Id, result.Id);
                Assert.Equal("Academic Year Q2 2018/19", result.Title);
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
                Assert.Equal("<p>Release 2 summary 2 order 0</p>", (summarySectionContent[0] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release2SummarySectionHtmlContentBlock3.Id, summarySectionContent[1].Id);
                Assert.Equal("<p>Release 2 summary 3 order 1</p>", (summarySectionContent[1] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release2SummarySectionHtmlContentBlock1.Id, summarySectionContent[2].Id);
                Assert.Equal("<p>Release 2 summary 1 order 2</p>", (summarySectionContent[2] as HtmlBlockViewModel)?.Body);

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

                Assert.Single(result.RelatedInformation);
                Assert.Equal(new Guid("a0855237-b2f1-4dae-b2fc-027bb2802ba3"), result.RelatedInformation[0].Id);
            }
        }

        [Fact]
        public void GetReleaseViewModel()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase("ReleaseViewModel");
            var options = builder.Options;

            var fileStorageService = new Mock<IFileStorageService>();

            using (var context = new ContentDbContext(options))
            {
                context.AddRange(Publications);
                context.AddRange(Releases);
                context.AddRange(ContentSections);
                context.AddRange(ReleaseContentSections);
                context.AddRange(ContentBlocks);
                context.SaveChanges();
            }

            using (var context = new ContentDbContext(options))
            {
                var service = new ReleaseService(context, fileStorageService.Object,
                    MapperForProfile<MappingProfiles>());

                var result = service.GetReleaseViewModel(PublicationARelease1.Id);

                Assert.Equal("Academic Year Q1 2018/19", result.Title);
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
                Assert.Equal("<p>Release 1 summary 2 order 0</p>", (summarySectionContent[0] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release1SummarySectionHtmlContentBlock3.Id, summarySectionContent[1].Id);
                Assert.Equal("<p>Release 1 summary 3 order 1</p>", (summarySectionContent[1] as HtmlBlockViewModel)?.Body);
                Assert.Equal(Release1SummarySectionHtmlContentBlock1.Id, summarySectionContent[2].Id);
                Assert.Equal("<p>Release 1 summary 1 order 2</p>", (summarySectionContent[2] as HtmlBlockViewModel)?.Body);

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

                Assert.Single(result.RelatedInformation);
                Assert.Equal(new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"), result.RelatedInformation[0].Id);
            }
        }
    }
}