using System.Collections.Generic;
using AutoMapper;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class MapperUtils
    {
        public static IMapper MapperForProfile<T>() where T : Profile, new()
        {
            return new MapperConfiguration(cfg => cfg.AddProfile<T>()).CreateMapper();
        }

        public static IMapper MapperForProfiles(IEnumerable<Profile> profiles) 
        {
            return new MapperConfiguration(cfg => cfg.AddProfiles(profiles)).CreateMapper();
        }
    }
}