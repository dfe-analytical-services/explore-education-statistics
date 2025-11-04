#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record CommentViewModel
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime Created { get; init; }
    public UserDetailsViewModel CreatedBy { get; init; } = null!;
    public DateTime? Updated { get; init; }
    public DateTime? Resolved { get; init; }
    public UserDetailsViewModel ResolvedBy { get; init; } = null!;
}
