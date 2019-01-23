using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Characteristic = GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Characteristic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<GeographicModel, TidyDataGeographic>().ReverseMap();
            CreateMap<CharacteristicViewModel, Characteristic>().ReverseMap();
        }
    }
}