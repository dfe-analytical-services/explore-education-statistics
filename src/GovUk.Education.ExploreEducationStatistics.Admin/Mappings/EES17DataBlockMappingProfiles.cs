using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Migrations.EES17;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Migrations.EES17;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Mappings
{
    /**
     * Temporary mapping profile for EES-17.
     */
    public class EES17DataBlockMappingProfiles : Profile
    {
        public EES17DataBlockMappingProfiles()
        {
            CreateMap<EES17Table, TableBuilderConfiguration>();

            CreateMap<EES17TableHeaders, TableHeaders>()
                .ForMember(headers => headers.Columns, m => m.MapFrom(headers => headers.columns))
                .ForMember(headers => headers.Rows, m => m.MapFrom(headers => headers.rows))
                .ForMember(headers => headers.ColumnGroups, m => m.MapFrom(headers => headers.columnGroups))
                .ForMember(headers => headers.RowGroups, m => m.MapFrom(headers => headers.rowGroups));

            CreateMap<EES17TableOption, TableHeader>()
                .ForMember(header => header.Value, m => m.MapFrom(option => option.value));

            CreateMap<EES17TableRowGroupOption, TableHeader>()
                .ForMember(header => header.Level, m => m.Ignore())
                .ForMember(header => header.Value, m => m.MapFrom(option => option.value));

            CreateMap<EES17ObservationQueryContext, ObservationQueryContext>()
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