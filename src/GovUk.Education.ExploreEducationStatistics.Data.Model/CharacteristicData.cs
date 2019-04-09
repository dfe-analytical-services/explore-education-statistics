namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class CharacteristicData : TidyData, ICharacteristicData
    {
        public Characteristic Characteristic { get; set; }

        public string CharacteristicName { get; set; }
    }
}