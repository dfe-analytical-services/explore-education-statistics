using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class
        LaCharacteristicResultBuilder : IResultBuilder<ICharacteristicGeographicData, TableBuilderLaCharacteristicData>
    {
        private readonly IMapper _mapper;

        public LaCharacteristicResultBuilder(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TableBuilderLaCharacteristicData BuildResult(ICharacteristicGeographicData data,
            ICollection<string> indicatorFilter)
        {
            return new TableBuilderLaCharacteristicData
            {
                TimePeriod = data.TimePeriod,
                TimeIdentifier = data.TimeIdentifier,
                Country = data.Country,
                Region = data.Region,
                LocalAuthority = data.LocalAuthority,
                SchoolType = data.SchoolType,
                // TODO Label is not currently set in CharacteristicViewModel. Not sure if it needs to be?
                // TODO If Label is not used then is CharacteristicViewModel needed?
                Characteristic = _mapper.Map<CharacteristicViewModel>(data.Characteristic),
                Indicators = indicatorFilter.Count > 0
                    ? QueryUtil.FilterIndicators(data.Indicators, indicatorFilter)
                    : data.Indicators
            };
        }
    }
}