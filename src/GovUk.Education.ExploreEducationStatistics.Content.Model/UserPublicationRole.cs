namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class UserPublicationRole : ResourceRole<Publication>
{
    public Publication Publication
    {
        get => Resource;
        set => Resource = value;
    }

    public required Guid PublicationId
    {
        get => ResourceId;
        set => ResourceId = value;
    }

    public required PublicationRole Role { get; set; }
}
