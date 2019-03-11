using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

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
            ICollection<string> attributeFilter)
        {
            return new TableBuilderLaCharacteristicData
            {
                Year = data.Year,
                Term = data.Term,
                Country = data.Country,
                Region = data.Region,
                LocalAuthority = data.LocalAuthority,
                SchoolType = data.SchoolType,
                // TODO Label is not currently set in CharacteristicViewModel. Not sure if it needs to be?
                // TODO If Label is not used then is CharacteristicViewModel needed?
                Characteristic = _mapper.Map<CharacteristicViewModel>(data.Characteristic),
                Attributes = attributeFilter.Count > 0
                    ? QueryUtil.FilterAttributes(data.Attributes, attributeFilter)
                    : data.Attributes
            };
        }
    }
}