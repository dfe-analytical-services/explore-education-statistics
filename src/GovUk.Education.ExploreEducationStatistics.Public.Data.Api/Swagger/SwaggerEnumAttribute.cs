using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;

/// <summary>
/// Configures Swashbuckle to provide enum documentation from a specified
/// enum type, overriding any default behaviour.
///
/// Useful for cases where the enum type can't be used directly in the model
/// (e.g. when deserializing as a string instead of the enum), but we still want
/// to document it using that enum's characteristics.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class SwaggerEnumAttribute(
    Type type,
    SwaggerEnumSerializer serializer = SwaggerEnumSerializer.Ref
) : Attribute
{
    /// <summary>
    /// The enum type to use.
    /// </summary>
    public Type Type { get; init; } = type;

    /// <summary>
    /// The way in which the enum should be serialized.
    /// </summary>
    public SwaggerEnumSerializer Serializer { get; init; } = serializer;
}

public enum SwaggerEnumSerializer
{
    /// <summary>
    /// Use `allOf` to reference the enum schema.
    /// </summary>
    Ref,

    /// <summary>
    /// Use enums defined by the enum schema.
    /// </summary>
    Schema,

    /// <summary>
    /// Use the enum's integer serialization.
    /// </summary>
    Int,

    /// <summary>
    /// Use the enum's string serialization.
    /// </summary>
    String,

    /// <summary>
    /// Use the enum's value serialization (using <see cref="EnumLabelValueAttribute"/>).
    /// </summary>
    Value,

    /// <summary>
    /// Use the enum's label serialization (using <see cref="EnumLabelValueAttribute"/>).
    /// </summary>
    Label,
}
