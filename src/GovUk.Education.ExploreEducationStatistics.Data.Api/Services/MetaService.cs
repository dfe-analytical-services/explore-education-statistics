using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class MetaService : IMetaService
    {
        private readonly IFilterItemService _filterItemService;
        private readonly IIndicatorGroupService _indicatorGroupService;
        private readonly IObservationService _observationService;
        private readonly IReleaseService _releaseService;
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public MetaService(
            IFilterItemService filterItemService,
            IIndicatorGroupService indicatorGroupService,
            IObservationService observationService,
            IReleaseService releaseService,
            ISubjectService subjectService,
            IMapper mapper)
        {
            _filterItemService = filterItemService;
            _indicatorGroupService = indicatorGroupService;
            _observationService = observationService;
            _releaseService = releaseService;
            _subjectService = subjectService;
            _mapper = mapper;
        }

        public PublicationMetaViewModel GetPublicationMeta(Guid publicationId)
        {
            var releaseId = _releaseService.GetLatestRelease(publicationId);

            var subjectMetaViewModels = _mapper.Map<IEnumerable<IdLabelViewModel>>(
                _subjectService.FindMany(subject => subject.Release.Id == releaseId));

            return new PublicationMetaViewModel
            {
                PublicationId = publicationId,
                Subjects = subjectMetaViewModels
            };
        }

        public SubjectMetaViewModel GetSubjectMeta(long subjectId, SubjectMetaQueryContext query = null)
        {
            var subject = _subjectService.Find(subjectId);

            if (subject == null)
            {
                // TODO throw exception
                return null;
            }

            return new SubjectMetaViewModel
            {
                Filters = GetFilters(subject.Id, query),
                Indicators = GetIndicators(subject.Id),
                Locations = GetObservationalUnits(subject.Id, query),
                TimePeriod = GetTimePeriods(subject.Id, query)
            };
        }

        private LegendOptionsMetaValueModel<IEnumerable<TimePeriodMetaViewModel>> GetTimePeriods(
            long subjectId,
            SubjectMetaQueryContext query = null)
        {
            var timePeriodsMeta = _observationService.GetTimePeriodsMeta(subjectId,
                GetYears(query),
                query?.Countries,
                query?.Regions,
                query?.LocalAuthorities,
                query?.LocalAuthorityDistricts);

            return new LegendOptionsMetaValueModel<IEnumerable<TimePeriodMetaViewModel>>
            {
                Hint = "Filter statistics by a given start and end date",
                Legend = "Academic Year",
                Options = timePeriodsMeta.Select(tuple => new TimePeriodMetaViewModel
                {
                    Code = tuple.TimePeriod,
                    // TODO DFE-610 generate a label from code and year
                    Label = tuple.Year.ToString(),
                    Year = tuple.Year
                })
            };
        }

        private Dictionary<string, LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>> GetObservationalUnits(
            long subjectId,
            SubjectMetaQueryContext query = null)
        {
            return _observationService.GetObservationalUnitsMeta(subjectId,
                    GetYears(query),
                    query?.Countries,
                    query?.Regions,
                    query?.LocalAuthorities,
                    query?.LocalAuthorityDistricts)
                .Where(pair => pair.Value.Any())
                .ToDictionary(
                    pair => pair.Key.ToString().PascalCase(),
                    pair => new LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                    {
                        Hint = "",
                        Legend = pair.Key.GetEnumLabel(),
                        Options = _mapper.Map<IEnumerable<LabelValueViewModel>>(pair.Value)
                    });
        }

        private Dictionary<string, LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>> GetIndicators(
            long subjectId)
        {
            return _indicatorGroupService.GetIndicatorGroups(subjectId).ToDictionary(
                group => group.Label.PascalCase(),
                group => new LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>
                {
                    Label = group.Label,
                    Options = _mapper.Map<IEnumerable<IndicatorMetaViewModel>>(group.Indicators)
                }
            );
        }

        private Dictionary<string, LegendOptionsMetaValueModel<Dictionary<string,
            LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>> GetFilters(
            long subjectId,
            SubjectMetaQueryContext query = null)
        {
            var filterItems = _filterItemService.GetFilterItems(subjectId,
                GetYears(query),
                query?.Countries,
                query?.LocalAuthorities,
                query?.LocalAuthorityDistricts,
                query?.Regions);

            return filterItems
                .GroupBy(item => item.FilterGroup.Filter)
                .ToDictionary(
                    itemsGroupedByFilter => itemsGroupedByFilter.Key.Label.PascalCase(),
                    itemsGroupedByFilter => new LegendOptionsMetaValueModel<Dictionary<string,
                        LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>
                    {
                        Hint = itemsGroupedByFilter.Key.Hint,
                        Legend = itemsGroupedByFilter.Key.Label,
                        Options = itemsGroupedByFilter.GroupBy(item => item.FilterGroup).ToDictionary(
                            itemsGroupedByFilterGroup => itemsGroupedByFilterGroup.Key.Label.PascalCase(),
                            itemsGroupedByFilterGroup =>
                                new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                {
                                    Label = itemsGroupedByFilterGroup.Key.Label,
                                    Options = itemsGroupedByFilterGroup.Select(item => new LabelValueViewModel
                                    {
                                        Label = item.Label,
                                        Value = item.Id.ToString()
                                    })
                                })
                    });
        }

        private static IEnumerable<int> GetYears(SubjectMetaQueryContext query)
        {
            return query != null
                ? QueryUtil.YearsQuery(query.Years, query.StartYear, query.EndYear)
                : new List<int>();
        }
    }
}