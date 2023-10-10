#nullable enable
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;

public class TrimStringModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!context.Metadata.IsComplexType && context.Metadata.ModelType == typeof(string))
        {
            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            return new TrimStringModelBinder(new SimpleTypeModelBinder(context.Metadata.ModelType, loggerFactory));
        }

        return null;
    }
}
