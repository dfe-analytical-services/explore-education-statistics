using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class ReleaseServiceTests
    {
        private readonly MapperConfiguration _config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfiles());
        });

        private static readonly Contact Contact = new Contact
        {
            Id = new Guid("9bf9cc0b-e85f-466b-b90f-354944dcc82e")
        };

        private static readonly Topic Topic = new Topic
        {
            Id = new Guid("7b781f47-d305-4204-b7cd-ea49c23273fa"),
            Theme = new Theme
            {
                Id = new Guid("1982dec8-b724-4e15-887d-f473fe667826"),
                Title = "Title X"
            }
        };

        private static readonly List<Publication> Publications = new List<Publication>
        {
            new Publication
            {
                Id = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                Title = "Publication A",
                Contact = Contact,
                Topic = Topic
            },
            new Publication
            {
                Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                Title = "Publication B",
                Contact = Contact,
                Topic = Topic
            }
        };

        private static readonly List<Release> Releases = new List<Release>
        {
            new Release
            {
                Id = new Guid("62ac9e2b-a0c3-42aa-9a10-d833777ad379"),
                PublicationId = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
                TimePeriodCoverage = TimeIdentifier.AcademicYearQ1,
                RelatedInformation = new List<BasicLink>
                {
                    new BasicLink
                    {
                        Id = new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"),
                        Description = "Related Information",
                        Url = "http://example.com"
                    }
                }
            },
            new Release
            {
                Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                PublicationId = new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"),
		RelatedInformation = new List<BasicLink>
                {
                    new BasicLink
                    {
                        Id = new Guid("9eb283bd-4f28-4e65-bc91-1da9cc6567f9"),
                        Description = "Related Information",
                        Url = "http://example.com"
                    }
                }
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
                ReleaseId = new Guid("62ac9e2b-a0c3-42aa-9a10-d833777ad379"),
                ContentSectionId = new Guid("f29b4729-2061-4908-ba35-4a7d2c2291cd")
            },
            new ReleaseContentSection
            {
                ReleaseId = new Guid("62ac9e2b-a0c3-42aa-9a10-d833777ad379"),
                ContentSectionId = new Guid("a5638c29-9c54-4250-aa1a-d0fa4bce7240")
            },
            new ReleaseContentSection
            {
                ReleaseId = new Guid("62ac9e2b-a0c3-42aa-9a10-d833777ad379"),
                ContentSectionId = new Guid("cd30ff69-2eb1-4898-bc1b-46c50586c2e5")
            },
            new ReleaseContentSection
            {
                ReleaseId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                ContentSectionId = new Guid("99c28757-bf61-490e-a51f-98dd58afb578")
            },
            new ReleaseContentSection
            {
                ReleaseId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                ContentSectionId = new Guid("0b4b7ec8-98d7-4c75-ac97-bdab9bb51238")
            },
            new ReleaseContentSection
            {
                ReleaseId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
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

        private readonly IMapper _mapper;

        public ReleaseServiceTests()
        {
            _mapper = _config.CreateMapper();
        }

        [Fact]
        public void GetLatest_ReturnsA_WithARelease()
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: "FindLatestPublication");
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
                var service = new ReleaseService(context, fileStorageService.Object, _mapper);

                var result = service.GetLatestReleaseViewModel(new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), Enumerable.Empty<Guid>());

                Assert.IsType<ReleaseViewModel>(result);
            }
        }

        [Fact]
        public void GetId_ReturnsA_WithATheme()
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
                var service = new ReleaseService(context, fileStorageService.Object, _mapper);

                var result = service.GetReleaseViewModel(new Guid("62ac9e2b-a0c3-42aa-9a10-d833777ad379"));

                Assert.Equal("Academic Year Q1", result.Title);
                Assert.Equal(new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), result.Publication.Id);
                Assert.Equal(new Guid("4b3eba44-c9d9-455e-b4fd-a5d0d61b9c62"), result.KeyStatisticsSection.Content[0].Id);

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