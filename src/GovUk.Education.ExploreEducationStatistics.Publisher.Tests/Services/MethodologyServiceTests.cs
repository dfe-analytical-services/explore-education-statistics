using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class MethodologyServiceTests
    {
        [Fact]
        public async Task GetTree()
        {
            var topicA = new Topic
            {
                Title = "Topic A",
                Theme = new Theme
                {
                    Title = "Theme A",
                    Slug = "theme-a",
                    Summary = "The first theme"
                },
                Slug = "topic-a"
            };

            var methodologyA = new Methodology
            {
                Slug = "methodology-a",
                Title = "Methodology A",
                Summary = "first methodology",
                Published = new DateTime(2019, 1, 01),
                Updated = new DateTime(2019, 1, 15),
                Annexes = new List<ContentSection>(),
                Content = new List<ContentSection>()
            };

            var methodologyB = new Methodology
            {
                Slug = "methodology-b",
                Title = "Methodology B",
                Summary = "second methodology",
                Published = new DateTime(2019, 3, 01),
                Updated = new DateTime(2019, 3, 15),
                Annexes = new List<ContentSection>(),
                Content = new List<ContentSection>()
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                Topic = topicA,
                Slug = "publication-a",
                Summary = "first publication",
                Methodology = methodologyA
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                Topic = topicA,
                Slug = "publication-b",
                Summary = "second publication",
                Methodology = methodologyB
            };

            var publicationARelease1 = new Release
            {
                Publication = publicationA,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ1,
                Published = new DateTime(2019, 1, 01),
                ApprovalStatus = Approved
            };

            var publicationBRelease1 = new Release
            {
                Publication = publicationB,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ1,
                Published = null,
                ApprovalStatus = Draft
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Topics.AddAsync(topicA);
                await contentDbContext.Methodologies.AddRangeAsync(methodologyA, methodologyB);
                await contentDbContext.Publications.AddRangeAsync(publicationA, publicationB);
                await contentDbContext.Releases.AddRangeAsync(publicationARelease1, publicationBRelease1);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = new MethodologyService(contentDbContext, MapperForProfile<MappingProfiles>());

                var result = service.GetTree(Enumerable.Empty<Guid>());

                Assert.Single(result);
                var theme = result.First();
                Assert.Equal("Theme A", theme.Title);

                Assert.Single(theme.Topics);
                var topic = theme.Topics.First();
                Assert.Equal("Topic A", topic.Title);

                Assert.Single(topic.Publications);
                var publication = topic.Publications.First();
                Assert.Equal("Publication A", publication.Title);

                var methodology = publication.Methodology;
                Assert.Equal("methodology-a", methodology.Slug);
                Assert.Equal("first methodology", methodology.Summary);
                Assert.Equal("Methodology A", methodology.Title);
            }
        }

        [Fact]
        public async Task GetFiles()
        {
            var methodology = new Methodology();

            var imageFile1 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    Filename = "image1.png",
                    Type = Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    Filename = "image2.png",
                    Type = Image
                }
            };

            var otherFile = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2, otherFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext);

                var result = await service.GetFiles(methodology.Id, Image);

                Assert.Equal(2, result.Count);
                Assert.Equal(imageFile1.File.Id, result[0].Id);
                Assert.Equal(imageFile2.File.Id, result[1].Id);
            }
        }

        [Fact]
        public async Task GetViewModel()
        {
            var methodology = new Methodology
            {
                Id = Guid.NewGuid(),
                Slug = "methodology-slug",
                Title = "Methodology",
                Summary = "Methodology summary",
                Published = new DateTime(2019, 1, 01),
                Updated = new DateTime(2019, 1, 15),
                Annexes = new List<ContentSection>(),
                Content = new List<ContentSection>()
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext);

                var result = await service.GetViewModelAsync(methodology.Id, PublishContext());

                Assert.Equal(methodology.Id, result.Id);
                Assert.Equal("Methodology", result.Title);
                Assert.Equal(new DateTime(2019, 1, 01), result.Published);
                Assert.Equal(new DateTime(2019, 1, 15), result.Updated);
                Assert.Equal("Methodology summary", result.Summary);

                var annexes = result.Annexes;
                Assert.NotNull(annexes);
                Assert.Empty(annexes);

                var content = result.Content;
                Assert.NotNull(content);
                Assert.Empty(content);
            }
        }

        [Fact]
        public async Task GetViewModel_NotYetPublished()
        {
            var methodology = new Methodology
            {
                Slug = "methodology-slug",
                Title = "Methodology",
                Summary = "Methodology summary",
                Published = null,
                Updated = new DateTime(2019, 6, 15),
                Annexes = new List<ContentSection>(),
                Content = new List<ContentSection>()
            };
            
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext);

                var context = PublishContext();
                var result = await service.GetViewModelAsync(methodology.Id, context);

                Assert.Equal(methodology.Id, result.Id);
                Assert.Equal("Methodology", result.Title);
                Assert.Equal(context.Published, result.Published);
                Assert.Equal(new DateTime(2019, 6, 15), result.Updated);
                Assert.Equal("Methodology summary", result.Summary);

                var annexes = result.Annexes;
                Assert.NotNull(annexes);
                Assert.Empty(annexes);

                var content = result.Content;
                Assert.NotNull(content);
                Assert.Empty(content);
            }
        }

        [Fact]
        public async Task GetByRelease()
        {
            var methodology = new Methodology();

            var release = new Release
            {
                Publication = new Publication
                {
                    Title = "Publication",
                    Slug = "publication-slug",
                    Methodology = methodology
                },
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ1,
                ApprovalStatus = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();
        
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext);

                var result = await service.GetByRelease(release.Id);
                Assert.Equal(methodology.Id, result.Id);
            }
        }

        [Fact]
        public async Task GetByRelease_PublicationHasNoMethodology()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Title = "Publication",
                    Slug = "publication-slug",
                    Methodology = null
                },
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ1,
                ApprovalStatus = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();
        
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext);

                var result = await service.GetByRelease(release.Id);
                Assert.Null(result);
            }
        }

        private static PublishContext PublishContext()
        {
            var published = DateTime.Today.Add(new TimeSpan(9, 30, 0));
            return new PublishContext(published, true);
        }

        private MethodologyService SetupMethodologyService(ContentDbContext contentDbContext)
        {
            return new MethodologyService(
                contentDbContext,
                MapperForProfile<MappingProfiles>());
        }
    }
}
