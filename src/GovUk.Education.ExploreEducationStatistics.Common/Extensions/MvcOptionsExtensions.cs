#nullable enable
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class MvcOptionsExtensions
{
    public static void AddCommaSeparatedQueryModelBinderProvider(this MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(","));
    }

    public static void AddTrimStringBinderProvider(this MvcOptions option)
    {
        var simpleTypeModelBinderProvider = option.ModelBinderProviders
            .FirstOrDefault(x => x.GetType() == typeof(SimpleTypeModelBinderProvider));

        if (simpleTypeModelBinderProvider == null)
        {
            return;
        }

        // The first provider that returns a binder is used, so insert before the SimpleTypeModelBinderProvider
        var index = option.ModelBinderProviders.IndexOf(simpleTypeModelBinderProvider);
        option.ModelBinderProviders.Insert(index, new TrimStringModelBinderProvider());
    }
}
