using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            var subject = _subjectService.Find(subjectId, new List<Expression<Func<Subject, object>>> {s => s.Release});

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
                PublicationId = subject.Release.PublicationId,
                Characteristics = GetCharacteristicMetas(subject.Id),
                Indicators = GetIndicatorMetas(subject.Id),
                ObservationalUnits = new ObservationalUnitsMetaViewModel
                {
                    Location = new LocationMetaViewModel
                    {
                        LocalAuthority = localAuthority,
                        National = national,
                        Region = region,
                        School = new List<LabelValueViewModel>()
                    },
                    TimePeriod = new TimePeriodMetaViewModel
                    {
                        Hint = "Filter statistics by a given start and end date",
                        Legend = "Academic Year",
                        Options = new List<LabelValueViewModel>
                        {
                            new LabelValueViewModel
                            {
                                Label = "2011/12",
                                Value = "201112"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2012/13",
                                Value = "201213"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2013/14",
                                Value = "201314"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2014/15",
                                Value = "201415"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2015/16",
                                Value = "201516"
                            },
                            new LabelValueViewModel
                            {
                                Label = "2016/17",
                                Value = "201617"
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