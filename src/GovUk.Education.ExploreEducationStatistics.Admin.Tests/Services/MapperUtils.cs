using AutoMapper;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class MapperUtils
    {
        public static IMapper MapperForProfile<T>() where T : Profile, new()
        {   
            return new MapperConfiguration(cfg => cfg.AddProfile<T>()).CreateMapper();
        }   
    }
}