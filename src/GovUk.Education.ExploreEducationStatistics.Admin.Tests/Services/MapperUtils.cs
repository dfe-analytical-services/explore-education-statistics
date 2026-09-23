#nullable enable
using System.Reflection;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public static class MapperUtils
{
    public static IMapper AdminMapper(ContentDbContext? contentDbContext = null)
    {
        var services = new ServiceCollection();

        // Note, the Admin AutoMapper profile currently has no AfterMap MappingAction classes
        // that depend on a DbContext or any other services requiring Dependency Injection.

        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>(), Array.Empty<Assembly>());

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IMapper>();
    }
}
