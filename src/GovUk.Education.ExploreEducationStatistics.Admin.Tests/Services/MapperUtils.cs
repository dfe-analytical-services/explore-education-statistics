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
        services.AddTransient(_ => new DataBlockViewModelPostMappingAction(contentDbContext));
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>(), Array.Empty<Assembly>());

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IMapper>();
    }
}
