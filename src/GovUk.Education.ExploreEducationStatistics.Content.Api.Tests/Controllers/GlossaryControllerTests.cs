#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    [Collection(BlobCacheServiceTests)]
    public class GlossaryControllerTests : BlobCacheServiceTestFixture {

        [Fact]
        public async Task GetAllGlossaryEntries()
        {
            var glossaryEntries = new List<GlossaryCategoryViewModel>
            {
                new()
                {
                    Heading = "Glossary Category 1",
                    Entries = new List<GlossaryEntryViewModel>
                    {
                        new()
                        {
                            Body = "A body",
                            Slug = "A slug",
                            Title = "A title"
                        }
                    }
                }
            };

            var (controller, mocks) = BuildControllerAndDependencies();
            
            mocks.cacheService
                .Setup(s => s.GetItem(
                  It.IsAny<GlossaryCacheKey>(), typeof(List<GlossaryCategoryViewModel>)))
                .ReturnsAsync(null);

            mocks.glossaryService
                .Setup(s => s.GetAllGlossaryEntries())
                .ReturnsAsync(glossaryEntries);
            
            mocks.cacheService
                .Setup(s => s.SetItem<object>(
                    It.IsAny<GlossaryCacheKey>(), 
                    glossaryEntries))
                .Returns(Task.CompletedTask);

            var result = await controller.GetAllGlossaryEntries();
            VerifyAllMocks(mocks);
            
            Assert.Equal(glossaryEntries, result);
        }
        
        [Fact]
        public async Task GetGlossaryEntry()
        {
            var glossaryEntry = new GlossaryEntryViewModel
            {
                Body = "A body",
                Slug = "A slug",
                Title = "A title"
            };

            var (controller, mocks) = 
                BuildControllerAndDependencies();
            
            mocks.glossaryService
                .Setup(s => s.GetGlossaryEntry(glossaryEntry.Slug))
                .ReturnsAsync(glossaryEntry);
            
            var result = await controller.GetGlossaryEntry(glossaryEntry.Slug);
            VerifyAllMocks(mocks);
            
            result.AssertOkResult(glossaryEntry);
        }

        [Fact]
        public void GlossaryCategoryViewModel_SerialiseAndDeserialise()
        {
            var original = new GlossaryCategoryViewModel
            {
                Heading = "Glossary Category 1",
                Entries = new List<GlossaryEntryViewModel>
                {
                    new()
                    {
                        Body = "A body",
                        Slug = "A slug",
                        Title = "A title"
                    }
                }
            };

            var converted = DeserializeObject<GlossaryCategoryViewModel>(SerializeObject(original));
            converted.AssertDeepEqualTo(original);
        }

        private static (
            GlossaryController controller, 
            (
                Mock<IGlossaryService> glossaryService,
                Mock<IBlobCacheService> cacheService) mocks) 
            BuildControllerAndDependencies()
        {
            var glossaryService = new Mock<IGlossaryService>(Strict);
            var controller = new GlossaryController(glossaryService.Object);
            return (controller, (glossaryService, CacheService));
        }
    }
}
