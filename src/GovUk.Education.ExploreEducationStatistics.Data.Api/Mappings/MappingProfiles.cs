using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

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
            
            CreateMap<FastTrack, FastTrackViewModel>();
            
            CreateMap<Permalink, PermalinkViewModel>();
            
            CreateMap<ObservationQueryContext, TableBuilderQueryViewModel>();
        }
    }
}