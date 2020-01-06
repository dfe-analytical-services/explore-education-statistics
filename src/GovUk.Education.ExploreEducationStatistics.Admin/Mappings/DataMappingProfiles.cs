using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    public class DataMappingProfiles : Profile
    {
        public DataMappingProfiles()
        {
            CreateMap<BoundaryLevel, BoundaryLevelIdLabel>();

            CreateMap<Indicator, IndicatorMetaViewModel>()
                .ForMember(dest => dest.Value, opts => opts.MapFrom(indicator => indicator.Id))
                .ForMember(dest => dest.Unit, opts => opts.MapFrom(indicator => indicator.Unit.GetEnumValue()));

            CreateMap<Location, LocationViewModel>();

            AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesFromAssembly)
                .Where(p => typeof(IObservationalUnit).IsAssignableFrom(p))
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

            CreateMap<Publication, PublicationMetaViewModel>();

            CreateMap<Subject, IdLabel>()
                .ForMember(dest => dest.Label, opts => opts.MapFrom(subject => subject.Name));

            CreateMap<Theme, ThemeMetaViewModel>();

            CreateMap<Topic, TopicMetaViewModel>();
        }

        private IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
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