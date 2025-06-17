#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;

public class FilterHierarchiesOptionsModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.ModelType == typeof(List<FilterHierarchyOptions>))
        {
            return new FilterHierarchiesOptionsModelBinder();
        }

        return null;
    }
}
