using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;

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
        typeof(Guid),
    };

    public SeparatedQueryModelBinderProvider(string separator)
    {
        _separator = separator;
    }

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var separatorAttribute = context.Metadata is DefaultModelMetadata defaultMetadata
            ? defaultMetadata.Attributes.Attributes.FirstOrDefault(attribute => attribute is QuerySeparatorAttribute)
                as QuerySeparatorAttribute
            : null;

        if (!IsSupported(context, separatorAttribute))
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

        var binder =
            provider.GetBinder(context)
            ?? throw new NullReferenceException($"Could not get binder for {provider.GetType().Name}");

        return new SeparatedQueryModelBinder(binder, separatorAttribute?.Separator ?? _separator);
    }

    private bool IsSupported(ModelBinderProviderContext context, QuerySeparatorAttribute? attribute)
    {
        if (attribute is not null && context.Metadata.IsEnumerableType)
        {
            return true;
        }

        return context.BindingInfo.BindingSource == BindingSource.Query
            && context.Metadata.IsEnumerableType
            && SupportedElementTypes.Contains(context.Metadata.ElementType!);
    }
}
