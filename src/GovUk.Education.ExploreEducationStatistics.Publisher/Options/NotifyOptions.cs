namespace GovUk.Education.ExploreEducationStatistics.Publisher.Options;

public class NotifyOptions
{
    public const string Section = "Notify";

    public required string ApiKey { get; init; }

    public EmailTemplateOptions EmailTemplates { get; init; } = null!;

    public class EmailTemplateOptions
    {
        public string EinTilesRequireUpdateId { get; init; } = null!;
    }
}
