using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Builders;

public class FullTableQueryBuilder
{
    public FullTableQuery Build() =>
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
            FilterHierarchiesOptions = new List<FilterHierarchyOptions>
            {
                new()
                {
                    LeafFilterId = Guid.NewGuid(),
                    Options = [[Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]],
                },
                new()
                {
                    LeafFilterId = Guid.NewGuid(),
                    Options = [[Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]],
                }
            }
        };
}