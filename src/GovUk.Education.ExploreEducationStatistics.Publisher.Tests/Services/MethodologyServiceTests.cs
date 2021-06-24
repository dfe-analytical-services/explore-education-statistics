using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class MethodologyServiceTests
    {
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
        public async Task GetByRelease()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Title = "Publication",
                    Slug = "publication-slug"
                },
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ1,
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
                //TODO SOW4 EES-2385 Get the latest methodologies related to this release
                Assert.Empty(result);
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
                },
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ1,
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
                Assert.Empty(result);
            }
        }

        private static MethodologyService SetupMethodologyService(ContentDbContext contentDbContext)
        {
            return new MethodologyService(contentDbContext);
        }
    }
}
