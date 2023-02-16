#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Location
    {
        private static readonly Lazy<List<PropertyInfo>> _locationAttributeProperties = new(
            () => typeof(Location).GetProperties()
                .Where(member => member.PropertyType.IsAssignableTo(typeof(LocationAttribute)))
                .ToList()
        );

        public Guid Id { get; set; }

        public GeographicLevel GeographicLevel { get; set; }

        public string? Country_Code { get; set; }
        public string? Country_Name { get; set; }

        [NotMapped]
        public Country? Country
        {
            get => Country_Code == null && Country_Name == null
                ? null
                : new Country(Country_Code, Country_Name);
            init
            {
                Country_Code = value?.Code;
                Country_Name = value?.Name;
            }
        }

        public string? EnglishDevolvedArea_Code { get; set; }
        public string? EnglishDevolvedArea_Name { get; set; }

        [NotMapped]
        public EnglishDevolvedArea? EnglishDevolvedArea
        {
            get => EnglishDevolvedArea_Code == null && EnglishDevolvedArea_Name == null
                ? null
                : new EnglishDevolvedArea(EnglishDevolvedArea_Code, EnglishDevolvedArea_Name);
            init
            {
                EnglishDevolvedArea_Code = value?.Code;
                EnglishDevolvedArea_Name = value?.Name;
            }
        }

        public string? Institution_Code { get; set; }
        public string? Institution_Name { get; set; }

        [NotMapped]
        public Institution? Institution
        {
            get => Institution_Code == null && Institution_Name == null
                ? null
                : new Institution(Institution_Code, Institution_Name);
            init
            {
                Institution_Code = value?.Code;
                Institution_Name = value?.Name;
            }
        }

        public string? LocalAuthority_Code { get; set; }
        public string? LocalAuthority_OldCode { get; set; }
        public string? LocalAuthority_Name { get; set; }

        [NotMapped]
        public LocalAuthority? LocalAuthority
        {
            get => LocalAuthority_Code == null && LocalAuthority_OldCode == null && LocalAuthority_Name == null
                ? null
                : new LocalAuthority(LocalAuthority_Code, LocalAuthority_OldCode, LocalAuthority_Name);
            init
            {
                LocalAuthority_Code = value?.Code;
                LocalAuthority_OldCode = value?.OldCode;
                LocalAuthority_Name = value?.Name;
            }
        }

        public string? LocalAuthorityDistrict_Code { get; set; }
        public string? LocalAuthorityDistrict_Name { get; set; }

        [NotMapped]
        public LocalAuthorityDistrict? LocalAuthorityDistrict
        {
            get => LocalAuthorityDistrict_Code == null && LocalAuthorityDistrict_Name == null
                ? null
                : new LocalAuthorityDistrict(LocalAuthorityDistrict_Code, LocalAuthorityDistrict_Name);
            init
            {
                LocalAuthorityDistrict_Code = value?.Code;
                LocalAuthorityDistrict_Name = value?.Name;
            }
        }

        public string? LocalEnterprisePartnership_Code { get; set; }
        public string? LocalEnterprisePartnership_Name { get; set; }

        [NotMapped]
        public LocalEnterprisePartnership? LocalEnterprisePartnership
        {
            get => LocalEnterprisePartnership_Code == null && LocalEnterprisePartnership_Name == null
                ? null
                : new LocalEnterprisePartnership(LocalEnterprisePartnership_Code, LocalEnterprisePartnership_Name);
            init
            {
                LocalEnterprisePartnership_Code = value?.Code;
                LocalEnterprisePartnership_Name = value?.Name;
            }
        }

        public string? MayoralCombinedAuthority_Code { get; set; }
        public string? MayoralCombinedAuthority_Name { get; set; }

        [NotMapped]
        public MayoralCombinedAuthority? MayoralCombinedAuthority
        {
            get => MayoralCombinedAuthority_Code == null && MayoralCombinedAuthority_Name == null
                ? null
                : new MayoralCombinedAuthority(MayoralCombinedAuthority_Code, MayoralCombinedAuthority_Name);
            init
            {
                MayoralCombinedAuthority_Code = value?.Code;
                MayoralCombinedAuthority_Name = value?.Name;
            }
        }

        public string? MultiAcademyTrust_Code { get; set; }
        public string? MultiAcademyTrust_Name { get; set; }

        [NotMapped]
        public MultiAcademyTrust? MultiAcademyTrust
        {
            get => MultiAcademyTrust_Code == null && MultiAcademyTrust_Name == null
                ? null
                : new MultiAcademyTrust(MultiAcademyTrust_Code, MultiAcademyTrust_Name);
            init
            {
                MultiAcademyTrust_Code = value?.Code;
                MultiAcademyTrust_Name = value?.Name;
            }
        }

        public string? OpportunityArea_Code { get; set; }
        public string? OpportunityArea_Name { get; set; }

        [NotMapped]
        public OpportunityArea? OpportunityArea
        {
            get => OpportunityArea_Code == null && OpportunityArea_Name == null
                ? null
                : new OpportunityArea(OpportunityArea_Code, OpportunityArea_Name);
            init
            {
                OpportunityArea_Code = value?.Code;
                OpportunityArea_Name = value?.Name;
            }
        }

        public string? ParliamentaryConstituency_Code { get; set; }
        public string? ParliamentaryConstituency_Name { get; set; }

        [NotMapped]
        public ParliamentaryConstituency? ParliamentaryConstituency
        {
            get => ParliamentaryConstituency_Code == null && ParliamentaryConstituency_Name == null
                ? null
                : new ParliamentaryConstituency(ParliamentaryConstituency_Code, ParliamentaryConstituency_Name);
            init
            {
                ParliamentaryConstituency_Code = value?.Code;
                ParliamentaryConstituency_Name = value?.Name;
            }
        }

        public string? PlanningArea_Code { get; set; }
        public string? PlanningArea_Name { get; set; }

        [NotMapped]
        public PlanningArea? PlanningArea
        {
            get => PlanningArea_Code == null && PlanningArea_Name == null
                ? null
                : new PlanningArea(PlanningArea_Code, PlanningArea_Name);
            init
            {
                PlanningArea_Code = value?.Code;
                PlanningArea_Name = value?.Name;
            }
        }

        public string? Provider_Code { get; set; }
        public string? Provider_Name { get; set; }

        [NotMapped]
        public Provider? Provider
        {
            get => Provider_Code == null && Provider_Name == null
                ? null
                : new Provider(Provider_Code, Provider_Name);
            init
            {
                Provider_Code = value?.Code;
                Provider_Name = value?.Name;
            }
        }

        public string? Region_Code { get; set; }
        public string? Region_Name { get; set; }

        [NotMapped]
        public Region? Region
        {
            get => Region_Code == null && Region_Name == null
                ? null
                : new Region(Region_Code, Region_Name);
            init
            {
                Region_Code = value?.Code;
                Region_Name = value?.Name;
            }
        }

        public string? RscRegion_Code { get; set; }

        [NotMapped]
        public RscRegion? RscRegion
        {
            get => RscRegion_Code == null
                ? null
                : new RscRegion(RscRegion_Code);
            init => RscRegion_Code = value?.Code;
        }

        public string? School_Code { get; set; }
        public string? School_Name { get; set; }

        [NotMapped]
        public School? School
        {
            get => School_Code == null && School_Name == null
                ? null
                : new School(School_Code, School_Name);
            init
            {
                School_Code = value?.Code;
                School_Name = value?.Name;
            }
        }

        public string? Sponsor_Code { get; set; }
        public string? Sponsor_Name { get; set; }

        [NotMapped]
        public Sponsor? Sponsor
        {
            get => Sponsor_Code == null && Sponsor_Name == null
                ? null
                : new Sponsor(Sponsor_Code, Sponsor_Name);
            init
            {
                Sponsor_Code = value?.Code;
                Sponsor_Name = value?.Name;
            }
        }

        public string? Ward_Code { get; set; }
        public string? Ward_Name { get; set; }

        [NotMapped]
        public Ward? Ward
        {
            get => Ward_Code == null && Ward_Name == null
                ? null
                : new Ward(Ward_Code, Ward_Name);
            init
            {
                Ward_Code = value?.Code;
                Ward_Name = value?.Name;
            }
        }

        protected bool Equals(Location other)
        {
            return Id.Equals(other.Id)
                   && GeographicLevel == other.GeographicLevel
                   && Country_Code == other.Country_Code
                   && Country_Name == other.Country_Name
                   && EnglishDevolvedArea_Code == other.EnglishDevolvedArea_Code
                   && EnglishDevolvedArea_Name == other.EnglishDevolvedArea_Name
                   && Institution_Code == other.Institution_Code
                   && Institution_Name == other.Institution_Name
                   && LocalAuthority_Code == other.LocalAuthority_Code
                   && LocalAuthority_OldCode == other.LocalAuthority_OldCode
                   && LocalAuthority_Name == other.LocalAuthority_Name
                   && LocalAuthorityDistrict_Code == other.LocalAuthorityDistrict_Code
                   && LocalAuthorityDistrict_Name == other.LocalAuthorityDistrict_Name
                   && LocalEnterprisePartnership_Code == other.LocalEnterprisePartnership_Code
                   && LocalEnterprisePartnership_Name == other.LocalEnterprisePartnership_Name
                   && MayoralCombinedAuthority_Code == other.MayoralCombinedAuthority_Code
                   && MayoralCombinedAuthority_Name == other.MayoralCombinedAuthority_Name
                   && MultiAcademyTrust_Code == other.MultiAcademyTrust_Code
                   && MultiAcademyTrust_Name == other.MultiAcademyTrust_Name
                   && OpportunityArea_Code == other.OpportunityArea_Code
                   && OpportunityArea_Name == other.OpportunityArea_Name
                   && ParliamentaryConstituency_Code == other.ParliamentaryConstituency_Code
                   && ParliamentaryConstituency_Name == other.ParliamentaryConstituency_Name
                   && PlanningArea_Code == other.PlanningArea_Code
                   && PlanningArea_Name == other.PlanningArea_Name
                   && Provider_Code == other.Provider_Code
                   && Provider_Name == other.Provider_Name
                   && Region_Code == other.Region_Code
                   && Region_Name == other.Region_Name
                   && RscRegion_Code == other.RscRegion_Code
                   && School_Code == other.School_Code
                   && School_Name == other.School_Name
                   && Sponsor_Code == other.Sponsor_Code
                   && Sponsor_Name == other.Sponsor_Name
                   && Ward_Code == other.Ward_Code
                   && Ward_Name == other.Ward_Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Id);
            hashCode.Add((int) GeographicLevel);
            hashCode.Add(Country_Code);
            hashCode.Add(Country_Name);
            hashCode.Add(EnglishDevolvedArea_Code);
            hashCode.Add(EnglishDevolvedArea_Name);
            hashCode.Add(Institution_Code);
            hashCode.Add(Institution_Name);
            hashCode.Add(LocalAuthority_Code);
            hashCode.Add(LocalAuthority_OldCode);
            hashCode.Add(LocalAuthority_Name);
            hashCode.Add(LocalAuthorityDistrict_Code);
            hashCode.Add(LocalAuthorityDistrict_Name);
            hashCode.Add(LocalEnterprisePartnership_Code);
            hashCode.Add(LocalEnterprisePartnership_Name);
            hashCode.Add(MayoralCombinedAuthority_Code);
            hashCode.Add(MayoralCombinedAuthority_Name);
            hashCode.Add(MultiAcademyTrust_Code);
            hashCode.Add(MultiAcademyTrust_Name);
            hashCode.Add(OpportunityArea_Code);
            hashCode.Add(OpportunityArea_Name);
            hashCode.Add(ParliamentaryConstituency_Code);
            hashCode.Add(ParliamentaryConstituency_Name);
            hashCode.Add(PlanningArea_Code);
            hashCode.Add(PlanningArea_Name);
            hashCode.Add(Provider_Code);
            hashCode.Add(Provider_Name);
            hashCode.Add(Region_Code);
            hashCode.Add(Region_Name);
            hashCode.Add(RscRegion_Code);
            hashCode.Add(School_Code);
            hashCode.Add(School_Name);
            hashCode.Add(Sponsor_Code);
            hashCode.Add(Sponsor_Name);
            hashCode.Add(Ward_Code);
            hashCode.Add(Ward_Name);
            return hashCode.ToHashCode();
        }

        public IEnumerable<LocationAttribute> GetAttributes()
        {
            return _locationAttributeProperties.Value
                .Select(property => property.GetValue(this))
                .OfType<LocationAttribute?>()
                .WhereNotNull();
        }

        public LocationAttribute ToLocationAttribute()
        {
            LocationAttribute? principalAttribute = GeographicLevel switch
            {
                GeographicLevel.Country => Country,
                GeographicLevel.EnglishDevolvedArea => EnglishDevolvedArea,
                GeographicLevel.LocalAuthority => LocalAuthority,
                GeographicLevel.LocalAuthorityDistrict => LocalAuthorityDistrict,
                GeographicLevel.LocalEnterprisePartnership => LocalEnterprisePartnership,
                GeographicLevel.Institution => Institution,
                GeographicLevel.MayoralCombinedAuthority => MayoralCombinedAuthority,
                GeographicLevel.MultiAcademyTrust => MultiAcademyTrust,
                GeographicLevel.OpportunityArea => OpportunityArea,
                GeographicLevel.ParliamentaryConstituency => ParliamentaryConstituency,
                GeographicLevel.PlanningArea => PlanningArea,
                GeographicLevel.Provider => Provider,
                GeographicLevel.Region => Region,
                GeographicLevel.RscRegion => RscRegion,
                GeographicLevel.School => School,
                GeographicLevel.Sponsor => Sponsor,
                GeographicLevel.Ward => Ward,
                _ => throw new ArgumentOutOfRangeException(nameof(GeographicLevel), GeographicLevel, null)
            };

            return principalAttribute ??
                   throw new InvalidOperationException(
                       $"{nameof(Location)} attribute corresponding with {nameof(GeographicLevel)} '{GeographicLevel}' is null");
        }
    }

    public static class LocationListExtensions
    {
        public static Dictionary<GeographicLevel, List<LocationAttributeNode>> GetLocationAttributesHierarchical(
            this IList<Location> distinctLocations,
            Dictionary<GeographicLevel, List<string>>? hierarchies = null)
        {
            var hierarchyWithLocationSelectors = hierarchies == null
                ? new Dictionary<GeographicLevel, List<Func<Location, LocationAttribute>>>()
                : MapLocationAttributeSelectors(hierarchies);

            return distinctLocations
                .Distinct()
                .GroupBy(location => location.GeographicLevel)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping =>
                    {
                        var geographicLevel = grouping.Key;
                        var locationsForLevel = grouping.ToList();

                        if (hierarchyWithLocationSelectors.ContainsKey(geographicLevel))
                        {
                            return GroupLocationAttributes(locationsForLevel,
                                hierarchyWithLocationSelectors[geographicLevel]);
                        }

                        // No hierarchy configured for level so return flat list
                        return locationsForLevel
                            .Select(CreateLocationLeafNode)
                            .ToList();
                    });
        }

        private static Dictionary<GeographicLevel, List<Func<Location, LocationAttribute>>>
            MapLocationAttributeSelectors(Dictionary<GeographicLevel, List<string>> hierarchies)
        {
            return hierarchies.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Select(propertyName =>
                {
                    // Function which resolves an ILocationAttribute from a Location by property name e.g. 'Region'.
                    // This gets used when grouping the attributes of a location by a property name configured in a hierarchy.
                    return (Func<Location, LocationAttribute>) (location =>
                    {
                        var propertyInfo = typeof(Location).GetProperty(propertyName);

                        if (propertyInfo == null)
                        {
                            throw new ArgumentException($"{nameof(Location)} does not have a property {propertyName}");
                        }

                        // Only allow properties of Location that derive from LocationAttribute to be used in a hierarchy 
                        if (!typeof(LocationAttribute).IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            throw new ArgumentException($"{nameof(Location)} property {propertyName} is not a {nameof(LocationAttribute)}");
                        }

                        var value = propertyInfo.GetValue(location);

                        // A `null` value is possible here as a property configured in a hierarchy
                        // may not be included by all of the data, or might not even be present at all. 
                        // E.g. a Country-Region-LA hierarchy could be defined for LA level but Country and/or Region
                        // can be missing from the location data.

                        // Rather than returning null here, create an ILocationAttribute instance corresponding with
                        // the property type. When building the view model for the hierarchy it's useful to know the
                        // 'level' from the type of ILocationAttribute. This couldn't be determined if the value was null.
                        if (value == null)
                        {
                            var propertyType = propertyInfo.PropertyType;
                            var constructorParams = propertyType.GetConstructors().Single().GetParameters();
                            var nullParams = new object[constructorParams.Length];
                            value = Activator.CreateInstance(propertyType, nullParams);
                        }

                        return (value as LocationAttribute)!;
                    });
                }).ToList()
            );
        }

        private static List<LocationAttributeNode> GroupLocationAttributes(
            IEnumerable<Location> locations,
            IReadOnlyList<Func<Location, LocationAttribute>> attributeSelectors)
        {
            if (attributeSelectors.IsNullOrEmpty())
            {
                return locations.Select(CreateLocationLeafNode).ToList();
            }

            // Recursively GroupBy the Location attributes
            return locations
                .GroupBy(attributeSelectors[0])
                .Select(
                    grouping => new LocationAttributeNode(grouping.Key)
                    {
                        Children = GroupLocationAttributes(grouping, attributeSelectors.Skip(1).ToList())
                    })
                .ToList();
        }

        private static LocationAttributeNode CreateLocationLeafNode(Location location)
        {
            return new(location.ToLocationAttribute())
            {
                LocationId = location.Id
            };
        }
    }
}
