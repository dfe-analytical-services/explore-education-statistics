namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;

/// <summary>
/// This default implementation of <see cref="IFileAccessor"/> interacts
/// directly with a standard filesystem, using <see cref="Directory"/>
/// and <see cref="File"/> to perform the work.
/// </summary>
internal class FilesystemFileAccessor : IFileAccessor
{
    public bool DirectoryExists(string directory)
    {
        return Directory.Exists(directory);
    }

    public void CreateDirectory(string directory)
    {
        Directory.CreateDirectory(directory);
    }

    public void DeleteDirectory(string directory)
    {
        Directory.Delete(directory, recursive: true);
    }

    public IList<string> ListFiles(string directory)
    {
        return Directory
            .GetFiles(directory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList(); 
    }

    public void Move(string sourcePath, string destinationPath)
    {
        Directory.Move(sourcePath, destinationPath);
    }
}