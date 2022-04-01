namespace GovUk.Education.ExploreEducationStatistics.Common
{
    public interface IBlobContainer
    {
        public string Name { get; }
        public string EmulatedName { get; }
    }

    public static class BlobContainers
    {
        public static readonly IBlobContainer PrivateReleaseFiles = new BlobContainer("releases");
        public static readonly IBlobContainer PublicReleaseFiles = new BlobContainer("downloads");
        public static readonly IBlobContainer PrivateContent = new PrivateBlobContainer("cache");
        public static readonly IBlobContainer PublicContent = new PublicBlobContainer("cache");
        public static readonly IBlobContainer Permalinks = new BlobContainer("permalinks");
        public static readonly IBlobContainer PublisherLeases = new BlobContainer("leases");
        public static readonly IBlobContainer PrivateMethodologyFiles = new PrivateBlobContainer("methodologies");
        public static readonly IBlobContainer PublicMethodologyFiles = new PublicBlobContainer("methodologies");
    }

    /// <summary>
    /// Blob container with an immutable name that doesn't change when used with emulator storage
    /// </summary>
    public record BlobContainer : IBlobContainer
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
    public record PublicBlobContainer : IBlobContainer
    {
        public string Name { get; }

        public PublicBlobContainer(string name)
        {
            Name = name;
        }

        public string EmulatedName => $"public-{Name}";

        public override string ToString() => Name;
    }

    /// <summary>
    /// Blob container with a name prefixed by 'private-' when used with emulator storage
    /// </summary>
    public record PrivateBlobContainer : IBlobContainer
    {
        public string Name { get; }

        public PrivateBlobContainer(string name)
        {
            Name = name;
        }

        public string EmulatedName => $"private-{Name}";

        public override string ToString() => Name;
    }
}
