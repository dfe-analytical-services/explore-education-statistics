namespace GovUk.Education.ExploreEducationStatistics.Common;

public interface IBlobContainer
{
    public string Name { get; }
    public string EmulatedName { get; }
}

public static class BlobContainers
{
    public static readonly IBlobContainer PrivateReleaseFiles = new BlobContainer(Constants.ContainerNames.PrivateReleaseFiles);
    public static readonly IBlobContainer PrivateReleaseTempFiles = new BlobContainer(Constants.ContainerNames.PrivateReleaseTempFiles);
    public static readonly IBlobContainer PublicReleaseFiles = new BlobContainer(Constants.ContainerNames.PublicReleaseFiles);
    public static readonly IBlobContainer PrivateContent = new PrivateBlobContainer(Constants.ContainerNames.PrivateContent);
    public static readonly IBlobContainer PublicContent = new PublicBlobContainer(Constants.ContainerNames.PublicContent);
    public static readonly IBlobContainer PermalinkSnapshots = new BlobContainer(Constants.ContainerNames.PermalinkSnapshots);
    public static readonly IBlobContainer PublisherLeases = new BlobContainer(Constants.ContainerNames.PublisherLeases);
    public static readonly IBlobContainer PrivateMethodologyFiles = new PrivateBlobContainer(Constants.ContainerNames.PrivateMethodologyFiles);
    public static readonly IBlobContainer PublicMethodologyFiles = new PublicBlobContainer(Constants.ContainerNames.PublicMethodologyFiles);
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
