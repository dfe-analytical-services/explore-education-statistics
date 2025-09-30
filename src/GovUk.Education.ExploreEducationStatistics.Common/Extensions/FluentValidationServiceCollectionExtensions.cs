#nullable enable
using System.Reflection;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class FluentValidationServiceCollectionExtensions
{
    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        return AddFluentValidation(services, Assembly.GetCallingAssembly());
    }

    private static IServiceCollection AddFluentValidation(
        IServiceCollection services,
        Assembly assembly
    )
    {
        ValidatorOptions.Global.LanguageManager = new FluentValidationLanguageManager();

        services.AddValidatorsFromAssembly(assembly);

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<FluentValidationActionFilter>();
        });

        return services;
    }
}
