using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.FilterHierarchiesOptionsUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Requests;

public record FullTableQueryRequest
{
    public Guid SubjectId { get; set; }

    public List<Guid> LocationIds { get; set; } = new();

    public TimePeriodQuery? TimePeriod { get; set; }

    public IEnumerable<Guid> Filters { get; set; } = new List<Guid>();

    public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();

    public IDictionary<Guid, List<FilterHierarchyOption>>? FilterHierarchiesOptions { get; set; } = null;

    public FullTableQuery AsFullTableQuery()
    {
        return new FullTableQuery
        {
            SubjectId = this.SubjectId,
            LocationIds = this.LocationIds,
            TimePeriod = this.TimePeriod,
            Filters = this.Filters,
            Indicators = this.Indicators,
            FilterHierarchiesOptions = CreateFilterHierarchiesOptionsFromDictionary(this.FilterHierarchiesOptions),
        };
    }

    public class Validator : AbstractValidator<FullTableQueryRequest>
    {
        public Validator()
        {
            RuleFor(context => context.LocationIds).NotEmpty();
            RuleFor(context => context.TimePeriod).NotNull();

            // NOTE: No rule for filters - a data set might have no filters!
            // NOTE: No rule for filterHierarchiesOptions - a data set might not have a filter hierarchy!

            RuleFor(context => context.Indicators).NotEmpty();
        }
    }
}

// NOTE: This covers the both queries made after the locations step and the time periods step
// in the table tool. This is why we don't check TimePeriods in validation, as it may or may not
// be set.
public class LocationsOrTimePeriodsQueryRequest
{
    public Guid SubjectId { get; set; }

    public List<Guid> LocationIds { get; set; } = new();

    public TimePeriodQuery? TimePeriod { get; set; }

    public FullTableQuery AsFullTableQuery()
    {
        return new FullTableQuery
        {
            SubjectId = this.SubjectId,
            LocationIds = this.LocationIds,
            TimePeriod = this.TimePeriod,
            Filters = [],
            Indicators = [],
            FilterHierarchiesOptions = null,
        };
    }

    public class Validator : AbstractValidator<LocationsOrTimePeriodsQueryRequest>
    {
        public Validator()
        {
            RuleFor(context => context.LocationIds).NotEmpty();

            // No TimePeriods check, as it may be null, but also could be set
        }
    }
}
