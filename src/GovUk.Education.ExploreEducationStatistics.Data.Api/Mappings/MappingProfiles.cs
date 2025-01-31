#nullable enable
using AutoMapper;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings;

/**
 * AutoMapper Profile which is configured by AutoMapper.Extensions.Microsoft.DependencyInjection.
 */
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Null collections will be mapped to null collections instead of empty collections.
        AllowNullCollections = true;
    }
}
