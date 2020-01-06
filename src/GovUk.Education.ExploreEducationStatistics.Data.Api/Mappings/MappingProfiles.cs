using System;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings
{
    /**
     * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
     */
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Null collections will be mapped to null collections instead of empty collections.
            AllowNullCollections = true;
            
            CreateMap<BoundaryLevel, BoundaryLevelIdLabel>();
            
            CreateMap<FastTrack, FastTrackViewModel>();

            CreateMap<Indicator, IndicatorMetaViewModel>()
                .ForMember(dest => dest.Value, opts => opts.MapFrom(indicator => indicator.Id))
                .ForMember(dest => dest.Unit, opts => opts.MapFrom(indicator => indicator.Unit.GetEnumValue()));

            CreateMap<Location, LocationViewModel>();

            AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
                .Where(p => typeof(IObservationalUnit).IsAssignableFrom(p))
                .ToList().ForEach(type =>
                {
                    if (type == typeof(LocalAuthority))
                    {
                        CreateMap<LocalAuthority, CodeNameViewModel>()
                            .ForMember(model => model.Code, opts => opts.MapFrom(localAuthority => localAuthority.GetCodeOrOldCodeIfEmpty()));
                    }
                    else
                    {
                        CreateMap(type, typeof(CodeNameViewModel));
                    }
                });

            CreateMap<Permalink, PermalinkViewModel>();

            CreateMap<Publication, PublicationMetaViewModel>();

            CreateMap<Subject, IdLabel>()
                .ForMember(dest => dest.Label, opts => opts.MapFrom(subject => subject.Name));

            CreateMap<TableBuilderQueryContext, TableBuilderQueryViewModel>();
            
            CreateMap<Theme, ThemeMetaViewModel>();

            CreateMap<Topic, TopicMetaViewModel>();
        }
    }
}