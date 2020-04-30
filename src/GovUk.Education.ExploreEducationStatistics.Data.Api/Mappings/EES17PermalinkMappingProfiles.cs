using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations.EES17;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings
{
    /**
     * Temporary mapping profile for EES-17.
     */
    public class EES17PermalinkMappingProfiles : Profile
    {
        public EES17PermalinkMappingProfiles()
        {
            CreateMap<EES17Permalink, Permalink>()
                .ForMember(permalink => permalink.Configuration,
                    m => m.MapFrom(permalink => permalink.Query.Configuration));

            CreateMap<EES17TableBuilderConfiguration, TableBuilderConfiguration>()
                .ForMember(configuration => configuration.TableHeaders,
                    m => m.MapFrom(configuration => configuration.TableHeadersConfig));

            CreateMap<EES17TableBuilderTableHeadersConfig, TableHeaders>();

            CreateMap<LabelValue, TableHeader>();

            CreateMap<EES17TableBuilderQueryContext, ObservationQueryContext>()
                .ForMember(context => context.Locations, m => m.MapFrom(context => new LocationQuery
                {
                    GeographicLevel = context.GeographicLevel,
                    Country = context.Country,
                    Institution = context.Institution,
                    LocalAuthority = context.LocalAuthority,
                    LocalAuthorityDistrict = context.LocalAuthorityDistrict,
                    LocalEnterprisePartnership = context.LocalEnterprisePartnership,
                    MultiAcademyTrust = context.MultiAcademyTrust,
                    MayoralCombinedAuthority = context.MayoralCombinedAuthority,
                    OpportunityArea = context.OpportunityArea,
                    ParliamentaryConstituency = context.ParliamentaryConstituency,
                    PlanningArea = context.PlanningArea,
                    Region = context.Region,
                    RscRegion = context.RscRegion,
                    Sponsor = context.Sponsor,
                    Ward = context.Ward
                }));
        }
    }
}