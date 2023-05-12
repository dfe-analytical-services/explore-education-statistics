# nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Exceptions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

/// <summary>
/// Class responsible for up-front calculation of the column indexes to look up particular
/// expected pieces of information from a given data file. This includes column indexes for
/// Geographic Level, various Location attributes and other  mandatory information like Time
/// Periods.
/// </summary>
public class FixedInformationDataFileReader
{
    private enum LocationColumn
    {
        [EnumLabelValue("country_code")]
        CountryCode,
        
        [EnumLabelValue("country_name")]
        CountryName,
        
        [EnumLabelValue("english_devolved_area_code")]
        EnglishDevolvedAreaCode,
        
        [EnumLabelValue("english_devolved_area_name")]
        EnglishDevolvedAreaName,
    
        [EnumLabelValue("institution_id")]
        InstitutionCode,
        
        [EnumLabelValue("institution_name")]
        InstitutionName,
    
        [EnumLabelValue("new_la_code")]
        NewLaCode,
        
        [EnumLabelValue("old_la_code")]
        OldLaCode,
        
        [EnumLabelValue("la_name")]
        LaName,
    
        [EnumLabelValue("lad_code")]
        LadCode,
        
        [EnumLabelValue("lad_name")]
        LadName,
    
        [EnumLabelValue("local_enterprise_partnership_code")]
        LepCode,
        
        [EnumLabelValue("local_enterprise_partnership_name")]
        LepName,
    
        [EnumLabelValue("mayoral_combined_authority_code")]
        McaCode,
        
        [EnumLabelValue("mayoral_combined_authority_name")]
        McaName,
    
        [EnumLabelValue("trust_id")]
        MatCode,
        
        [EnumLabelValue("trust_name")]
        MatName,
    
        [EnumLabelValue("opportunity_area_code")]
        OpportunityAreaCode,
        
        [EnumLabelValue("opportunity_area_name")]
        OpportunityAreaName,
    
        [EnumLabelValue("pcon_code")]
        ParliamentaryConstituencyCode,
        
        [EnumLabelValue("pcon_name")]
        ParliamentaryConstituencyName,
    
        [EnumLabelValue("provider_ukprn")]
        ProviderCode,
        
        [EnumLabelValue("provider_name")]
        ProviderName,
    
        [EnumLabelValue("region_code")]
        RegionCode,
        
        [EnumLabelValue("region_name")]
        RegionName,
    
        [EnumLabelValue("rsc_region_lead_name")]
        RscRegionName,
    
        [EnumLabelValue("school_urn")]
        SchoolCode,
        
        [EnumLabelValue("school_name")]
        SchoolName,
    
        [EnumLabelValue("sponsor_id")]
        SponsorCode,
        
        [EnumLabelValue("sponsor_name")]
        SponsorName,
    
        [EnumLabelValue("ward_code")]
        WardCode,
        
        [EnumLabelValue("ward_name")]
        WardName,
    
        [EnumLabelValue("planning_area_code")]
        PlanningAreaCode,
        
        [EnumLabelValue("planning_area_name")]
        PlanningAreaName
    }

    private static readonly EnumToEnumLabelConverter<TimeIdentifier> TimeIdentifierLookup = new();
    private static readonly EnumToEnumLabelConverter<GeographicLevel> GeographicLevelLookup = new();
    
    private readonly int _timeIdentifierColumnIndex;
    private readonly int _yearColumnIndex;
    private readonly int _geographicLevelColumnIndex;
    private readonly Dictionary<LocationColumn, int> _locationColumnIndexes;

    public FixedInformationDataFileReader(List<string> csvHeaders)
    {
        _timeIdentifierColumnIndex = csvHeaders.FindIndex(h => h.Equals("time_identifier"));
        _yearColumnIndex = csvHeaders.FindIndex(h => h.Equals("time_period"));
        _geographicLevelColumnIndex = csvHeaders.FindIndex(h => h.Equals("geographic_level"));
        _locationColumnIndexes = EnumUtil.GetEnumValues<LocationColumn>().ToDictionary(
            enumValue => enumValue,
            enumValue => csvHeaders.FindIndex(h => h.Equals(enumValue.GetEnumLabel())));
    }

    public TimeIdentifier GetTimeIdentifier(IReadOnlyList<string> rowValues)
    {
        var value = rowValues[_timeIdentifierColumnIndex];
            
        try
        {
            return (TimeIdentifier) TimeIdentifierLookup.ConvertFromProvider.Invoke(value)!;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new InvalidTimeIdentifierException(value);
        }
    }

    public int GetYear(IReadOnlyList<string> rowValues)
    {
        var year = rowValues[_yearColumnIndex];
            
        if (year == null)
        {
            throw new InvalidTimePeriodException(null);
        }

        return int.Parse(year.Substring(0, 4));
    }
    
    public GeographicLevel GetGeographicLevel(IReadOnlyList<string> rowValues)
    {
        var value = rowValues[_geographicLevelColumnIndex];

        try
        {
            return (GeographicLevel) GeographicLevelLookup.ConvertFromProvider.Invoke(value)!;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new InvalidGeographicLevelException(value);
        }
    }

    public Location GetLocation(IReadOnlyList<string> rowValues)
    {
        return new Location
        {
            GeographicLevel = GetGeographicLevel(rowValues),
            Country = GetCountry(rowValues),
            EnglishDevolvedArea = GetEnglishDevolvedArea(rowValues),
            Institution = GetInstitution(rowValues),
            LocalAuthority = GetLocalAuthority(rowValues),
            LocalAuthorityDistrict = GetLocalAuthorityDistrict(rowValues),
            LocalEnterprisePartnership = GetLocalEnterprisePartnership(rowValues),
            MayoralCombinedAuthority = GetMayoralCombinedAuthority(rowValues),
            MultiAcademyTrust = GetMultiAcademyTrust(rowValues),
            OpportunityArea = GetOpportunityArea(rowValues),
            ParliamentaryConstituency = GetParliamentaryConstituency(rowValues),
            PlanningArea = GetPlanningArea(rowValues),
            Provider = GetProvider(rowValues),
            Region = GetRegion(rowValues),
            RscRegion = GetRscRegion(rowValues),
            School = GetSchool(rowValues),
            Sponsor = GetSponsor(rowValues),
            Ward = GetWard(rowValues)
        };
    }

    private Country? GetCountry(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.CountryCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.CountryName, rowValues);
        return GetLocationAttributeOrDefault(() => new Country(code, name), code, name);
    }

    private EnglishDevolvedArea? GetEnglishDevolvedArea(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.EnglishDevolvedAreaCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.EnglishDevolvedAreaName, rowValues);
        return GetLocationAttributeOrDefault(() => new EnglishDevolvedArea(code, name), code, name);
    }

    private Institution? GetInstitution(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.InstitutionCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.InstitutionName, rowValues);
        return GetLocationAttributeOrDefault(() => new Institution(code, name), code, name);
    }

    private LocalAuthority? GetLocalAuthority(IReadOnlyList<string> rowValues)
    {
        var oldCode = GetLocationAttributeValue(LocationColumn.OldLaCode, rowValues);
        var newCode = GetLocationAttributeValue(LocationColumn.NewLaCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.LaName, rowValues);
        return GetLocationAttributeOrDefault(
            () => new LocalAuthority(newCode, oldCode, name), 
            oldCode, newCode, name);
    }

    private LocalAuthorityDistrict? GetLocalAuthorityDistrict(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.LadCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.LadName, rowValues);
        return GetLocationAttributeOrDefault(() => new LocalAuthorityDistrict(code, name), code, name);
    }

    private LocalEnterprisePartnership? GetLocalEnterprisePartnership(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.LepCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.LepName, rowValues);
        return GetLocationAttributeOrDefault(() => new LocalEnterprisePartnership(code, name), code, name);
    }

    private MayoralCombinedAuthority? GetMayoralCombinedAuthority(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.McaCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.McaName, rowValues);
        return GetLocationAttributeOrDefault(() => new MayoralCombinedAuthority(code, name), code, name);
    }

    private MultiAcademyTrust? GetMultiAcademyTrust(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.MatCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.MatName, rowValues);
        return GetLocationAttributeOrDefault(() => new MultiAcademyTrust(code, name), code, name);
    }

    private OpportunityArea? GetOpportunityArea(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.OpportunityAreaCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.OpportunityAreaName, rowValues);
        return GetLocationAttributeOrDefault(() => new OpportunityArea(code, name), code, name);
    }

    private ParliamentaryConstituency? GetParliamentaryConstituency(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.ParliamentaryConstituencyCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.ParliamentaryConstituencyName, rowValues);
        return GetLocationAttributeOrDefault(() => new ParliamentaryConstituency(code, name), code, name);
    }

    private Provider? GetProvider(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.ProviderCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.ProviderName, rowValues);
        return GetLocationAttributeOrDefault(() => new Provider(code, name), code, name);
    }

    private Region? GetRegion(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.RegionCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.RegionName, rowValues);
        return GetLocationAttributeOrDefault(() => new Region(code, name), code, name);
    }

    private RscRegion? GetRscRegion(IReadOnlyList<string> rowValues)
    {
        var name = GetLocationAttributeValue(LocationColumn.RscRegionName, rowValues);
        return GetLocationAttributeOrDefault(() => new RscRegion(name), name);
    }

    private School? GetSchool(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.SchoolCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.SchoolName, rowValues);
        return GetLocationAttributeOrDefault(() => new School(code, name), code, name);
    }

    private Sponsor? GetSponsor(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.SponsorCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.SponsorName, rowValues);
        return GetLocationAttributeOrDefault(() => new Sponsor(code, name), code, name);
    }

    private Ward? GetWard(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.WardCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.WardName, rowValues);
        return GetLocationAttributeOrDefault(() => new Ward(code, name), code, name);
    }

    private PlanningArea? GetPlanningArea(IReadOnlyList<string> rowValues)
    {
        var code = GetLocationAttributeValue(LocationColumn.PlanningAreaCode, rowValues);
        var name = GetLocationAttributeValue(LocationColumn.PlanningAreaName, rowValues);
        return GetLocationAttributeOrDefault(() => new PlanningArea(code, name), code, name);
    }

    private string? GetLocationAttributeValue(LocationColumn column, IReadOnlyList<string> rowValues)
    {
        var columnIndex = _locationColumnIndexes[column];
        return columnIndex != -1 ? rowValues[columnIndex].Trim() : null;
    }

    private TLocationAttribute? GetLocationAttributeOrDefault<TLocationAttribute>(
        Func<TLocationAttribute> creatorFunc, 
        params string?[] attributeValues)
        where TLocationAttribute : LocationAttribute
    {
        return !attributeValues.All(v => v.IsNullOrEmpty()) ? creatorFunc.Invoke() : default;
    }
}