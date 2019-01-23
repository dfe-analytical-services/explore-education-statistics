namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public interface ITidyDataCharacteristic : ITidyData
    {
        Characteristic Characteristic { get; set; }
    }
}