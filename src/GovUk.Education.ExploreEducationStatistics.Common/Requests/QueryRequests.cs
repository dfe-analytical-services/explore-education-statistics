#nullable enable
using System;
using System.Collections.Generic;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Common.Requests;

public record FullTableQueryRequest
{
    public Guid SubjectId { get; set; }

    public List<Guid> LocationIds { get; set; } = new();

    public TimePeriodQuery? TimePeriod { get; set; }

    public IEnumerable<Guid> Filters { get; set; } = new List<Guid>();

    public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();

    public class Validator : AbstractValidator<FullTableQueryRequest>
    {
        public Validator()
        {
            RuleFor(context => context.LocationIds)
                .NotEmpty();
            RuleFor(context => context.TimePeriod)
                .NotNull();

            // NOTE: No rule for filters - a data set might have no filters!

            RuleFor(context => context.Indicators)
                .NotEmpty();
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

    public class Validator : AbstractValidator<LocationsOrTimePeriodsQueryRequest>
    {
        public Validator()
        {
            RuleFor(context => context.LocationIds)
                .NotEmpty();

            // No TimePeriods check, as it may be null, but also could be set
        }
    }
}

public static class QueryContextMappingExtensions
{
    public static ObservationQueryContext AsObservationQueryContext(
        this FullTableQueryRequest fullTableQueryRequest)
    {
        return new ObservationQueryContext
        {
            SubjectId = fullTableQueryRequest.SubjectId,
            LocationIds = fullTableQueryRequest.LocationIds,
            TimePeriod = fullTableQueryRequest.TimePeriod,
            Filters = fullTableQueryRequest.Filters,
            Indicators = fullTableQueryRequest.Indicators,
            BoundaryLevel = null,
        };
    }

    public static ObservationQueryContext AsObservationQueryContext(
        this LocationsOrTimePeriodsQueryRequest locationsOrTimePeriodsQueryRequest)
    {
        return new ObservationQueryContext
        {
            SubjectId = locationsOrTimePeriodsQueryRequest.SubjectId,
            LocationIds = locationsOrTimePeriodsQueryRequest.LocationIds,
            TimePeriod = locationsOrTimePeriodsQueryRequest.TimePeriod,
            Filters = [],
            Indicators = [],
            BoundaryLevel = null,
        };
    }
}
