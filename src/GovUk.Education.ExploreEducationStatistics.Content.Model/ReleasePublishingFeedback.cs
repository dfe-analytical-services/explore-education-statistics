#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record ReleasePublishingFeedback : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; set; }

    public required string EmailToken { get; set; }
    
    public required Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public required string PublicationTitle { get; set; }

    public required string ReleaseTitle { get; set; }

    public required PublicationRole UserPublicationRole { get; set; }
    
    public ReleasePublishingFeedbackResponse? Response { get; set; }

    public string? AdditionalFeedback { get; set; }

    public DateTime Created { get; set; }

    public DateTime? FeedbackReceived { get; set; }
}

public enum ReleasePublishingFeedbackResponse
{
    [EnumLabelValue("Extremely satisfied")]
    ExtremelySatisfied,

    [EnumLabelValue("Very satisfied")]
    VerySatisfied,

    [EnumLabelValue("Satisfied")]
    Satisfied,

    [EnumLabelValue("Slightly dissatisfied")]
    SlightlyDissatisfied,

    [EnumLabelValue("Not satisfied at all")]
    NotSatisfiedAtAll
}
