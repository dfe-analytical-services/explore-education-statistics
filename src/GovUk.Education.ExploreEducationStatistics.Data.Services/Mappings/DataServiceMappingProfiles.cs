using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Mappings
{
    public class DataServiceMappingProfiles : Profile
    {
        public DataServiceMappingProfiles()
        {
            CreateMap<BoundaryLevel, BoundaryLevelIdLabel>();

            CreateMap<Location, LocationViewModel>();

            AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesFromAssembly)
                .Where(p => typeof(ObservationalUnit).IsAssignableFrom(p))
                .ToList().ForEach(type =>
                {
                    if (type == typeof(LocalAuthority))
                    {
                        CreateMap<LocalAuthority, CodeNameViewModel>()
                            .ForMember(model => model.Code,
                                opts => opts.MapFrom(localAuthority => localAuthority.GetCodeOrOldCodeIfEmpty()));
                    }
                    else
                    {
                        CreateMap(type, typeof(CodeNameViewModel));
                    }
                });

            CreateMap<Content.Model.Publication, TopicPublicationViewModel>();

            CreateMap<Content.Model.Theme, ThemeViewModel>()
                .ForMember(dest => dest.Topics, opts => opts.Ignore());

            CreateMap<Content.Model.Topic, TopicViewModel>()
                .ForMember(dest => dest.Publications, opts => opts.Ignore());
        }

        private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
