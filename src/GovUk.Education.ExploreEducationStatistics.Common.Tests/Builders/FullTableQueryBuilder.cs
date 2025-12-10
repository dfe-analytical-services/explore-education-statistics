#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Builders;

public class FullTableQueryBuilder
{
    private List<Guid>? _locationIds;
    private int? _endYear;

    public FullTableQuery Build()
    {
        return new()
        {
            LocationIds = _locationIds ?? MockUtils.GenerateGuids(2),
            TimePeriod = new TimePeriodQuery
            {
                StartYear = 2000,
                StartCode = TimeIdentifier.AcademicYear,
                EndYear = _endYear ?? 2001,
                EndCode = TimeIdentifier.AcademicYear,
            },
            Indicators = MockUtils.GenerateGuids(2),
            Filters = MockUtils.GenerateGuids(2),
            SubjectId = Guid.NewGuid(),
            FilterHierarchiesOptions =
            [
                new()
                {
                    LeafFilterId = Guid.NewGuid(),
                    Options = [MockUtils.GenerateGuids(2), MockUtils.GenerateGuids(2)],
                },
                new()
                {
                    LeafFilterId = Guid.NewGuid(),
                    Options = [MockUtils.GenerateGuids(2), MockUtils.GenerateGuids(2)],
                },
            ],
        };
    }

    public FullTableQueryBuilder WithLocationIds(List<Guid> locationIds)
    {
        _locationIds = locationIds;
        return this;
    }

    public FullTableQueryBuilder WithEndYear(int endYear)
    {
        _endYear = endYear;
        return this;
    }

    public FullTableQueryRequest BuildRequest()
    {
        return new()
        {
            LocationIds = _locationIds ?? MockUtils.GenerateGuids(2),
            TimePeriod = new TimePeriodQuery
            {
                StartYear = 2000,
                StartCode = TimeIdentifier.AcademicYear,
                EndYear = 2001,
                EndCode = TimeIdentifier.AcademicYear,
            },
            Indicators = MockUtils.GenerateGuids(2),
            Filters = MockUtils.GenerateGuids(2),
            SubjectId = Guid.NewGuid(),
            FilterHierarchiesOptions = new Dictionary<Guid, List<FilterHierarchyOption>>
            {
                { Guid.NewGuid(), [new FilterHierarchyOption(MockUtils.GenerateGuids(2))] },
                { Guid.NewGuid(), [new FilterHierarchyOption(MockUtils.GenerateGuids(2))] },
            },
        };
    }
}
