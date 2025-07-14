namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;

/// <summary>
/// This interface represents a component that interacts with a file store
/// comprising directories and files.
///
/// It declares common filesystem interactions, mostly modelled from equivalent
/// calls from <see cref="Directory"/> and <see cref="File"/>.
/// </summary>
public interface IFileAccessor
{
    bool DirectoryExists(string directory);
    
    void CreateDirectory(string directory);
    
    void DeleteDirectory(string directory);
    
    IList<string> ListFiles(string directory);
    
    void Move(string sourcePath, string destinationPath);
}