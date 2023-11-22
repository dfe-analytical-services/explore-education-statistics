#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;

public class TrimStringModelBinder : IModelBinder
{
    private readonly IModelBinder _baseModelBinder;

    public TrimStringModelBinder(IModelBinder baseModelBinder)
    {
        _baseModelBinder = baseModelBinder;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        if (bindingContext.ModelType != typeof(string))
        {
            throw new ArgumentException("Model type must be a string");
        }

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult != ValueProviderResult.None)
        {
            var value = valueProviderResult.FirstValue;
            if (!string.IsNullOrEmpty(value))
            {
                bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

                var model = value.Trim();
                if (bindingContext.ModelMetadata.ConvertEmptyStringToNull && model.Length == 0)
                {
                    model = null;
                }

                bindingContext.Result = ModelBindingResult.Success(model);
                return Task.CompletedTask;
            }
        }

        // Delegate to the built-in binder
        return _baseModelBinder.BindModelAsync(bindingContext);
    }
}
