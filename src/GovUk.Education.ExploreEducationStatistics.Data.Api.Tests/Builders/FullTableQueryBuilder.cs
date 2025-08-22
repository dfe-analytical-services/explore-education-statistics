using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;

public class FullTableQueryRequestBuilder
{
    public FullTableQueryRequest Build() =>
        new()
        {
            LocationIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            TimePeriod = new TimePeriodQuery
            {
                StartYear = 2000,
                StartCode = TimeIdentifier.AcademicYear,
                EndYear = 2001,
                EndCode = TimeIdentifier.AcademicYear,
            },
            Indicators = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            Filters = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            SubjectId = Guid.NewGuid(),
            FilterHierarchiesOptions = new Dictionary<Guid, List<FilterHierarchyOption>>
            {
                { Guid.NewGuid(), [new FilterHierarchyOption([Guid.NewGuid(), Guid.NewGuid()])] },
                { Guid.NewGuid(), [new FilterHierarchyOption([Guid.NewGuid(), Guid.NewGuid()])] },
            }
        };
}
