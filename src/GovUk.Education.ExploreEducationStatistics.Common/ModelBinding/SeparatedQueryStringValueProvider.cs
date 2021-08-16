#nullable enable
using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding
{
    public class SeparatedQueryStringValueProvider : QueryStringValueProvider
    {
        private readonly string _separator;

        public SeparatedQueryStringValueProvider(IQueryCollection values, string separator)
            : base(BindingSource.Query, values, CultureInfo.InvariantCulture)
        {
            _separator = separator;
        }

        public override ValueProviderResult GetValue(string key)
        {
            var result = base.GetValue(key);

            if (result == ValueProviderResult.None)
            {
                return result;
            }

            if (result.Values.Any(x => x.IndexOf(_separator, StringComparison.OrdinalIgnoreCase) > 0))
            {
                var splitValues = new StringValues(
                    result.Values
                        .SelectMany(x => x.Split(_separator, StringSplitOptions.RemoveEmptyEntries))
                        .ToArray()
                );

                return new ValueProviderResult(splitValues, result.Culture);
            }

            return result;
        }
    }
}