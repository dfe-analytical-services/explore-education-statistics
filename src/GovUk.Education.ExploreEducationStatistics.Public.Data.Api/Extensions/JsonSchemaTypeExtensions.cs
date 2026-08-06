using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Extensions;

public static class JsonSchemaTypeExtensions
{
    /// <summary>
    /// Returns the type with the specified flag cleared.
    /// </summary>
    /// <param name="type">The <see cref="JsonSchemaType"/> to modify.</param>
    /// <param name="flags">The <see cref="JsonSchemaType"/> flags to clear.</param>
    /// <returns>The <see cref="JsonSchemaType"/> with the specified flags cleared.</returns>
    [return: NotNullIfNotNull(nameof(type))]
    public static JsonSchemaType? WithoutFlags(this JsonSchemaType? type, JsonSchemaType flags) => type & ~flags;
}
