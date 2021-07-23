using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class FootnoteService : IFootnoteService
    {
        private readonly StatisticsDbContext _context;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly IUserService _userService;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IGuidGenerator _guidGenerator;

        public FootnoteService(
            StatisticsDbContext context,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IUserService userService,
            IFootnoteRepository footnoteRepository,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper, 
            IGuidGenerator guidGenerator)
        {
            _context = context;
            _contentPersistenceHelper = contentPersistenceHelper;
            _userService = userService;
            _footnoteRepository = footnoteRepository;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
            _guidGenerator = guidGenerator;
        }

        public async Task<Either<ActionResult, Footnote>> CreateFootnote(
            Guid releaseId,
            string content,
            IReadOnlyCollection<Guid> filterIds,
            IReadOnlyCollection<Guid> filterGroupIds,
            IReadOnlyCollection<Guid> filterItemIds,
            IReadOnlyCollection<Guid> indicatorIds,
            IReadOnlyCollection<Guid> subjectIds)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async () =>
                {
                    var newFootnoteId = _guidGenerator.NewGuid();

                    var footnote = new Footnote
                    {
                        Id = newFootnoteId,
                        Content = content,
                        Subjects = CreateSubjectLinks(newFootnoteId, subjectIds),
                        Filters = CreateFilterLinks(newFootnoteId, filterIds),
                        FilterGroups = CreateFilterGroupLinks(newFootnoteId, filterGroupIds),
                        FilterItems = CreateFilterItemLinks(newFootnoteId, filterItemIds),
                        Indicators = CreateIndicatorsLinks(newFootnoteId, indicatorIds)
                    };

                    await _context.Footnote.AddAsync(footnote);

                    await _context.ReleaseFootnote.AddAsync(new ReleaseFootnote
                        {
                            ReleaseId = releaseId,
                            Footnote = footnote
                        });

                    await _context.SaveChangesAsync();
                    return await GetFootnote(releaseId, footnote.Id);
                });
        }

        public async Task<Either<ActionResult, List<Footnote>>> CopyFootnotes(Guid sourceReleaseId, Guid destinationReleaseId)
        {
            return await GetFootnotes(sourceReleaseId)
                .OnSuccess(async footnotes => 
                    await footnotes.ForEachAsync(async footnote =>
                    {
                        var filters = footnote.Filters
                            .Select(filterFootnote => filterFootnote.FilterId).ToList();
                        var filterGroups = footnote.FilterGroups
                            .Select(filterGroupFootnote => filterGroupFootnote.FilterGroupId).ToList();
                        var filterItems = footnote.FilterItems
                            .Select(filterItemFootnote => filterItemFootnote.FilterItemId).ToList();
                        var indicators = footnote.Indicators
                            .Select(indicatorFootnote => indicatorFootnote.IndicatorId).ToList();
                        var subjects = footnote.Subjects
                            .Select(subjectFootnote => subjectFootnote.SubjectId).ToList();

                        return await CreateFootnote(destinationReleaseId,
                            footnote.Content,
                            filters,
                            filterGroups,
                            filterItems,
                            indicators,
                            subjects);
                    }));
        }

        public async Task<Either<ActionResult, Unit>> DeleteFootnote(Guid releaseId, Guid id)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(_ => _statisticsPersistenceHelper.CheckEntityExists<Footnote>(id)
                .OnSuccess(async footnote =>
                {
                    await _footnoteRepository.DeleteFootnote(releaseId, footnote.Id);
                    await _context.SaveChangesAsync();
                    return Unit.Instance;
                }));
        }

        public async Task<Either<ActionResult, Footnote>> UpdateFootnote(
            Guid releaseId,
            Guid id,
            string content,
            IReadOnlyCollection<Guid> filterIds,
            IReadOnlyCollection<Guid> filterGroupIds,
            IReadOnlyCollection<Guid> filterItemIds,
            IReadOnlyCollection<Guid> indicatorIds,
            IReadOnlyCollection<Guid> subjectIds)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(_ => _statisticsPersistenceHelper.CheckEntityExists<Footnote>(id, HydrateFootnote)
                .OnSuccess(async footnote =>
                {
                    if (await _footnoteRepository.IsFootnoteExclusiveToReleaseAsync(releaseId, footnote.Id))
                    {
                        _context.Update(footnote);

                        footnote.Content = content;

                        UpdateFilterLinks(footnote, filterIds.ToList());
                        UpdateFilterGroupLinks(footnote, filterGroupIds.ToList());
                        UpdateFilterItemLinks(footnote, filterItemIds.ToList());
                        UpdateIndicatorLinks(footnote, indicatorIds.ToList());
                        UpdateSubjectLinks(footnote, subjectIds.ToList());

                        await _context.SaveChangesAsync();
                        return await _footnoteRepository.GetFootnote(id);
                    }

                    // If this amendment of the footnote affects other release then break the link with the old
                    // and create a new one
                    await _footnoteRepository.DeleteReleaseFootnoteLinkAsync(releaseId, footnote.Id);

                    return await CreateFootnote(
                        releaseId,
                        content,
                        filterIds,
                        filterGroupIds,
                        filterItemIds,
                        indicatorIds,
                        subjectIds);
                }));
        }

        public Task<Either<ActionResult, Footnote>> GetFootnote(Guid releaseId, Guid id)
        {
            return _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess<ActionResult, Release, Footnote>(async release =>
                    {
                        var footnote = await _footnoteRepository.GetFootnote(id);

                        if (footnote == null
                            || footnote.Releases.All(rf => rf.ReleaseId != release.Id))
                        {
                            return new NotFoundResult();
                        }

                        return footnote;
                    }
                );
        }

        public async Task<Either<ActionResult, IEnumerable<Footnote>>> GetFootnotes(Guid releaseId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(_ => _footnoteRepository.GetFootnotes(releaseId));
        }

        public IEnumerable<Footnote> GetFootnotes(Guid releaseId, Guid subjectId)
        {
            return _footnoteRepository.GetFootnotes(releaseId, subjectId);
        }

        private List<SubjectFootnote> CreateSubjectLinks(Guid footnoteId, IReadOnlyCollection<Guid> subjectIds)
        {
            return subjectIds
                .Select(id => 
                    new SubjectFootnote
                    {
                        FootnoteId = footnoteId,
                        SubjectId = id
                    })
                .ToList();
        }

        private List<FilterFootnote> CreateFilterLinks(Guid footnoteId, IReadOnlyCollection<Guid> filterIds)
        {
            return filterIds.Select(id => 
                new FilterFootnote
                {
                    FootnoteId = footnoteId,
                    FilterId = id
                })
                .ToList();
        }

        private List<FilterGroupFootnote> CreateFilterGroupLinks(Guid footnoteId, IReadOnlyCollection<Guid> filterGroupIds)
        {
            return filterGroupIds.Select(id => 
                new FilterGroupFootnote
                {
                    FootnoteId = footnoteId,
                    FilterGroupId = id
                })
                .ToList();
        }

        private List<FilterItemFootnote> CreateFilterItemLinks(Guid footnoteId, IReadOnlyCollection<Guid> filterItemIds)
        {
            return filterItemIds.Select(id => 
                new FilterItemFootnote
                {
                    FootnoteId = footnoteId,
                    FilterItemId = id
                })
                .ToList();
        }

        private List<IndicatorFootnote> CreateIndicatorsLinks(Guid footnoteId, IReadOnlyCollection<Guid> indicatorIds)
        {
            return indicatorIds.Select(id => 
                new IndicatorFootnote
                {
                    FootnoteId = footnoteId,
                    IndicatorId = id
                })
                .ToList();
        }

        private void UpdateFilterLinks(Footnote footnote, IReadOnlyCollection<Guid> filterIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.Filters.Select(link => link.FilterId), filterIds))
            {
                footnote.Filters = CreateFilterLinks(footnote.Id, filterIds);
            }
        }

        private void UpdateFilterGroupLinks(Footnote footnote, IReadOnlyCollection<Guid> filterGroupIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.FilterGroups.Select(link => link.FilterGroupId), filterGroupIds))
            {
                footnote.FilterGroups = CreateFilterGroupLinks(footnote.Id, filterGroupIds);
            }
        }

        private void UpdateFilterItemLinks(Footnote footnote, IReadOnlyCollection<Guid> filterItemIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.FilterItems.Select(link => link.FilterItemId), filterItemIds))
            {
                footnote.FilterItems = CreateFilterItemLinks(footnote.Id, filterItemIds);
            }
        }

        private void UpdateIndicatorLinks(Footnote footnote, IReadOnlyCollection<Guid> indicatorIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.Indicators.Select(link => link.IndicatorId), indicatorIds))
            {
                footnote.Indicators = CreateIndicatorsLinks(footnote.Id, indicatorIds);
            }
        }

        private void UpdateSubjectLinks(Footnote footnote, IReadOnlyCollection<Guid> subjectIds)
        {
            if (!SequencesAreEqualIgnoringOrder(
                footnote.Subjects.Select(link => link.SubjectId), subjectIds))
            {
                footnote.Subjects = CreateSubjectLinks(footnote.Id, subjectIds);
            }
        }

        private static bool SequencesAreEqualIgnoringOrder(IEnumerable<Guid> left, IEnumerable<Guid> right)
        {
            return left.OrderBy(id => id).SequenceEqual(right.OrderBy(id => id));
        }

        private static IQueryable<Footnote> HydrateFootnote(IQueryable<Footnote> query)
        {
            return query
                .Include(f => f.Filters)
                .Include(f => f.FilterGroups)
                .Include(f => f.FilterItems)
                .Include(f => f.Indicators)
                .Include(f => f.Subjects);
        }
    }
}