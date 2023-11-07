using System;
using System.Collections.Generic;
using AutoMapper;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class MapperUtils
    {
        public static IMapper MapperForProfile<T>() where T : Profile, new()
        {
            return new MapperConfiguration(cfg => cfg.AddProfile<T>()).CreateMapper();
        }

        public static IMapper MapperForProfile<T>(Func<Type, object> serviceLocator) where T : Profile, new()
        {
            return new MapperConfiguration(cfg => cfg.AddProfile<T>()).CreateMapper(serviceLocator);
        }

        public static IMapper MapperForProfiles(IEnumerable<Profile> profiles)
        {
            return new MapperConfiguration(cfg => cfg.AddProfiles(profiles)).CreateMapper();
        }
    }
}