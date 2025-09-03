namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public class PageFeedbackViewModel
{
    public Guid Id { get; set; }

    public DateTime Created { get; set; }

    public required string Url { get; set; }

    public string? UserAgent { get; set; }

    public required string Response { get; set; }

    public string? Context { get; set; }

    public string? Issue { get; set; }

    public string? Intent { get; set; }

    public bool Read { get; set; }
}
