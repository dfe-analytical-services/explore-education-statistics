namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationOptionMetaLink
{
    public required int MetaId { get; set; }

    public LocationMeta Meta { get; set; } = null!;

    public required int OptionId { get; set; }

    public LocationOptionMeta Option { get; set; } = null!;
}
