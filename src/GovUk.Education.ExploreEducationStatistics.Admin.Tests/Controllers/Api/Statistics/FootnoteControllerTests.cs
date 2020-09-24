using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    public class FootnoteControllerTests
    {
        private readonly FootnoteController _controller;

        private static readonly Guid FootnoteId = Guid.NewGuid();

        private static readonly Guid ReleaseId = Guid.NewGuid();

        public FootnoteControllerTests()
        {
            var subjectIds = new[] {Guid.NewGuid(), Guid.NewGuid()};

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

            var filterService = new Mock<IFilterService>();
            var indicatorGroupService = new Mock<IIndicatorGroupService>();
            var footnoteService = new Mock<IFootnoteService>();
            var releaseMetaService = new Mock<IReleaseMetaService>();

            var createFootnoteResult = Task.FromResult(new Either<ActionResult, Footnote>(footnote));

            footnoteService.Setup(s => s.CreateFootnote(
                ReleaseId,
                "Sample footnote",
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>())).Returns(createFootnoteResult);

            var updateFootnoteResult = Task.FromResult(new Either<ActionResult, Footnote>(new Footnote
            {
                Id = FootnoteId,
                Content = "Updated sample footnote",
                Filters = new List<FilterFootnote>(),
                FilterGroups = new List<FilterGroupFootnote>(),
                FilterItems = new List<FilterItemFootnote>(),
                Indicators = new List<IndicatorFootnote>(),
                Subjects = new List<SubjectFootnote>()
            }));

            footnoteService.Setup(s => s.UpdateFootnote(
                ReleaseId,
                FootnoteId,
                "Updated sample footnote",
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<IReadOnlyCollection<Guid>>())).Returns(updateFootnoteResult);

            var footnotes = Task.FromResult(new Either<ActionResult, IEnumerable<Footnote>>(new List<Footnote>
            {
                footnote
            }));

            footnoteService.Setup(s => s.GetFootnotesAsync(ReleaseId)).Returns(footnotes);

            var deleteFootnoteResult = Task.FromResult(new Either<ActionResult, bool>(true));
            footnoteService.Setup(s => s.DeleteFootnote(ReleaseId, FootnoteId)).ReturnsAsync(Unit.Instance);

            var subjects = subjectIds.Select(id => new IdLabel(id, $"Subject {id}")).ToList();
            releaseMetaService.Setup(s => s.GetSubjectsAsync(ReleaseId)).ReturnsAsync(subjects);

            filterService.Setup(s => s.GetFiltersIncludingItems(It.IsIn(subjectIds))).Returns(
                new List<Filter>
                {
                    new Filter
                    {
                        Id = Guid.NewGuid(),
                        Hint = "Filter Hint",
                        Label = "Filter label",
                        Name = "Filter name",
                        FilterGroups = new List<FilterGroup>
                        {
                            new FilterGroup
                            {
                                Id = Guid.NewGuid(),
                                Label = "Filter group",
                                FilterItems = new List<FilterItem>
                                {
                                    new FilterItem
                                    {
                                        Id = Guid.NewGuid(),
                                        Label = "Filter item",
                                    }
                                }
                            }
                        }
                    }
                }
            );

            indicatorGroupService.Setup(s => s.GetIndicatorGroups(It.IsIn(subjectIds))).Returns(
                new List<IndicatorGroup>
                {
                    new IndicatorGroup
                    {
                        Id = Guid.NewGuid(),
                        Label = "Indicator group",
                        Indicators = new List<Indicator>
                        {
                            new Indicator
                            {
                                Id = Guid.NewGuid(),
                                Label = "Indicator label",
                                Name = "Indicator name",
                                Unit = Data.Model.Unit.Percent,
                                DecimalPlaces = 2
                            }
                        }
                    }
                });

            _controller = new FootnoteController(filterService.Object,
                footnoteService.Object,
                indicatorGroupService.Object,
                releaseMetaService.Object);
        }

        [Fact]
        public async void Post_CreateFootnote_Returns_Ok()
        {
            var result = await _controller.CreateFootnote(ReleaseId, new CreateFootnoteViewModel()
            {
                Content = "Sample footnote",
                Filters = new List<Guid>(),
                FilterGroups = new List<Guid>(),
                FilterItems = new List<Guid>(),
                Indicators = new List<Guid>(),
                Subjects = new List<Guid>()
            });

            Assert.IsType<FootnoteViewModel>(result.Value);
        }

        [Fact]
        public async void Get_Footnotes_Returns_Ok()
        {
            var result = await _controller.GetFootnotes(ReleaseId);
            Assert.IsAssignableFrom<IEnumerable<FootnoteViewModel>>(result.Value);
        }

        [Fact]
        public async void Put_UpdateFootnote_Returns_Ok()
        {
            var result = await _controller.UpdateFootnote(ReleaseId, FootnoteId, new UpdateFootnoteViewModel
            {
                Content = "Updated sample footnote",
                Filters = new List<Guid>(),
                FilterGroups = new List<Guid>(),
                FilterItems = new List<Guid>(),
                Indicators = new List<Guid>(),
                Subjects = new List<Guid>()
            });

            Assert.IsType<FootnoteViewModel>(result.Value);
        }

        [Fact]
        public async void Delete_DeleteFootnote_Returns_Ok()
        {
            var result = await _controller.DeleteFootnote(ReleaseId, FootnoteId);
            Assert.IsType<NoContentResult>(result);
        }
    }
}