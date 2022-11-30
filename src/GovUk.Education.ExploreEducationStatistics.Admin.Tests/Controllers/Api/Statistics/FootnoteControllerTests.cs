#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FootnoteViewModel = GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics.FootnoteViewModel;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces.IReleaseService;
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

            var filterRepository = new Mock<IFilterRepository>();
            var indicatorGroupRepository = new Mock<IIndicatorGroupRepository>();
            var footnoteService = new Mock<IFootnoteService>();
            var releaseService = new Mock<IReleaseService>();
            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>();

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

            footnoteService.Setup(s => s.GetFootnotes(ReleaseId))
                .ReturnsAsync(new List<Footnote>
                {
                    footnote
                });

            footnoteService.Setup(s => s.DeleteFootnote(ReleaseId, FootnoteId)).ReturnsAsync(Unit.Instance);

            releaseService.Setup(s => s.ListSubjects(ReleaseId))
                .ReturnsAsync(
                    subjectIds
                        .Select(
                            id => new SubjectViewModel(
                                id: id,
                                name: $"Subject {id}",
                                order: 0,
                                content: "Test content",
                                timePeriods: new TimePeriodLabels(),
                                geographicLevels: new List<string>(),
                                file: new FileInfo
                                {
                                    Id = Guid.NewGuid(),
                                    FileName = "test.csv",
                                    Size = "1 Mb"
                                }
                            )
                        )
                        .ToList()
                );

            filterRepository.Setup(s => s.GetFiltersIncludingItems(It.IsIn(subjectIds)))
                .ReturnsAsync(
                    new List<Filter>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Hint = "Filter Hint",
                            Label = "Filter label",
                            Name = "Filter name",
                            FilterGroups = new List<FilterGroup>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Label = "Filter group",
                                    FilterItems = new List<FilterItem>
                                    {
                                        new()
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

            indicatorGroupRepository.Setup(s => s.GetIndicatorGroups(It.IsIn(subjectIds)))
                .ReturnsAsync(
                    new List<IndicatorGroup>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Indicator group",
                            Indicators = new List<Indicator>
                            {
                                new()
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

            _controller = new FootnoteController(filterRepository.Object,
                footnoteService.Object,
                indicatorGroupRepository.Object,
                releaseService.Object,
                releaseDataFileRepository.Object);
        }

        [Fact]
        public async Task Post_CreateFootnote_Returns_Ok()
        {
            var result = await _controller.CreateFootnote(ReleaseId, new FootnoteCreateViewModel()
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
        public async Task Get_Footnotes_Returns_Ok()
        {
            var result = await _controller.GetFootnotes(ReleaseId);
            Assert.IsAssignableFrom<IEnumerable<FootnoteViewModel>>(result.Value);
        }

        [Fact]
        public async Task Put_UpdateFootnote_Returns_Ok()
        {
            var result = await _controller.UpdateFootnote(ReleaseId, FootnoteId, new FootnoteUpdateViewModel
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
        public async Task Delete_DeleteFootnote_Returns_Ok()
        {
            var result = await _controller.DeleteFootnote(ReleaseId, FootnoteId);
            Assert.IsType<NoContentResult>(result);
        }
    }
}
