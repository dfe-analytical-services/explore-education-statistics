using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class CharacteristicResultBuilder : IResultBuilder<ICharacteristicData, TableBuilderCharacteristicData>
    {
        private readonly IMapper _mapper;

        public CharacteristicResultBuilder(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TableBuilderCharacteristicData BuildResult(ICharacteristicData data, ICollection<string> indicatorFilter)
        {
            return new TableBuilderCharacteristicData
            {
                TimePeriod = data.TimePeriod,
                TimeIdentifier = data.TimeIdentifier,
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