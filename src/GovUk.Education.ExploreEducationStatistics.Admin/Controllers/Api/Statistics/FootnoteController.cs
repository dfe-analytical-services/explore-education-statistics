using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces.IReleaseService;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;

[Route("api")]
[ApiController]
[Authorize]
public class FootnoteController : ControllerBase
{
    private readonly IFootnoteService _footnoteService;
    private readonly IStorageDataSetResolver _storageDataSetResolver;
    private readonly IReleaseService _releaseService;
    private readonly IReleaseDataFileRepository _releaseDataFileRepository;
    private static IComparer<string> LabelComparer { get; } = new LabelRelationalComparer();

    public FootnoteController(
        IFootnoteService footnoteService,
        IStorageDataSetResolver storageDataSetResolver,
        IReleaseService releaseService,
        IReleaseDataFileRepository releaseDataFileRepository
    )
    {
        _footnoteService = footnoteService;
        _storageDataSetResolver = storageDataSetResolver;
        _releaseService = releaseService;
        _releaseDataFileRepository = releaseDataFileRepository;
    }

    [HttpPost("releases/{releaseVersionId:guid}/footnotes")]
    public async Task<ActionResult<FootnoteViewModel>> CreateFootnote(
        Guid releaseVersionId,
        FootnoteCreateRequest footnote
    )
    {
        return await _footnoteService
            .CreateFootnote(
                releaseVersionId,
                footnote.Content,
                footnote.Filters,
                footnote.FilterGroups,
                footnote.FilterItems,
                footnote.Indicators,
                footnote.Subjects
            )
            .OnSuccess(BuildFootnoteViewModel)
            .HandleFailuresOrOk();
    }

    [HttpDelete("releases/{releaseVersionId:guid}/footnotes/{footnoteId:guid}")]
    public async Task<ActionResult> DeleteFootnote(Guid releaseVersionId, Guid footnoteId)
    {
        return await _footnoteService
            .DeleteFootnote(releaseVersionId: releaseVersionId, footnoteId: footnoteId)
            .HandleFailuresOrNoContent();
    }

    [HttpGet("releases/{releaseVersionId:guid}/footnotes/{footnoteId:guid}")]
    public async Task<ActionResult<FootnoteViewModel>> GetFootnote(Guid releaseVersionId, Guid footnoteId)
    {
        return await _footnoteService
            .GetFootnote(releaseVersionId: releaseVersionId, footnoteId: footnoteId)
            .OnSuccess(BuildFootnoteViewModel)
            .HandleFailuresOrOk();
    }

    [HttpGet("releases/{releaseVersionId:guid}/footnotes")]
    public async Task<ActionResult<IEnumerable<FootnoteViewModel>>> GetFootnotes(Guid releaseVersionId)
    {
        return await _footnoteService
            .GetFootnotes(releaseVersionId)
            .OnSuccess(footnotes => footnotes.Select(BuildFootnoteViewModel))
            .HandleFailuresOrOk();
    }

    [HttpPut("releases/{releaseVersionId:guid}/footnotes/{footnoteId:guid}")]
    public async Task<ActionResult<FootnoteViewModel>> UpdateFootnote(
        Guid releaseVersionId,
        Guid footnoteId,
        FootnoteUpdateRequest footnote
    )
    {
        return await _footnoteService
            .UpdateFootnote(
                releaseVersionId: releaseVersionId,
                footnoteId: footnoteId,
                footnote.Content,
                filterIds: footnote.Filters,
                filterGroupIds: footnote.FilterGroups,
                filterItemIds: footnote.FilterItems,
                indicatorIds: footnote.Indicators,
                subjectIds: footnote.Subjects
            )
            .OnSuccess(BuildFootnoteViewModel)
            .HandleFailuresOrOk();
    }

    [HttpGet("releases/{releaseVersionId:guid}/footnotes-meta")]
    public async Task<ActionResult<FootnotesMetaViewModel>> GetFootnotesMeta(Guid releaseVersionId)
    {
        return await _releaseService
            .ListSubjects(releaseVersionId)
            .OnSuccess(async subjects =>
            {
                var subjectMetaViewModels = await subjects
                    .ToAsyncEnumerable()
                    .SelectAwait(async subject =>
                    {
                        var releaseFile = await _releaseDataFileRepository.GetBySubject(releaseVersionId, subject.Id);
                        var dataSet = _storageDataSetResolver.Resolve(releaseFile.File);

                        return new FootnotesSubjectMetaViewModel
                        {
                            Filters = await GetFilters(dataSet),
                            Indicators = await GetIndicators(dataSet),
                            SubjectId = subject.Id,
                            SubjectName = releaseFile.Name,
                        };
                    })
                    .ToListAsync();

                return new FootnotesMetaViewModel
                {
                    Subjects = subjectMetaViewModels.ToDictionary(result => result.SubjectId, result => result),
                };
            })
            .HandleFailuresOrOk();
    }

    [HttpPatch("releases/{releaseVersionId:guid}/footnotes")]
    public async Task<ActionResult<Unit>> UpdateFootnotes(Guid releaseVersionId, FootnotesUpdateRequest request)
    {
        return await _footnoteService.UpdateFootnotes(releaseVersionId, request).HandleFailuresOrOk();
    }

    private static async Task<Dictionary<Guid, FootnotesIndicatorsMetaViewModel>> GetIndicators(IStorageDataSet dataSet)
    {
        return (await dataSet.ListIndicatorGroups())
            .OrderBy(group => group.Label, LabelComparer)
            .ToDictionary(
                group => group.Id,
                group => new FootnotesIndicatorsMetaViewModel
                {
                    Label = group.Label,
                    Options = group
                        .Indicators.OrderBy(indicator => indicator.Label, LabelComparer)
                        .Select(indicator => new IndicatorMetaViewModel(indicator))
                        .ToList(),
                }
            );
    }

    private static async Task<Dictionary<Guid, FootnotesFilterMetaViewModel>> GetFilters(IStorageDataSet dataSet)
    {
        return (await dataSet.ListFilters()).ToDictionary(
            filter => filter.Id,
            filter => new FootnotesFilterMetaViewModel
            {
                Hint = filter.Hint,
                Legend = filter.Label,
                Options = filter
                    .FilterGroups.OrderBy(items => items.Label, LabelComparer)
                    .ToDictionary(
                        filterGroup => filterGroup.Id,
                        filterGroup => BuildFilterItemsMetaViewModel(filterGroup, filterGroup.FilterItems)
                    ),
            }
        );
    }

    private static FootnotesFilterGroupsMetaViewModel BuildFilterItemsMetaViewModel(
        FilterGroup filterGroup,
        IEnumerable<FilterItem> filterItems
    )
    {
        return new FootnotesFilterGroupsMetaViewModel
        {
            Label = filterGroup.Label,
            Options = filterItems
                .OrderBy(item => item.Label, LabelComparer)
                .Select(item => new LabelValue(item.Label, item.Id.ToString()))
                .ToList(),
        };
    }

    private static FootnoteViewModel BuildFootnoteViewModel(Footnote footnote)
    {
        var subjects = new Dictionary<Guid, FootnoteSubjectViewModel>();

        foreach (var subjectFootnote in footnote.Subjects)
        {
            GetOrAddSubject(subjects, subjectFootnote.SubjectId).Selected = true;
        }

        foreach (var filterFootnote in footnote.Filters)
        {
            GetOrAddFilter(subjects, filterFootnote.SubjectId, filterFootnote.FilterId).Selected = true;
        }

        foreach (var filterGroupFootnote in footnote.FilterGroups)
        {
            GetOrAddFilterGroup(
                subjects,
                filterGroupFootnote.SubjectId,
                filterGroupFootnote.FilterId,
                filterGroupFootnote.FilterGroupId
            ).Selected = true;
        }

        foreach (var filterItemFootnote in footnote.FilterItems)
        {
            GetOrAddFilterGroup(
                subjects,
                filterItemFootnote.SubjectId,
                filterItemFootnote.FilterId,
                filterItemFootnote.FilterGroupId
            )
                .FilterItems.Add(filterItemFootnote.FilterItemId.ToString());
        }

        foreach (var indicatorFootnote in footnote.Indicators)
        {
            GetOrAddSubject(subjects, indicatorFootnote.SubjectId)
                .IndicatorGroups.GetOrSet(
                    indicatorFootnote.IndicatorGroupId,
                    () => new FootnoteIndicatorGroupViewModel { Indicators = [] }
                )
                .Indicators.Add(indicatorFootnote.IndicatorId.ToString());
        }

        return new FootnoteViewModel
        {
            Id = footnote.Id,
            Content = footnote.Content,
            Subjects = subjects,
        };

        static FootnoteSubjectViewModel GetOrAddSubject(
            Dictionary<Guid, FootnoteSubjectViewModel> subjects,
            Guid subjectId
        ) => subjects.GetOrSet(subjectId, () => new FootnoteSubjectViewModel { Filters = [], IndicatorGroups = [] });

        static FootnoteFilterViewModel GetOrAddFilter(
            Dictionary<Guid, FootnoteSubjectViewModel> subjects,
            Guid subjectId,
            Guid filterId
        ) =>
            GetOrAddSubject(subjects, subjectId)
                .Filters.GetOrSet(filterId, () => new FootnoteFilterViewModel { FilterGroups = [] });

        static FootnoteFilterGroupViewModel GetOrAddFilterGroup(
            Dictionary<Guid, FootnoteSubjectViewModel> subjects,
            Guid subjectId,
            Guid filterId,
            Guid filterGroupId
        ) =>
            GetOrAddFilter(subjects, subjectId, filterId)
                .FilterGroups.GetOrSet(filterGroupId, () => new FootnoteFilterGroupViewModel { FilterItems = [] });
    }
}
