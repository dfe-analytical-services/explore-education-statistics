using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics
{
    [Route("api/data/[controller]")]
    [ApiController]
    [Authorize]
    public class FootnoteController : ControllerBase
    {
        private readonly IFilterService _filterService;
        private readonly IFootnoteService _footnoteService;
        private readonly IIndicatorGroupService _indicatorGroupService;
        private readonly IReleaseMetaService _releaseMetaService;
        private readonly IMapper _mapper;

        public FootnoteController(IFilterService filterService,
            IFootnoteService footnoteService,
            IIndicatorGroupService indicatorGroupService,
            IReleaseMetaService releaseMetaService,
            IMapper mapper)
        {
            _filterService = filterService;
            _footnoteService = footnoteService;
            _indicatorGroupService = indicatorGroupService;
            _releaseMetaService = releaseMetaService;
            _mapper = mapper;
        }

        [HttpPost]
        public ActionResult<FootnoteViewModel> CreateFootnote(CreateFootnoteViewModel footnote)
        {
            var created = _footnoteService.CreateFootnote(footnote.Content,
                footnote.Filters,
                footnote.FilterGroups,
                footnote.FilterItems,
                footnote.Indicators,
                footnote.Subjects);

            return BuildFootnoteViewModel(created);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteFootnote(long id)
        {
            return CheckFootnoteExists(id, () =>
            {
                _footnoteService.DeleteFootnote(id);
                return new NoContentResult();
            });
        }

        [HttpGet("release/{releaseId}")]
        public ActionResult<FootnotesViewModel> GetFootnotes(Guid releaseId)
        {
            var footnotes = _footnoteService.GetFootnotes(releaseId).Select(BuildFootnoteViewModel);
            var subjects = _releaseMetaService.GetSubjects(releaseId).ToDictionary(subject => subject.Id, subject =>
                new FootnotesSubjectMetaViewModel
                {
                    Filters = GetFilters(subject.Id),
                    Indicators = GetIndicators(subject.Id),
                    SubjectId = subject.Id,
                    SubjectName = subject.Label
                });

            return new FootnotesViewModel
            {
                Footnotes = footnotes,
                Meta = subjects
            };
        }

        [HttpPut("{id}")]
        public ActionResult<FootnoteViewModel> UpdateFootnote(long id, UpdateFootnoteViewModel footnote)
        {
            var updated = _footnoteService.UpdateFootnote(id,
                footnote.Content,
                footnote.Filters,
                footnote.FilterGroups,
                footnote.FilterItems,
                footnote.Indicators,
                footnote.Subjects);

            return BuildFootnoteViewModel(updated);
        }

        private ActionResult CheckFootnoteExists(long id, Func<ActionResult> andThen)
        {
            var footnote = _footnoteService.GetFootnote(id);
            return footnote == null ? NotFound() : andThen.Invoke();
        }

        private Dictionary<long, FootnotesIndicatorsMetaViewModel> GetIndicators(long subjectId)
        {
            return _indicatorGroupService.GetIndicatorGroups(subjectId).ToDictionary(
                group => group.Id,
                group => new FootnotesIndicatorsMetaViewModel
                {
                    Label = group.Label,
                    Options = group.Indicators.ToDictionary(
                        indicator => indicator.Id,
                        indicator => _mapper.Map<IndicatorMetaViewModel>(indicator))
                }
            );
        }

        private Dictionary<long, FootnotesFilterMetaViewModel> GetFilters(long subjectId)
        {
            return _filterService.GetFiltersIncludingItems(subjectId)
                .ToDictionary(
                    filter => filter.Id,
                    filter => new FootnotesFilterMetaViewModel
                    {
                        Hint = filter.Hint,
                        Legend = filter.Label,
                        Options = filter.FilterGroups.ToDictionary(
                            filterGroup => filterGroup.Id,
                            filterGroup => BuildFilterItemsMetaViewModel(filterGroup, filterGroup.FilterItems))
                    });
        }

        private static FootnotesFilterItemsMetaViewModel BuildFilterItemsMetaViewModel(FilterGroup filterGroup,
            IEnumerable<FilterItem> filterItems)
        {
            return new FootnotesFilterItemsMetaViewModel
            {
                Label = filterGroup.Label,
                Options = filterItems.ToDictionary(
                    item => item.Id,
                    item => new LabelValue
                    {
                        Label = item.Label,
                        Value = item.Id.ToString()
                    })
            };
        }

        private static IEnumerable<FootnoteViewModel> BuildFootnoteViewModel(IEnumerable<Footnote> footnotes)
        {
            return footnotes.Select(footnote =>
            {
                return new FootnoteViewModel
                {
                    Id = footnote.Id,
                    Content = footnote.Content,
                    Subjects = new Dictionary<long, FootnoteSubjectViewModel>
                    {
                        {
                            1, new FootnoteSubjectViewModel
                            {
                                Filters = new Dictionary<long, FootnoteFilterViewModel>
                                {
                                    {
                                        1, new FootnoteFilterViewModel
                                        {
                                            FilterGroups = new Dictionary<long, FootnoteFilterGroupViewModel>
                                            {
                                                {
                                                    1, new FootnoteFilterGroupViewModel
                                                    {
                                                        FilterItems = new List<long>
                                                        {
                                                            56, 57, 58
                                                        },
                                                        Selected = false
                                                    }
                                                }
                                            },
                                            Selected = false
                                        }
                                    }
                                },
                                IndicatorGroups = new Dictionary<long, FootnoteIndicatorGroupViewModel>
                                {
                                    {
                                        1, new FootnoteIndicatorGroupViewModel
                                        {
                                            Indicators = new List<long>
                                            {
                                                16, 3
                                            },
                                            Selected = false
                                        }
                                    }
                                },
                                Selected = false
                            }
                        }
                    }
                };
            });
        }

        private static FootnoteViewModel BuildFootnoteViewModel(Footnote footnote)
        {
            return new FootnoteViewModel
            {
                Id = footnote.Id,
                Content = footnote.Content,
                Subjects = new Dictionary<long, FootnoteSubjectViewModel>
                {
                    {
                        1, new FootnoteSubjectViewModel
                        {
                            Filters = new Dictionary<long, FootnoteFilterViewModel>
                            {
                                {
                                    1, new FootnoteFilterViewModel
                                    {
                                        FilterGroups = new Dictionary<long, FootnoteFilterGroupViewModel>
                                        {
                                            {
                                                1, new FootnoteFilterGroupViewModel
                                                {
                                                    FilterItems = new List<long>
                                                    {
                                                        56, 57, 58
                                                    },
                                                    Selected = false
                                                }
                                            }
                                        },
                                        Selected = false
                                    }
                                }
                            },
                            IndicatorGroups = new Dictionary<long, FootnoteIndicatorGroupViewModel>
                            {
                                {
                                    1, new FootnoteIndicatorGroupViewModel
                                    {
                                        Indicators = new List<long>
                                        {
                                            16, 3
                                        },
                                        Selected = false
                                    }
                                }
                            },
                            Selected = false
                        }
                    }
                }
            };
        }
    }
}