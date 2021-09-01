#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding
{
    /// <summary>
    /// Model binder allowing us to parse values delimited by a separator for
    /// query string keys. This is particularly useful for compressing
    /// the overall size of the URL as the traditional query string
    /// syntax (e.g. `key=a&key=b&key=c`) is really inefficient.
    /// </summary>
    public class SeparatedQueryModelBinder : IModelBinder
    {
        private readonly IModelBinder _modelBinder;
        private readonly string _separator;

        public SeparatedQueryModelBinder(IModelBinder modelBinder, string separator)
        {
            _modelBinder = modelBinder;
            _separator = separator;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var query = bindingContext.HttpContext.Request.Query;

            // In the below, we swap the current provider with our own, and then swap
            // it back after the model binder has completed binding to avoid potentially
            // messing up the value provider for the next value in the current binding.
            if (bindingContext.ValueProvider is CompositeValueProvider compositeProvider)
            {
                var queryStringProvider = compositeProvider
                    .FirstOrDefault(provider => provider is QueryStringValueProvider);

                if (queryStringProvider is null)
                {
                    return;
                }

                var index = compositeProvider.IndexOf(queryStringProvider);

                compositeProvider[index] =
                    new SeparatedQueryStringValueProvider(query, _separator);

                await _modelBinder.BindModelAsync(bindingContext);

                compositeProvider[index] = queryStringProvider;
            }
            else if (bindingContext.ValueProvider is QueryStringValueProvider)
            {
                var originalProvider = bindingContext.ValueProvider;

                bindingContext.ValueProvider =
                    new SeparatedQueryStringValueProvider(query, _separator);

                await _modelBinder.BindModelAsync(bindingContext);

                bindingContext.ValueProvider = originalProvider;
            }
            else
            {
                await _modelBinder.BindModelAsync(bindingContext);
            }
        }
    }
}