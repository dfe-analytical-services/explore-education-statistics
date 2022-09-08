#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class GlossaryControllerTests
    {
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

            mocks.glossaryCacheService
                .Setup(s => s.GetGlossary())
                .ReturnsAsync(glossaryEntries);

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
        public void GlossaryCategoryViewModel_SerialiseAndDeserialize()
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
            Mock<IGlossaryCacheService> glossaryCacheService,
            Mock<IGlossaryService> glossaryService) mocks
            )
            BuildControllerAndDependencies()
        {
            var glossaryCacheService = new Mock<IGlossaryCacheService>(Strict);
            var glossaryService = new Mock<IGlossaryService>(Strict);

            var controller = new GlossaryController(
                glossaryCacheService.Object,
                glossaryService.Object);
            return (controller, (glossaryCacheService, glossaryService));
        }
    }
}
