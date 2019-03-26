namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class CharacteristicDataLa : CharacteristicData, ICharacteristicGeographicData
    {
        public Region Region { get; set; }

        public string RegionCode { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public string LocalAuthorityCode { get; set; }
    }
}