#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Location
    {
        public Guid Id { get; set; }

        public GeographicLevel GeographicLevel { get; set; }

        public string? Country_Code { get; set; }
        public string? Country_Name { get; set; }

        [NotMapped]
        public Country Country
        {
            get => new(Country_Code, Country_Name);
            set
            {
                Country_Code = value.Code;
                Country_Name = value.Name;
            }
        }

        public string? EnglishDevolvedArea_Code { get; set; }
        public string? EnglishDevolvedArea_Name { get; set; }

        [NotMapped]
        public EnglishDevolvedArea EnglishDevolvedArea
        {
            get => new(EnglishDevolvedArea_Code, EnglishDevolvedArea_Name);
            set
            {
                EnglishDevolvedArea_Code = value.Code;
                EnglishDevolvedArea_Name = value.Name;
            }
        }

        public string? Institution_Code { get; set; }
        public string? Institution_Name { get; set; }

        [NotMapped]
        public Institution Institution
        {
            get => new(Institution_Code, Institution_Name);
            set
            {
                Institution_Code = value.Code;
                Institution_Name = value.Name;
            }
        }

        public string? LocalAuthority_Code { get; set; }
        public string? LocalAuthority_OldCode { get; set; }
        public string? LocalAuthority_Name { get; set; }

        [NotMapped]
        public LocalAuthority LocalAuthority
        {
            get => new(LocalAuthority_Code, LocalAuthority_OldCode, LocalAuthority_Name);
            set
            {
                LocalAuthority_Code = value.Code;
                LocalAuthority_OldCode = value.OldCode;
                LocalAuthority_Name = value.Name;
            }
        }

        public string? LocalAuthorityDistrict_Code { get; set; }
        public string? LocalAuthorityDistrict_Name { get; set; }

        [NotMapped]
        public LocalAuthorityDistrict LocalAuthorityDistrict
        {
            get => new(LocalAuthorityDistrict_Code, LocalAuthorityDistrict_Name);
            set
            {
                LocalAuthorityDistrict_Code = value.Code;
                LocalAuthorityDistrict_Name = value.Name;
            }
        }

        public string? LocalEnterprisePartnership_Code { get; set; }
        public string? LocalEnterprisePartnership_Name { get; set; }

        [NotMapped]
        public LocalEnterprisePartnership LocalEnterprisePartnership
        {
            get => new(LocalEnterprisePartnership_Code, LocalEnterprisePartnership_Name);
            set
            {
                LocalEnterprisePartnership_Code = value.Code;
                LocalEnterprisePartnership_Name = value.Name;
            }
        }

        public string? MayoralCombinedAuthority_Code { get; set; }
        public string? MayoralCombinedAuthority_Name { get; set; }

        [NotMapped]
        public MayoralCombinedAuthority MayoralCombinedAuthority
        {
            get => new(MayoralCombinedAuthority_Code, MayoralCombinedAuthority_Name);
            set
            {
                MayoralCombinedAuthority_Code = value.Code;
                MayoralCombinedAuthority_Name = value.Name;
            }
        }

        public string? MultiAcademyTrust_Code { get; set; }
        public string? MultiAcademyTrust_Name { get; set; }

        [NotMapped]
        public Mat MultiAcademyTrust
        {
            get => new(MultiAcademyTrust_Code, MultiAcademyTrust_Name);
            set
            {
                MultiAcademyTrust_Code = value.Code;
                MultiAcademyTrust_Name = value.Name;
            }
        }

        public string? OpportunityArea_Code { get; set; }
        public string? OpportunityArea_Name { get; set; }

        [NotMapped]
        public OpportunityArea OpportunityArea
        {
            get => new(OpportunityArea_Code, OpportunityArea_Name);
            set
            {
                OpportunityArea_Code = value.Code;
                OpportunityArea_Name = value.Name;
            }
        }

        public string? ParliamentaryConstituency_Code { get; set; }
        public string? ParliamentaryConstituency_Name { get; set; }

        [NotMapped]
        public ParliamentaryConstituency ParliamentaryConstituency
        {
            get => new(ParliamentaryConstituency_Code, ParliamentaryConstituency_Name);
            set
            {
                ParliamentaryConstituency_Code = value.Code;
                ParliamentaryConstituency_Name = value.Name;
            }
        }

        public string? Provider_Code { get; set; }
        public string? Provider_Name { get; set; }

        [NotMapped]
        public Provider Provider
        {
            get => new(Provider_Code, Provider_Name);
            set
            {
                Provider_Code = value.Code;
                Provider_Name = value.Name;
            }
        }

        public string? Region_Code { get; set; }
        public string? Region_Name { get; set; }

        [NotMapped]
        public Region Region
        {
            get => new(Region_Code, Region_Name);
            set
            {
                Region_Code = value.Code;
                Region_Name = value.Name;
            }
        }

        public string? RscRegion_Code { get; set; }

        [NotMapped]
        public RscRegion RscRegion
        {
            get => new(RscRegion_Code);
            set => RscRegion_Code = value.Code;
        }

        public string? School_Code { get; set; }
        public string? School_Name { get; set; }

        [NotMapped]
        public School School
        {
            get => new(School_Code, School_Name);
            set
            {
                School_Code = value.Code;
                School_Name = value.Name;
            }
        }

        public string? Sponsor_Code { get; set; }
        public string? Sponsor_Name { get; set; }

        [NotMapped]
        public Sponsor Sponsor
        {
            get => new(Sponsor_Code, Sponsor_Name);
            set
            {
                Sponsor_Code = value.Code;
                Sponsor_Name = value.Name;
            }
        }

        public string? Ward_Code { get; set; }
        public string? Ward_Name { get; set; }

        [NotMapped]
        public Ward Ward
        {
            get => new(Ward_Code, Ward_Name);
            set
            {
                Ward_Code = value.Code;
                Ward_Name = value.Name;
            }
        }

        public string? PlanningArea_Code { get; set; }
        public string? PlanningArea_Name { get; set; }

        [NotMapped]
        public PlanningArea PlanningArea
        {
            get => new(PlanningArea_Code, PlanningArea_Name);
            set
            {
                PlanningArea_Code = value.Code;
                PlanningArea_Name = value.Name;
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
                   && Ward_Name == other.Ward_Name 
                   && PlanningArea_Code == other.PlanningArea_Code 
                   && PlanningArea_Name == other.PlanningArea_Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Id);
            hashCode.Add((int)GeographicLevel);
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
            hashCode.Add(PlanningArea_Code);
            hashCode.Add(PlanningArea_Name);
            return hashCode.ToHashCode();
        }
    }
}
