namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public interface ICharacteristicData : ITidyData
    {
        Characteristic Characteristic { get; set; }
    }
}