#nullable enable
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Internal;
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

    private static IServiceCollection AddFluentValidation(IServiceCollection services, Assembly assembly)
    {
        ValidatorOptions.Global.LanguageManager = new FluentValidationLanguageManager();

        ValidatorOptions.Global.UseCamelCasePropertyNames();

        services.AddValidatorsFromAssembly(assembly);

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<FluentValidationActionFilter>();
        });

        return services;
    }

    public static void UseCamelCasePropertyNames(this ValidatorConfiguration globalConfig)
    {
        globalConfig.PropertyNameResolver = (_, memberInfo, expression) =>
            JsonNamingPolicy.CamelCase.ConvertName(ResolvePropertyName(memberInfo, expression));
    }

    private static string ResolvePropertyName(MemberInfo? memberInfo, LambdaExpression? expression)
    {
        if (expression is not null)
        {
            var chain = PropertyChain.FromExpression(expression);

            if (chain.Count > 0)
            {
                return chain.ToString();
            }
        }

        if (memberInfo is not null)
        {
            return memberInfo.Name;
        }

        return string.Empty;
    }
}
