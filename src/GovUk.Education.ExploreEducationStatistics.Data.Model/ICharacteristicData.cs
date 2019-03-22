namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public interface ICharacteristicData : ITidyData
    {
        Characteristic Characteristic { get; set; }
        
        string CharacteristicName { get; set; }
    }
}