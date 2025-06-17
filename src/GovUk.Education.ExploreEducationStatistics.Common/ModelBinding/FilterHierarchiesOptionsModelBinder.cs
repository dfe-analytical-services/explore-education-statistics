#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;

public class FilterHierarchiesOptionsModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        // Read the request body
        using var reader = new StreamReader(bindingContext.HttpContext.Request.Body);
        var json = await reader.ReadToEndAsync();

        if (string.IsNullOrEmpty(json))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return;
        }

        var filterHierarchiesOptions = FullTableQuery.CreateFilterHierarchiesOptionsFromJson(json);

        bindingContext.Result = ModelBindingResult.Success(filterHierarchiesOptions);
    }
}
