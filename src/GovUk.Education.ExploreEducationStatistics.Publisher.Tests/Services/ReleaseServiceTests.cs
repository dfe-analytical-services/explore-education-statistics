using System;
using System.Collections.Generic;
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
            cfg.CreateMap<Release, ReleaseViewModel>();
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
                KeyStatisticsId = new Guid("4b3eba44-c9d9-455e-b4fd-a5d0d61b9c62"),
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
                KeyStatisticsId = new Guid("dd7c0651-8a75-4996-95c6-42fc4b82b3f8"),
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
                ReleaseId = new Guid("62ac9e2b-a0c3-42aa-9a10-d833777ad379"),
                Order = 1,
                Heading = "Content Section 1",
                Caption = ""
            },
            new ContentSection
            {
                Id = new Guid("a5638c29-9c54-4250-aa1a-d0fa4bce7240"),
                ReleaseId = new Guid("62ac9e2b-a0c3-42aa-9a10-d833777ad379"),
                Order = 2,
                Heading = "Content Section 2",
                Caption = ""
            },
            new ContentSection
            {
                Id = new Guid("99c28757-bf61-490e-a51f-98dd58afb578"),
                ReleaseId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                Order = 1,
                Heading = "Content Section 1",
                Caption = ""
            },
            new ContentSection
            {
                Id = new Guid("0b4b7ec8-98d7-4c75-ac97-bdab9bb51238"),
                ReleaseId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"),
                Order = 2,
                Heading = "Content Section 2",
                Caption = ""
            }
        };

        private static readonly List<DataBlock> DataBlocks = new List<DataBlock>
        {
            new DataBlock
            {
                Id = new Guid("4b3eba44-c9d9-455e-b4fd-a5d0d61b9c62")
            },
            new DataBlock
            {
                Id = new Guid("dd7c0651-8a75-4996-95c6-42fc4b82b3f8")
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

                var result = service.GetLatestRelease(new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"));

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
                context.AddRange(DataBlocks);
                context.SaveChanges();
            }

            using (var context = new ContentDbContext(options))
            {
                var service = new ReleaseService(context, fileStorageService.Object, _mapper);

                var result = service.GetRelease(new Guid("62ac9e2b-a0c3-42aa-9a10-d833777ad379"));

                Assert.Equal("Academic Year Q1", result.Title);
                Assert.Equal(new Guid("24fcd99c-0508-4437-91c4-90c777414ab9"), result.Publication.Id);
                Assert.Equal(new Guid("4b3eba44-c9d9-455e-b4fd-a5d0d61b9c62"), result.KeyStatistics.Id);

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