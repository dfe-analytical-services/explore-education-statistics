#nullable enable
using System;
using System.Collections.Generic;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query
{
    public class ObservationQueryContext : IEquatable<ObservationQueryContext>
    {
        public Guid SubjectId { get; set; }

        public List<Guid> LocationIds { get; set; } = new();

        public TimePeriodQuery? TimePeriod { get; set; }

        public IEnumerable<Guid> Filters { get; set; } = new List<Guid>();

        // TODO EES-3328 - remove BoundaryLevel from ObservationQueryContext as we now store it on
        // MapChart configuration instead
        public long? BoundaryLevel { get; set; }

        public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();

        public override string ToString()
        {
            return
                $"{nameof(SubjectId)}: {SubjectId}, " +
                $"{nameof(TimePeriod)}: {TimePeriod}, " +
                $"{nameof(Filters)}: [{(Filters.IsNullOrEmpty() ? string.Empty : Filters.JoinToString(", "))}], " +
                $"{nameof(BoundaryLevel)}: {BoundaryLevel}, " +
                $"{nameof(Indicators)}: [{(Indicators.IsNullOrEmpty() ? string.Empty : Indicators.JoinToString(", "))}], " +
                $"{nameof(LocationIds)}: [{LocationIds.JoinToString(", ")}]";
        }

        public bool Equals(ObservationQueryContext? ctx)
        {
            return ctx?.SubjectId == this.SubjectId
                   && ctx.LocationIds == this.LocationIds
                   && ctx.TimePeriod == this.TimePeriod
                   && ctx.Filters.Equals(this.Filters)
                   && ctx.Indicators.Equals(this.Indicators)
                   && ctx.BoundaryLevel == this.BoundaryLevel;
        }
    }

    public class FullTableQueryRequest
    {
        // @MarkFix check all properties are actually used
        public Guid SubjectId { get; set; }

        public List<Guid> LocationIds { get; set; } = new();

        public TimePeriodQuery? TimePeriod { get; set; }

        public IEnumerable<Guid> Filters { get; set; } = new List<Guid>();

        public long? BoundaryLevel { get; set; } // @MarkFix not needed?

        public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();

        public override string ToString()
        {
            return
                $"{nameof(SubjectId)}: {SubjectId}, " +
                $"{nameof(TimePeriod)}: {TimePeriod}, " +
                $"{nameof(Filters)}: [{(Filters.IsNullOrEmpty() ? string.Empty : Filters.JoinToString(", "))}], " +
                $"{nameof(BoundaryLevel)}: {BoundaryLevel}, " +
                $"{nameof(Indicators)}: [{(Indicators.IsNullOrEmpty() ? string.Empty : Indicators.JoinToString(", "))}], " +
                $"{nameof(LocationIds)}: [{LocationIds.JoinToString(", ")}]";
        }

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

    public class TimePeriodsQueryRequest : ObservationQueryContext
    {
        public class Validator : AbstractValidator<TimePeriodsQueryRequest>
        {
            public Validator()
            {
                RuleFor(context => context.LocationIds)
                    .NotEmpty();
                RuleFor(context => context.TimePeriod)
                    .NotNull();
                RuleFor(context => context.Filters)
                    .Empty();
                RuleFor(context => context.Indicators)
                    .Empty();
            }
        }
    }

    public class LocationsQueryRequest : ObservationQueryContext
    {
        public class Validator : AbstractValidator<LocationsQueryRequest>
        {
            public Validator()
            {
                RuleFor(context => context.LocationIds)
                    .NotEmpty();
                RuleFor(context => context.TimePeriod)
                    .Null();
                RuleFor(context => context.Filters)
                    .Empty();
                RuleFor(context => context.Indicators)
                    .Empty();
            }
        }
    }

    // NOTE: This covers the both queries made after the locations step and the time periods step
    // in the table tool. This is why we don't check TimePeriods in validation, as it may or may not
    // be set.
    public class LocationsOrTimePeriodsQueryRequest
    {
        // @MarkFix check all properties are actually used
        public Guid SubjectId { get; set; }

        public List<Guid> LocationIds { get; set; } = new();

        public TimePeriodQuery? TimePeriod { get; set; }

        //public IEnumerable<Guid> Filters { get; set; } = new List<Guid>();

        //public long? BoundaryLevel { get; set; } // @MarkFix not needed?

        //public IEnumerable<Guid> Indicators { get; set; } = new List<Guid>();

        public override string ToString()
        {
            return
                $"{nameof(SubjectId)}: {SubjectId}, " +
                $"{nameof(TimePeriod)}: {TimePeriod}, " +
                //$"{nameof(Filters)}: [{(Filters.IsNullOrEmpty() ? string.Empty : Filters.JoinToString(", "))}], " +
                //$"{nameof(BoundaryLevel)}: {BoundaryLevel}, " +
                //$"{nameof(Indicators)}: [{(Indicators.IsNullOrEmpty() ? string.Empty : Indicators.JoinToString(", "))}], " +
                $"{nameof(LocationIds)}: [{LocationIds.JoinToString(", ")}]";
        }
        public class Validator : AbstractValidator<LocationsOrTimePeriodsQueryRequest>
        {
            public Validator()
            {
                RuleFor(context => context.LocationIds)
                    .NotEmpty();

                // No TimePeriods check, as it may be null, but also could be set

                //RuleFor(context => context.Filters)
                //    .Empty();
                //RuleFor(context => context.Indicators)
                //    .Empty();
            }
        }
    }

    public static class QueryContextMappingExtensions
    {
        public static ObservationQueryContext AsObservationQueryContext(this FullTableQueryRequest fullTableQueryRequest)
        {
            return new ObservationQueryContext
            {
                SubjectId = fullTableQueryRequest.SubjectId,
                LocationIds = fullTableQueryRequest.LocationIds,
                TimePeriod = fullTableQueryRequest.TimePeriod,
                Filters = fullTableQueryRequest.Filters,
                Indicators = fullTableQueryRequest.Indicators,
                BoundaryLevel = fullTableQueryRequest.BoundaryLevel,
            };
        }

        public static ObservationQueryContext AsObservationQueryContext(this LocationsOrTimePeriodsQueryRequest locationsOrTimePeriodsQueryRequest)
        {
            return new ObservationQueryContext
            {
                SubjectId = locationsOrTimePeriodsQueryRequest.SubjectId,
                LocationIds = locationsOrTimePeriodsQueryRequest.LocationIds,
                TimePeriod = locationsOrTimePeriodsQueryRequest.TimePeriod,
                Filters = [],
                Indicators = [],
                BoundaryLevel = null, // @MarkFix correct?
            };
        }
    }

}
