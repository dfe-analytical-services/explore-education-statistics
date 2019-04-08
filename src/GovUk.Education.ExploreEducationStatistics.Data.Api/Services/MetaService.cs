using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class MetaService : IMetaService
    {
        private readonly ICharacteristicDataService _characteristicDataService;
        private readonly IGeographicDataService _geographicDataService;
        private readonly IReleaseService _releaseService;
        private readonly ISubjectService _subjectService;
        private readonly IMapper _mapper;

        public MetaService(ICharacteristicDataService characteristicDataService,
            IGeographicDataService geographicDataService,
            IReleaseService releaseService,
            ISubjectService subjectService,
            IMapper mapper)
        {
            _characteristicDataService = characteristicDataService;
            _geographicDataService = geographicDataService;
            _releaseService = releaseService;
            _subjectService = subjectService;
            _mapper = mapper;
        }

        public PublicationMetaViewModel GetPublicationMeta(Guid publicationId)
        {
            var releaseId = _releaseService.GetLatestRelease(publicationId);

            var subjectMetaViewModels = _subjectService.FindMany(subject => subject.Release.Id == releaseId)
                .Select(subject => _mapper.Map<IdLabelViewModel>(subject));

            return new PublicationMetaViewModel
            {
                PublicationId = publicationId,
                Subjects = subjectMetaViewModels
            };
        }

        public SubjectMetaViewModel GetSubjectMeta(long subjectId)
        {
            var subject = _subjectService.Find(subjectId);

            var levelMetaGeographic = _geographicDataService.GetLevelMeta(subjectId);
            var levelMetaCharacteristic = _characteristicDataService.GetLevelMeta(subjectId);

            var national = levelMetaGeographic.Country.Concat(levelMetaCharacteristic.Country)
                .Distinct().Select(MapCountry);

            var localAuthority = levelMetaGeographic.LocalAuthority.Concat(levelMetaCharacteristic.LocalAuthority)
                .Distinct().Select(MapLocalAuthority);

            var region = levelMetaGeographic.Region.Concat(levelMetaCharacteristic.Region)
                .Distinct().Select(MapRegion);

            return new SubjectMetaViewModel
            {
                CategoricalFilters =
                    new Dictionary<string,
                        LegendOptionsMetaValueModel<Dictionary<string,
                            LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>>
                    {
                        {
                            "characteristics",
                            new LegendOptionsMetaValueModel<Dictionary<string,
                                LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>
                            {
                                Hint = "Filter by pupil characteristics",
                                Legend = "Characteristics",
                                Options =
                                    new Dictionary<string, LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>
                                    {
                                        {
                                            "age",
                                            new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                            {
                                                Label = "Age",
                                                Options = new List<LabelValueViewModel>
                                                {
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 16",
                                                        Value = "Age_16"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 15",
                                                        Value = "Age_15"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 14",
                                                        Value = "Age_14"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 13",
                                                        Value = "Age_13"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 12",
                                                        Value = "Age_12"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 11",
                                                        Value = "Age_11"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 19 and over",
                                                        Value = "Age_19_and_over"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 10",
                                                        Value = "Age_10"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 8",
                                                        Value = "Age_8"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 7",
                                                        Value = "Age_7"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 6",
                                                        Value = "Age_6"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 5",
                                                        Value = "Age_5"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age four and under",
                                                        Value = "Age_4_and_under"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 18",
                                                        Value = "Age_18"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 9",
                                                        Value = "Age_9"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Age 17",
                                                        Value = "Age_17"
                                                    }
                                                }
                                            }
                                        },
                                        {
                                            "gender",
                                            new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                            {
                                                Label = "Gender",
                                                Options = new List<LabelValueViewModel>
                                                {
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Girls",
                                                        Value = "Gender_female"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Boys",
                                                        Value = "Gender_male"
                                                    }
                                                }
                                            }
                                        },
                                        {
                                            "sen_provision",
                                            new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                            {
                                                Label = "SEN provision",
                                                Options = new List<LabelValueViewModel>
                                                {
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "SEN support",
                                                        Value = "SEN_provision_SEN_Support"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "No identified SEN",
                                                        Value = "SEN_provision_No_identified_SEN"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Statement of SEN or EHC plan",
                                                        Value = "SEN_provision_Statement_or_EHCP"
                                                    }
                                                }
                                            }
                                        },
                                        {
                                            "free_school_meals",
                                            new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                            {
                                                Label = "Free school meals",
                                                Options = new List<LabelValueViewModel>
                                                {
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "FSM unclassified",
                                                        Value = "FSM_unclassified"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "FSM not eligible",
                                                        Value = "FSM_Not_eligible"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "FSM eligible",
                                                        Value = "FSM_eligible"
                                                    }
                                                }
                                            }
                                        },
                                        {
                                            "total",
                                            new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                            {
                                                Label = "Total",
                                                Options = new List<LabelValueViewModel>
                                                {
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "All pupils",
                                                        Value = "Total"
                                                    }
                                                }
                                            }
                                        }
                                    }
                            }
                        },
                        {
                            "schoolTypes",
                            new LegendOptionsMetaValueModel<Dictionary<string,
                                LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>>
                            {
                                Hint = "Filter by number of pupils in school type(s)",
                                Legend = "School type",
                                Options =
                                    new Dictionary<string, LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>>
                                    {
                                        {
                                            "default",
                                            new LabelOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                                            {
                                                Label = "",
                                                Options = new List<LabelValueViewModel>
                                                {
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Total",
                                                        Value = "Total"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Primary schools",
                                                        Value = "State_Funded_Primary"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Secondary schools",
                                                        Value = "State_Funded_Secondary"
                                                    },
                                                    new LabelValueViewModel
                                                    {
                                                        Label = "Special schools",
                                                        Value = "Special"
                                                    }
                                                }
                                            }
                                        }
                                    }
                            }
                        }
                    },
                Characteristics = GetCharacteristicMetas(subject.Id),
                Indicators = GetIndicatorMetas(subject.Id),
                IndicatorsPrototype = new Dictionary<string, LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>>
                {
                    {
                        "absence_by_reason",
                        new LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>
                        {
                            Label = "Absence by reason", Options = new List<IndicatorMetaViewModel>
                            {
                                new IndicatorMetaViewModel
                                {
                                    Label = "Number of excluded sessions",
                                    // TODO DFE-412 Remove Name which was used by the original table tool
                                    Name = "Not used",
                                    Unit = "",
                                    Value = "sess_auth_excluded"
                                }
                            }
                        }
                    }
                },
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
                                Code = TimePeriod.AY,
                                Label = "2011/12",
                                Year = 2011
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimePeriod.AY,
                                Label = "2012/13",
                                Year = 2012
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimePeriod.AY,
                                Label = "2013/14",
                                Year = 2013
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimePeriod.AY,
                                Label = "2014/15",
                                Year = 2014
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimePeriod.AY,
                                Label = "2015/16",
                                Year = 2015
                            },
                            new TimePeriodMetaViewModel
                            {
                                Code = TimePeriod.AY,
                                Label = "2016/17",
                                Year = 2016
                            }
                        }
                    }
                }
            };
        }

        private Dictionary<string, IEnumerable<IndicatorMetaViewModel>> GetIndicatorMetas(long subjectId)
        {
            return _subjectService.GetIndicatorMetaGroups(subjectId)
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Select(MapIndicatorMeta)
                );
        }

        private Dictionary<string, IEnumerable<CharacteristicMetaViewModel>> GetCharacteristicMetas(long subjectId)
        {
            return _subjectService.GetCharacteristicMetaGroups(subjectId)
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Select(MapCharacteristicMeta)
                );
        }

        private IndicatorMetaViewModel MapIndicatorMeta(IndicatorMeta indicatorMeta)
        {
            return _mapper.Map<IndicatorMetaViewModel>(indicatorMeta);
        }

        private CharacteristicMetaViewModel MapCharacteristicMeta(CharacteristicMeta characteristicMeta)
        {
            return _mapper.Map<CharacteristicMetaViewModel>(characteristicMeta);
        }

        private LabelValueViewModel MapCountry(Country country)
        {
            return _mapper.Map<LabelValueViewModel>(country);
        }

        private LabelValueViewModel MapLocalAuthority(LocalAuthority localAuthority)
        {
            return _mapper.Map<LabelValueViewModel>(localAuthority);
        }

        private LabelValueViewModel MapRegion(Region region)
        {
            return _mapper.Map<LabelValueViewModel>(region);
        }
    }
}