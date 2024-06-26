using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Interfaces;

public interface IIndicatorDetails
{
    string PublicId { get; set; }

    string Label { get; set; }

    IndicatorUnit? Unit { get; set; }

    byte? DecimalPlaces { get; set; }
}
