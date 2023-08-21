#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public record CommentViewModel
    {
        public Guid Id { get; init; }
        public string Content { get; init; } = string.Empty;
        public DateTime Created { get; init; }
        public User CreatedBy { get; init; } = null!;
        public DateTime? Updated { get; init; }
        public DateTime? Resolved { get; init; }
        public User ResolvedBy { get; init; } = null!;
    }
}
