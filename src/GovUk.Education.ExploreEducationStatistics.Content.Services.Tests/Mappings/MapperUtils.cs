using System.Reflection;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Mappings;

// ReSharper disable once ClassNeverInstantiated.Global
public class MapperUtils
{
    /// <summary>
    /// Creates an AutoMapper profile that includes AfterMap support which require Dependency Injection in order to
    /// be created successfully. In this case, the AfterMap support classes require ContentDbContext to be wired in
    /// during use.
    /// </summary>
    public static IMapper ContentMapper(ContentDbContext? contentDbContext = null)
    {
        var services = new ServiceCollection();
        services.AddTransient(_ => new DataBlockViewModelPostMappingAction(contentDbContext));
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>(), Array.Empty<Assembly>());

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IMapper>();
    }
}
