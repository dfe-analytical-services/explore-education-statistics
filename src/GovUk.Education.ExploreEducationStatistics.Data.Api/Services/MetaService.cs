using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class MetaService : IMetaService
    {
        private readonly IIndicatorGroupService _indicatorGroupService;
        private readonly IObservationService _observationService;
        private readonly IReleaseService _releaseService;
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public MetaService(
            IIndicatorGroupService indicatorGroupService,
            IObservationService observationService,
            IReleaseService releaseService,
            ISubjectService subjectService,
            IMapper mapper)
        {
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

            var filters = GetFilters();
            var indicators = GetIndicators(subject);
            var locationMeta = _observationService.GetLocationMeta(subject.Id);

            var national = locationMeta.Country.Distinct().Select(MapCountry);

            var localAuthority = locationMeta.LocalAuthority.Distinct().Select(MapLocalAuthority);

            var localAuthorityDistrict =
                locationMeta.LocalAuthorityDistrict.Distinct().Select(MapLocalAuthorityDistrict);

            var region = locationMeta.Region.Distinct().Select(MapRegion);

            return new SubjectMetaViewModel
            {
                CategoricalFilters = filters,
                Indicators = indicators,
                ObservationalUnits = new ObservationalUnitsMetaViewModel
                {
                    Location = new LegendOptionsMetaValueModel<LocationMetaViewModel>
                    {
                        Hint = "Filter statistics by location level",
                        Legend = "Location",
                        Options = new LocationMetaViewModel
                        {
                            LocalAuthority = new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                            {
                                Label = "Local Authority",
                                Options = localAuthority
                            },
                            LocalAuthorityDistrict = new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                            {
                                Label = "Local Authority District",
                                Options = localAuthorityDistrict
                            },
                            National = new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                            {
                                Label = "National",
                                Options = national
                            },
                            Region = new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                            {
                                Label = "Region",
                                Options = region
                            },
                            School = new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                            {
                                Label = "School",
                                Options = new List<LabelValueViewModel>()
                            }
                        }
                    },
                    TimePeriod = new LegendOptionsMetaValueModel<IEnumerable<TimePeriodMetaViewModel>>
                    {
                        Hint = "Filter statistics by a given start and end date",
                        Legend = "Academic Year",
                        Options = new List<TimePeriodMetaViewModel>
                        {
                            new TimePeriodMetaViewModel
                            {
                                Code = TimeIdentifier.AY,
                                Label = "2011/12",
                                Year = 2011
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimeIdentifier.AY,
                                Label = "2012/13",
                                Year = 2012
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimeIdentifier.AY,
                                Label = "2013/14",
                                Year = 2013
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimeIdentifier.AY,
                                Label = "2014/15",
                                Year = 2014
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimeIdentifier.AY,
                                Label = "2015/16",
                                Year = 2015
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimeIdentifier.AY,
                                Label = "2016/17",
                                Year = 2016
                            }
                        }
                    }
                }
            };
        }

        private Dictionary<string, LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>> GetIndicators(
            Subject subject)
        {
            return _indicatorGroupService.GetGroupedIndicatorsBySubjectId(subject.Id).ToDictionary(
                pair => pair.Key.Id.ToString(),
                pair => new LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>
                {
                    Label = pair.Key.Label,
                    Options = _mapper.Map<IEnumerable<IndicatorMetaViewModel>>(pair.Value)
                }
            );
        }

        private Dictionary<string, LegendOptionsMetaValueModel<Dictionary<string,
            LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>> GetFilters()
        {
            return new Dictionary<string,
                LegendOptionsMetaValueModel<Dictionary<string,
                    LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>>
            {
                {
                    "1", // Filter.Id
                    new LegendOptionsMetaValueModel<Dictionary<string,
                        LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>
                    {
                        Hint = "Filter by pupil characteristic", // Filter.Hint
                        Legend = "Characteristic", // Filter.Label
                        Options =
                            new Dictionary<string, LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>
                            {
                                {
                                    "6", // FilterGroup.Id
                                    new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                    {
                                        Label = "NC year", // FilterGroup.Label
                                        Options = new List<LabelValueViewModel>
                                        {
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 1 and below", // FilterItem.Label
                                                Value = "29" // FilterItem.Id
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 2",
                                                Value = "30"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 3",
                                                Value = "31"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 4",
                                                Value = "32"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 5",
                                                Value = "33"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 6",
                                                Value = "34"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 7",
                                                Value = "35"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 8",
                                                Value = "36"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 9",
                                                Value = "37"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 10",
                                                Value = "38"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 11",
                                                Value = "39"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year 12 and above",
                                                Value = "40"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year not followed or missing",
                                                Value = "41"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "NC Year Unclassified",
                                                Value = "75"
                                            }
                                        }
                                    }
                                },
                                {
                                    "3",
                                    new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                    {
                                        Label = "Gender",
                                        Options = new List<LabelValueViewModel>
                                        {
                                            new LabelValueViewModel
                                            {
                                                Label = "Gender female",
                                                Value = "3"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "Gender male",
                                                Value = "4"
                                            }
                                        }
                                    }
                                },
                                {
                                    "9",
                                    new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                    {
                                        Label = "SEN provision",
                                        Options = new List<LabelValueViewModel>
                                        {
                                            new LabelValueViewModel
                                            {
                                                Label = "SEN provision No identified SEN",
                                                Value = "48"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "SEN provision Statement or EHCP",
                                                Value = "49"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "SEN provision SEN Support",
                                                Value = "50"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "SEN provision Unclassified",
                                                Value = "51"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "SEN provision statement",
                                                Value = "73"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "SEN provision SEN without statement",
                                                Value = "74"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "SEN provision School Action",
                                                Value = "80"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "SEN provision School Action Plus",
                                                Value = "81"
                                            }
                                        }
                                    }
                                },
                                {
                                    "7",
                                    new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                    {
                                        Label = "FSM",
                                        Options = new List<LabelValueViewModel>
                                        {
                                            new LabelValueViewModel
                                            {
                                                Label = "FSM eligible",
                                                Value = "42"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "FSM not eligible",
                                                Value = "43"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "FSM unclassified",
                                                Value = "44"
                                            }
                                        }
                                    }
                                },
                                {
                                    "1",
                                    new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                    {
                                        Label = "All pupils",
                                        Options = new List<LabelValueViewModel>
                                        {
                                            new LabelValueViewModel
                                            {
                                                Label = "All pupils",
                                                Value = "1"
                                            }
                                        }
                                    }
                                }
                            }
                    }
                },
                {
                    "2",
                    new LegendOptionsMetaValueModel<Dictionary<string,
                        LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>
                    {
                        Hint = "Filter by school type",
                        Legend = "School type",
                        Options =
                            new Dictionary<string, LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>
                            {
                                {
                                    "2",
                                    new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                    {
                                        Label = "Default",
                                        Options = new List<LabelValueViewModel>
                                        {
                                            new LabelValueViewModel
                                            {
                                                Label = "All schools",
                                                Value = "2"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "State-funded primary",
                                                Value = "70"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "State-funded secondary",
                                                Value = "71"
                                            },
                                            new LabelValueViewModel
                                            {
                                                Label = "Special",
                                                Value = "72"
                                            }
                                        }
                                    }
                                }
                            }
                    }
                }
            };
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