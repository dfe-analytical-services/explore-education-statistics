using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class MetaService : IMetaService
    {
        private readonly IFilterService _filterService;
        private readonly IIndicatorGroupService _indicatorGroupService;
        private readonly IObservationService _observationService;
        private readonly IReleaseService _releaseService;
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public MetaService(
            IFilterService filterService,
            IIndicatorGroupService indicatorGroupService,
            IObservationService observationService,
            IReleaseService releaseService,
            ISubjectService subjectService,
            IMapper mapper)
        {
            _filterService = filterService;
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

        public SubjectMetaViewModel GetSubjectMeta(long subjectId)
        {
            // TODO check subject exists
            var subject = _subjectService.Find(subjectId);

            var filters = GetFilters(subject.Id);
            var indicators = GetIndicators(subject.Id);
            var locationMeta = _observationService.GetLocationMeta(subject.Id);
            var timePeriods = GetTimePeriods(subject.Id);

            var national = locationMeta.Country.Distinct().Select(MapCountry);

            var localAuthority = locationMeta.LocalAuthority.Distinct().Select(MapLocalAuthority);

            var localAuthorityDistrict =
                locationMeta.LocalAuthorityDistrict.Distinct().Select(MapLocalAuthorityDistrict);

            var region = locationMeta.Region.Distinct().Select(MapRegion);

            return new SubjectMetaViewModel
            {
                Filters = filters,
                Indicators = indicators,
                Locations =
                    new Dictionary<string, LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>
                    {
                        {
                            GeographicLevel.Local_Authority.ToString().CamelCase(),
                            new LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                            {
                                Hint = "",
                                Legend = "Local Authority",
                                Options = localAuthority
                            }
                        },
                        {
                            GeographicLevel.Local_Authority_District.ToString().CamelCase(),
                            new LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                            {
                                Hint = "",
                                Legend = "Local Authority District",
                                Options = localAuthorityDistrict
                            }
                        },
                        {
                            GeographicLevel.National.ToString().CamelCase(),
                            new LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                            {
                                Hint = "",
                                Legend = "National",
                                Options = national
                            }
                        },
                        {
                            GeographicLevel.Regional.ToString().CamelCase(),
                            new LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                            {
                                Hint = "",
                                Legend = "Region",
                                Options = region
                            }
                        }
                    },
                TimePeriod = timePeriods
            };
        }

        private LegendOptionsMetaValueModel<IEnumerable<TimePeriodMetaViewModel>> GetTimePeriods(long subjectId)
        {
            var timePeriodsMeta = _observationService.GetTimePeriodsMeta(subjectId);
            return new LegendOptionsMetaValueModel<IEnumerable<TimePeriodMetaViewModel>>
            {
                Hint = "Filter statistics by a given start and end date",
                Legend = "Academic Year",
                Options = timePeriodsMeta.Select(tuple => new TimePeriodMetaViewModel
                {
                    Code = tuple.TimePeriod,
                    // TODO generate a label from code and year
                    Label = tuple.Year.ToString(),
                    Year = tuple.Year
                })
            };
        }

        private Dictionary<string, LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>> GetIndicators(
            long subjectId)
        {
            return _indicatorGroupService.GetIndicatorGroupsBySubjectId(subjectId).ToDictionary(
                pair => pair.Key.Label.CamelCase(),
                pair => new LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>
                {
                    Label = pair.Key.Label,
                    Options = _mapper.Map<IEnumerable<IndicatorMetaViewModel>>(pair.Value)
                }
            );
        }

        private Dictionary<string, LegendOptionsMetaValueModel<Dictionary<string,
            LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>> GetFilters(long subjectId)
        {
            return _filterService.GetFiltersBySubjectId(subjectId).ToDictionary(
                filter => filter.Label.CamelCase(),
                filter => new LegendOptionsMetaValueModel<Dictionary<string,
                    LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>
                {
                    Hint = filter.Hint,
                    Legend = filter.Label,
                    Options = filter.FilterGroups.ToDictionary(
                        group => group.Label.CamelCase() + "_" + group.Id,
                        group => new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                        {
                            Label = group.Label,
                            Options = group.FilterItems.Select(item => new LabelValueViewModel
                            {
                                Label = item.Label,
                                Value = item.Id.ToString()
                            })
                        })
                });
        }

        private LabelValueViewModel MapCountry(Country country)
        {
            return _mapper.Map<LabelValueViewModel>(country);
        }

        private LabelValueViewModel MapLocalAuthority(LocalAuthority localAuthority)
        {
            return _mapper.Map<LabelValueViewModel>(localAuthority);
        }

        private LabelValueViewModel MapLocalAuthorityDistrict(LocalAuthorityDistrict localAuthorityDistrict)
        {
            return _mapper.Map<LabelValueViewModel>(localAuthorityDistrict);
        }

        private LabelValueViewModel MapRegion(Region region)
        {
            return _mapper.Map<LabelValueViewModel>(region);
        }
    }
}