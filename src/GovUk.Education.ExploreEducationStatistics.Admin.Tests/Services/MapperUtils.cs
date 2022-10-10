#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class MapperUtils
    {
        public static IMapper AdminMapper()
        {
            return Common.Services.MapperUtils.MapperForProfile<MappingProfiles>();
        }
    }
}
