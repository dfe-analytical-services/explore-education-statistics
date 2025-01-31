using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;

/// <summary>
/// Modifies the model metadata to stop empty strings being converted to null.
/// This is mostly for query strings with empty parameters (e.g. `?foo=`) where
/// the presence of the query parameter should bind an empty string to the model.
/// </summary>
public class EmptyStringMetadataDetailsProvider : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        if (context.Key.MetadataKind == ModelMetadataKind.Property)
        {
            context.DisplayMetadata.ConvertEmptyStringToNull = false;
        }
    }
}
