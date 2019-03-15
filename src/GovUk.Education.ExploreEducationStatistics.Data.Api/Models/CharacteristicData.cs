namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public abstract class CharacteristicData : TidyData, ICharacteristicData
    {
        public Characteristic Characteristic { get; set; }

        public string CharacteristicName { get; set; }
    }
}