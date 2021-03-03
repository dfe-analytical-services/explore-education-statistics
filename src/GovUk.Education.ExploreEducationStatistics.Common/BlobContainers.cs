namespace GovUk.Education.ExploreEducationStatistics.Common
{
    public interface IBlobContainer
    {
        public string Name { get; }
        public string EmulatedName { get; }
    }

    public static class BlobContainers
    {
        public static readonly BlobContainer PrivateReleaseFiles = new BlobContainer("releases");
        public static readonly BlobContainer PublicReleaseFiles = new BlobContainer("downloads");
        public static readonly BlobContainer PublicContentContainerName = new BlobContainer("cache");
        public static readonly BlobContainer Permalinks = new BlobContainer("permalinks");
        public static readonly BlobContainer PermalinkMigrations = new BlobContainer("permalink-migrations");
        public static readonly BlobContainer PublisherLeases = new BlobContainer("leases");

        // TODO EES-1706 Methodology images - Not used yet but an example of how new containers used in
        // different Azure storage accounts will have the same name but still work with the emulator as different containers
        public static readonly BlobContainer PrivateMethodologyFiles = new PrivateBlobContainer("methodologies");
        public static readonly BlobContainer PublicMethodologyFiles = new PublicBlobContainer("methodologies");
    }

    /// <summary>
    /// Blob container with an immutable name that doesn't change when used with emulator storage
    /// </summary>
    public class BlobContainer : IBlobContainer
    {
        public string Name { get; }

        public BlobContainer(string name)
        {
            Name = name;
        }

        public string EmulatedName => Name;

        public override string ToString() => Name;
    }

    /// <summary>
    /// Blob container with a name prefixed by 'public-' when used with emulator storage
    /// </summary>
    public class PublicBlobContainer : BlobContainer
    {
        public PublicBlobContainer(string name) : base(name)
        {
        }

        public new string EmulatedName => $"public-{Name}";
    }

    /// <summary>
    /// Blob container with a name prefixed by 'private-' when used with emulator storage
    /// </summary>
    public class PrivateBlobContainer : BlobContainer
    {
        public PrivateBlobContainer(string name) : base(name)
        {
        }

        public new string EmulatedName => $"private-{Name}";
    }
}
