using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
            Summary = "The first theme",
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
                    Id = new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"),
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

        private static readonly List<ContentSection> ContentSections = new List<ContentSection>
        {
            new ContentSection
            {
                Id = new Guid("f29b4729-2061-4908-ba35-4a7d2c2291cd"),
                Order = 1,
                Heading = "Content Section 1",
                Caption = "",
                Type = ContentSectionType.Generic
            },
            new ContentSection
            {
                Id = new Guid("a5638c29-9c54-4250-aa1a-d0fa4bce7240"),
                Order = 2,
                Heading = "Content Section 2",
                Caption = "",
                Type = ContentSectionType.Generic
            },
            new ContentSection
            {
                Id = new Guid("cd30ff69-2eb1-4898-bc1b-46c50586c2e5"),
                Order = 2,
                Heading = "Content Section 3",
                Caption = "",
                Type = ContentSectionType.KeyStatistics
            },
            new ContentSection
            {
                Id = new Guid("99c28757-bf61-490e-a51f-98dd58afb578"),
                Order = 1,
                Heading = "Content Section 1",
                Caption = "",
                Type = ContentSectionType.Generic
            },
            new ContentSection
            {
                Id = new Guid("0b4b7ec8-98d7-4c75-ac97-bdab9bb51238"),
                Order = 2,
                Heading = "Content Section 2",
                Caption = "",
                Type = ContentSectionType.Generic
            },
            new ContentSection
            {
                Id = new Guid("16c3fcfe-000e-4a36-a3ce-c058d7e5c766"),
                Order = 2,
                Heading = "Content Section 3",
                Caption = "",
                Type = ContentSectionType.KeyStatistics
            }
        };

        private static readonly List<ReleaseContentSection> ReleaseContentSections = new List<ReleaseContentSection>
        {
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1.Id,
                ContentSectionId = new Guid("f29b4729-2061-4908-ba35-4a7d2c2291cd")
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1.Id,
                ContentSectionId = new Guid("a5638c29-9c54-4250-aa1a-d0fa4bce7240")
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease1.Id,
                ContentSectionId = new Guid("cd30ff69-2eb1-4898-bc1b-46c50586c2e5")
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease2.Id,
                ContentSectionId = new Guid("99c28757-bf61-490e-a51f-98dd58afb578")
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease2.Id,
                ContentSectionId = new Guid("0b4b7ec8-98d7-4c75-ac97-bdab9bb51238")
            },
            new ReleaseContentSection
            {
                ReleaseId = PublicationARelease2.Id,
                ContentSectionId = new Guid("16c3fcfe-000e-4a36-a3ce-c058d7e5c766")
            }
        };

        private static readonly List<DataBlock> DataBlocks = new List<DataBlock>
        {
            new DataBlock
            {
                Id = new Guid("4b3eba44-c9d9-455e-b4fd-a5d0d61b9c62"),
                ContentSectionId = new Guid("cd30ff69-2eb1-4898-bc1b-46c50586c2e5")
            },
            new DataBlock
            {
                Id = new Guid("dd7c0651-8a75-4996-95c6-42fc4b82b3f8"),
                ContentSectionId = new Guid("16c3fcfe-000e-4a36-a3ce-c058d7e5c766")
            }
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
        public void GetLatestReleaseViewModel_ReturnsA_WithARelease()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: "LatestReleaseViewModel");
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

                var result = service.GetLatestReleaseViewModel(PublicationA.Id, Enumerable.Empty<Guid>());

                Assert.Equal(PublicationARelease2.Id, result.Id);
                Assert.Equal("Academic Year Q2 2018/19", result.Title);
            }
        }

        [Fact]
        public void GetReleaseViewModel_ReturnsA_WithATheme()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase("ReleaseTheme");
            var options = builder.Options;

            var fileStorageService = new Mock<IFileStorageService>();

            using (var context = new ContentDbContext(options))
            {
                context.AddRange(Publications);
                context.AddRange(Releases);
                context.AddRange(ContentSections);
                context.AddRange(ReleaseContentSections);
                context.AddRange(DataBlocks);
                context.SaveChanges();
            }

            using (var context = new ContentDbContext(options))
            {
                var service = new ReleaseService(context, fileStorageService.Object,
                    MapperForProfile<MappingProfiles>());

                var result = service.GetReleaseViewModel(PublicationARelease1.Id);

                Assert.Equal("Academic Year Q1 2018/19", result.Title);
                Assert.Equal(PublicationA.Id, result.Publication.Id);
                Assert.Equal(new Guid("4b3eba44-c9d9-455e-b4fd-a5d0d61b9c62"),
                    result.KeyStatisticsSection.Content[0].Id);

                var content = result.Content;
                Assert.Equal(2, content.Count);
                Assert.Equal(new Guid("f29b4729-2061-4908-ba35-4a7d2c2291cd"), content[0].Id);
                Assert.Equal(new Guid("a5638c29-9c54-4250-aa1a-d0fa4bce7240"), content[1].Id);

                Assert.Single(result.RelatedInformation);
                Assert.Equal(new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"), result.RelatedInformation[0].Id);
            }
        }
    }
}