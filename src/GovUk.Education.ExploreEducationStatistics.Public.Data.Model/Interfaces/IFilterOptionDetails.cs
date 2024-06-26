namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Interfaces;

public interface IFilterOptionDetails
{
    string PublicId { get; set; }

    string Label { get; set; }

    bool? IsAggregate { get; set; }
}
