#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    [JsonKnownThisType(nameof(DataBlock))]
    public record DataBlockViewModel : IContentBlockViewModel
    {
        public Guid Id { get; init; }

        public List<CommentViewModel> Comments { get; init; } = new();

        public string Heading { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string? HighlightName { get; init; }

        public string? HighlightDescription { get; init; }

        public string Source { get; init; }  = string.Empty;

        public ObservationQueryContext Query { get; init; } = null!;

        public List<IChart> Charts { get; init; } = new();

        public int Order { get; init; }

        public TableBuilderConfiguration Table { get; init; } = null!;

        public DateTimeOffset? Locked { get; init; }

        public DateTimeOffset? LockedUntil { get; init; }

        public UserDetailsViewModel? LockedBy { get; init; }
    }

    public record DataBlockCreateViewModel
    {
        [Required] public string Heading { get; init; } = string.Empty;

        [Required] public string Name { get; init; } = string.Empty;

        public string? HighlightName { get; init; }

        public string? HighlightDescription { get; init; }

        public string Source { get; init; } = string.Empty;

        public ObservationQueryContext Query { get; init; } = null!;

        public List<IChart> Charts { get; init; } = new();

        public TableBuilderConfiguration Table { get; init; } = null!;
    }

    public record DataBlockUpdateViewModel
    {
        [Required]
        public string Heading { get; init; } = string.Empty;

        [Required]
        public string Name { get; init; } = string.Empty;

        public string? HighlightName { get; init; }

        public string? HighlightDescription { get; init; }

        public string Source { get; init; } = string.Empty;

        public ObservationQueryContext Query { get; init; } = null!;

        public List<IChart> Charts { get; init; } = new();

        public TableBuilderConfiguration Table { get; init; } = null!;
    }
}
