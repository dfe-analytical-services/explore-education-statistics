using AutoMapper;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class MapperUtils
    {
        public static IMapper MapperForProfile<T>() where T : Profile, new()
        {
            var profile = new T();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));
            var mapper = new Mapper(configuration);
            return mapper;
        }   
    }
}