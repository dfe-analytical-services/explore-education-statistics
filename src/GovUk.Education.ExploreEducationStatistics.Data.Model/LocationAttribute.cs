#nullable enable

using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public abstract record LocationAttribute(string? Code, string? Name)
{
    /// <summary>
    /// Produces a key string that represents this location attribute uniquely.
    /// </summary>
    /// <remarks>
    /// Used when adding Location cache entries to an in-memory cache while importing statistical data.
    /// 
    /// </remarks>
    public virtual string GetCacheKey()
    {
        return $"{GetType().Name}:{GetCodeOrFallback()}:{Name ?? string.Empty}";
    }

    public virtual string GetCodeOrFallback()
    {
        return Code ?? string.Empty;
    }

    public GeographicLevel GeographicLevel => Enum.Parse<GeographicLevel>(GetType().Name);
}

public record Country(string? Code, string? Name) : LocationAttribute(Code, Name);

public record EnglishDevolvedArea(string? Code, string? Name) : LocationAttribute(Code, Name);

public record Institution(string? Code, string? Name) : LocationAttribute(Code, Name);

public record LocalAuthority(string? Code, string? OldCode, string? Name) : LocationAttribute(Code, Name)
{
    public string? OldCode { get; } = OldCode;

    public override string GetCacheKey()
    {
        // Don't use GetCodeOrFallback here as the string needs to represent the local authority uniquely by all
        // attributes. Two local authorities with the same name and code but different old code are not identical.
        return $"{GetType().Name}:{Code ?? string.Empty}:{OldCode ?? string.Empty}:{Name ?? string.Empty}";
    }

    public override string GetCodeOrFallback()
    {
        return string.IsNullOrEmpty(Code) ? OldCode ?? string.Empty : Code;
    }
}

public record LocalAuthorityDistrict(string? Code, string? Name) : LocationAttribute(Code, Name);

public record LocalEnterprisePartnership(string? Code, string? Name) : LocationAttribute(Code, Name);

public record Mat(string? Code, string? Name) : LocationAttribute(Code, Name);

public record MayoralCombinedAuthority(string? Code, string? Name) : LocationAttribute(Code, Name);

public record OpportunityArea(string? Code, string? Name) : LocationAttribute(Code, Name);

public record ParliamentaryConstituency(string? Code, string? Name) : LocationAttribute(Code, Name);

public record PlanningArea(string? Code, string? Name) : LocationAttribute(Code, Name);

public record Provider(string? Code, string? Name) : LocationAttribute(Code, Name);

public record Region(string? Code, string? Name) : LocationAttribute(Code, Name);

public record RscRegion(string? Code) : LocationAttribute(Code, Code);

public record School(string? Code, string? Name) : LocationAttribute(Code, Name);

public record Sponsor(string? Code, string? Name) : LocationAttribute(Code, Name);

public record Ward(string? Code, string? Name) : LocationAttribute(Code, Name);
