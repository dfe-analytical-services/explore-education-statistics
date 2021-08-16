#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding
{
    /// <summary>
    /// Provides an instance of <see cref="SeparatedQueryModelBinder"/>
    /// when a suitable model parameter is detected e.g. an <see cref="IEnumerable{T}"/>.
    /// </summary>
    public class SeparatedQueryModelBinderProvider : IModelBinderProvider
    {
        private readonly string _separator;

        private static readonly HashSet<Type> SupportedElementTypes = new()
        {
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(byte),
            typeof(uint),
            typeof(ulong),
            typeof(ushort),
            typeof(Guid)
        };

        public SeparatedQueryModelBinderProvider(string separator)
        {
            _separator = separator;
        }

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (!IsSupported(context.Metadata))
            {
                return null;
            }

            IModelBinderProvider provider;

            if (context.Metadata.ModelType.IsArray)
            {
                provider = new ArrayModelBinderProvider();
            }
            else
            {
                provider = new CollectionModelBinderProvider();
            }

            var binder = provider.GetBinder(context);

            return new SeparatedQueryModelBinder(binder, _separator);
        }

        private bool IsSupported(ModelMetadata metadata) {
            return metadata.BindingSource == BindingSource.Query
                   && metadata.IsEnumerableType
                   && SupportedElementTypes.Contains(metadata.ElementType);
        }
    }
}