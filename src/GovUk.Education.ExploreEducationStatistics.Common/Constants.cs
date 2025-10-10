namespace GovUk.Education.ExploreEducationStatistics.Common;

public static class Constants
{
    public static class DataSet
    {
        public const string DataFileExtension = ".csv";
        public const string MetaFileExtension = ".meta.csv";
    }

    public static class ContainerNames
    {
        public const string PrivateReleaseFiles = "releases";
        public const string PrivateReleaseTempFiles = "releases-temp";
        public const string PublicReleaseFiles = "downloads";
        public const string PrivateContent = "cache";
        public const string PublicContent = "cache";
        public const string PermalinkSnapshots = "permalink-snapshots";
        public const string PublisherLeases = "leases";
        public const string PrivateMethodologyFiles = "methodologies";
        public const string PublicMethodologyFiles = "methodologies";
    }

    public static class RegexPatterns
    {
        public const string Guid = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        public const string WildcardDirectoryName = "[^/]+";
    }
}
