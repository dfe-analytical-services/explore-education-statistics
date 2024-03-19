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
    SwaggerEnumSerializer serializer = SwaggerEnumSerializer.Default)
    : Attribute
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
    Default,
    Int,
    String,
    Value,
    Label
}
