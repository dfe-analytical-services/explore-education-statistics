using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    public class FootnoteControllerTests
    {
        private readonly FootnoteController _controller;

        private const long FootnoteId = 1L;

        private readonly Guid _releaseId = Guid.NewGuid();

        public FootnoteControllerTests()
        {
            var footnoteService = new Mock<IFootnoteService>();
            var releaseMetaService = new Mock<IReleaseMetaService>();

            var footnote = new Footnote
            {
                Id = FootnoteId,
                Content = "Sample footnote",
                Filters = new List<FilterFootnote>(),
                FilterGroups = new List<FilterGroupFootnote>(),
                FilterItems = new List<FilterItemFootnote>(),
                Indicators = new List<IndicatorFootnote>(),
                Subjects = new List<SubjectFootnote>()
            };

            footnoteService.Setup(s => s.GetFootnote(FootnoteId)).Returns(footnote);

            footnoteService.Setup(s => s.CreateFootnote("Sample footnote",
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<IEnumerable<long>>())).Returns(footnote);

            footnoteService.Setup(s => s.UpdateFootnote(FootnoteId,
                "Updated sample footnote",
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<IEnumerable<long>>(),
                It.IsAny<IEnumerable<long>>())).Returns(new Footnote
            {
                Id = FootnoteId,
                Content = "Updated sample footnote",
                Filters = new List<FilterFootnote>(),
                FilterGroups = new List<FilterGroupFootnote>(),
                FilterItems = new List<FilterItemFootnote>(),
                Indicators = new List<IndicatorFootnote>(),
                Subjects = new List<SubjectFootnote>()
            });

            footnoteService.Setup(s => s.GetFootnotes(_releaseId)).Returns(new List<Footnote>
            {
                footnote
            });

            releaseMetaService.Setup(s => s.GetSubjects(_releaseId)).Returns(new List<IdLabel>
            {
                new IdLabel(1, "Subject 1"),
                new IdLabel(2, "Subject 2"),
                new IdLabel(3, "Subject 3")
            });

            _controller = new FootnoteController(footnoteService.Object, releaseMetaService.Object,
                MapperForProfile<MappingProfiles>());
        }

        [Fact]
        public void Post_CreateFootnote_Returns_Ok()
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

        [Fact]
        public void Get_Footnotes_Returns_Ok()
        {
            var result = _controller.GetFootnotes(_releaseId);
            Assert.IsAssignableFrom<FootnotesViewModel>(result.Value);
        }

        [Fact]
        public void Put_UpdateFootnote_Returns_Ok()
        {
            var result = _controller.UpdateFootnote(FootnoteId, new UpdateFootnoteViewModel
            {
                Content = "Updated sample footnote",
                Filters = new List<long>(),
                FilterGroups = new List<long>(),
                FilterItems = new List<long>(),
                Indicators = new List<long>(),
                Subjects = new List<long>()
            });

            Assert.IsAssignableFrom<FootnoteViewModel>(result.Value);
        }

        [Fact]
        public void Delete_CreateFootnote_Returns_Ok()
        {
            var result = _controller.DeleteFootnote(FootnoteId);
            Assert.IsAssignableFrom<NoContentResult>(result);
        }
    }
}