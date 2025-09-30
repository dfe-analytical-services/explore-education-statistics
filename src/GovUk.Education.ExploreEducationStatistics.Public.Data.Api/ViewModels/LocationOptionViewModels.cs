using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.Converters;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A location option that can be used to filter a data set.
/// </summary>
[JsonConverter(typeof(LocationOptionViewModelJsonConverter))]
[SwaggerSubType(typeof(LocationCodedOptionViewModel))]
[SwaggerSubType(typeof(LocationLocalAuthorityOptionViewModel))]
[SwaggerSubType(typeof(LocationProviderOptionViewModel))]
[SwaggerSubType(typeof(LocationRscRegionOptionViewModel))]
[SwaggerSubType(typeof(LocationSchoolOptionViewModel))]
public abstract record LocationOptionViewModel
{
    /// <summary>
    /// The ID of the location.
    /// </summary>
    /// <example>bOmZ4</example>
    public required string Id { get; init; }

    /// <summary>
    /// The human-readable label of the location.
    /// </summary>
    /// <example>Sheffield</example>
    public required string Label { get; init; }

    public static LocationOptionViewModel Create(LocationOptionMetaLink link)
    {
        return Create(link.Option, link.PublicId);
    }

    public static LocationOptionViewModel Create(LocationOptionMeta optionMeta, string publicId)
    {
        return optionMeta switch
        {
            LocationCodedOptionMeta codedOption => new LocationCodedOptionViewModel
            {
                Id = publicId,
                Label = codedOption.Label,
                Code = codedOption.Code,
            },
            LocationLocalAuthorityOptionMeta localAuthorityOption =>
                new LocationLocalAuthorityOptionViewModel
                {
                    Id = publicId,
                    Label = localAuthorityOption.Label,
                    Code = localAuthorityOption.Code,
                    OldCode = localAuthorityOption.OldCode,
                },
            LocationProviderOptionMeta providerOption => new LocationProviderOptionViewModel
            {
                Id = publicId,
                Label = providerOption.Label,
                Ukprn = providerOption.Ukprn,
            },
            LocationRscRegionOptionMeta rscRegionOption => new LocationRscRegionOptionViewModel
            {
                Id = publicId,
                Label = rscRegionOption.Label,
            },
            LocationSchoolOptionMeta schoolOption => new LocationSchoolOptionViewModel
            {
                Id = publicId,
                Label = schoolOption.Label,
                Urn = schoolOption.Urn,
                LaEstab = schoolOption.LaEstab,
            },
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Check if the option has a major change when compared to another option.
    /// </summary>
    public abstract bool HasMajorChange(LocationOptionViewModel otherOption);
}

/// <summary>
/// A location option that can be identified by code and used to filter a data set.
/// </summary>
public record LocationCodedOptionViewModel : LocationOptionViewModel
{
    /// <summary>
    /// The code of the location.
    /// </summary>
    /// <example>E12000003</example>
    [JsonPropertyOrder(1)]
    public required string Code { get; init; }

    public override bool HasMajorChange(LocationOptionViewModel otherOption) =>
        otherOption is not LocationCodedOptionViewModel codedOption
        || Id != codedOption.Id
        || Code != codedOption.Code;
}

/// <summary>
/// A location option for a local authority that can be used to filter a data set.
/// </summary>
public record LocationLocalAuthorityOptionViewModel : LocationOptionViewModel
{
    /// <summary>
    /// The ONS code of the local authority.
    /// </summary>
    /// <example>E08000019</example>
    [JsonPropertyOrder(1)]
    public required string Code { get; init; }

    /// <summary>
    /// The old code (previously the LEA code) of the local authority.
    /// </summary>
    /// <example>373</example>
    [JsonPropertyOrder(1)]
    public required string OldCode { get; init; }

    public override bool HasMajorChange(LocationOptionViewModel otherOption) =>
        otherOption is not LocationLocalAuthorityOptionViewModel localAuthorityOption
        || Id != localAuthorityOption.Id
        || Code != localAuthorityOption.Code
        || OldCode != localAuthorityOption.OldCode;
}

/// <summary>
/// A location option for a provider that can be used to filter a data set.
/// </summary>
public record LocationProviderOptionViewModel : LocationOptionViewModel
{
    /// <summary>
    /// The UKPRN (UK provider reference number) of the provider.
    /// </summary>
    /// <example>12345678</example>
    [JsonPropertyOrder(1)]
    public required string Ukprn { get; init; }

    public override bool HasMajorChange(LocationOptionViewModel otherOption) =>
        otherOption is not LocationProviderOptionViewModel providerOption
        || Id != providerOption.Id
        || Ukprn != providerOption.Ukprn;
}

/// <summary>
/// A location option for an RSC region that can be used to filter a data set.
/// </summary>
public record LocationRscRegionOptionViewModel : LocationOptionViewModel
{
    public override bool HasMajorChange(LocationOptionViewModel otherOption) =>
        otherOption is not LocationRscRegionOptionViewModel rscOption || Id != rscOption.Id;
}

/// <summary>
/// A location option for a school that can be used to filter a data set.
/// </summary>
public record LocationSchoolOptionViewModel : LocationOptionViewModel
{
    /// <summary>
    /// The URN (unique reference number) of the school.
    /// </summary>
    /// <example>123456</example>
    [JsonPropertyOrder(1)]
    public required string Urn { get; init; }

    /// <summary>
    /// The LAESTAB (local authority establishment number) of the school.
    /// </summary>
    /// <example>1234567</example>
    [JsonPropertyOrder(1)]
    public required string LaEstab { get; init; }

    public override bool HasMajorChange(LocationOptionViewModel otherOption) =>
        otherOption is not LocationSchoolOptionViewModel schoolOption
        || Id != schoolOption.Id
        || Urn != schoolOption.Urn
        || LaEstab != schoolOption.LaEstab;
}
