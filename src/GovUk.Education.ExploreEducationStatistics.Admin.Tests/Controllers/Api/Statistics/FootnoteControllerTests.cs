using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    public class FootnoteControllerTests
    {
        private readonly FootnoteController _controller;

        public FootnoteControllerTests()
        {
            var footnoteService = new Mock<IFootnoteService>();
            var releaseMetaService = new Mock<IReleaseMetaService>();

            footnoteService.Setup(s => s.CreateFootnote("Sample footnote",
                    It.IsAny<IEnumerable<long>>(),
                    It.IsAny<IEnumerable<long>>(),
                    It.IsAny<IEnumerable<long>>(),
                    It.IsAny<IEnumerable<long>>(),
                    It.IsAny<IEnumerable<long>>())).Returns(new Footnote
                {
                    Id = 1,
                    Content = "Sample footnote",
                    Filters = new List<FilterFootnote>(),
                    FilterGroups = new List<FilterGroupFootnote>(),
                    FilterItems = new List<FilterItemFootnote>(),
                    Indicators = new List<IndicatorFootnote>(),
                    Subjects = new List<SubjectFootnote>()
                });

            _controller = new FootnoteController(footnoteService.Object, releaseMetaService.Object, MapperForProfile<MappingProfiles>());
        }

        [Fact]
        public void CreateFootnote_Returns_Ok()
        {
            var result = _controller.CreateFootnote(new CreateFootnoteViewModel
            {
                Content = "Sample footnote",
                Filters = new List<long>(),
                FilterGroups = new List<long>(),
                FilterItems = new List<long>(),
                Indicators = new List<long>(),
                Subjects = new List<long>()
            });

            Assert.IsAssignableFrom<FootnoteViewModel>(result.Value);
        }
    }
}