using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Tests.Services.Workflow.MockBuilders;

public class FileAccessorMockBuilder
{
    private readonly Mock<IFileAccessor> _mock = new(MockBehavior.Strict);

    public FileAccessorMockBuilder()
    {
        Assert = new Asserter(_mock);
    }

    public FileAccessorMockBuilder WhereDirectoryExists(string directory)
    {
        _mock
            .Setup(m => m.DirectoryExists(directory))
            .Returns(true);
        return this;
    }

    public FileAccessorMockBuilder WhereDirectoryDoesNotExist(string directory)
    {
        _mock
            .Setup(m => m.DirectoryExists(directory))
            .Returns(false);
        return this;
    }

    public FileAccessorMockBuilder WhereDirectoryIsCreated(string directory)
    {
        _mock
            .Setup(m => m.CreateDirectory(directory));
        return this;
    }

    public FileAccessorMockBuilder WhereDirectoryIsDeleted(string directory)
    {
        _mock
            .Setup(m => m.DeleteDirectory(directory));
        return this;
    }

    public FileAccessorMockBuilder WhereFileListForDirectoryIs(string directory, IEnumerable<string> files)
    {
        _mock
            .Setup(m => m.ListFiles(directory))
            .Returns(files.ToList);
        return this;
    }

    public FileAccessorMockBuilder WhereFileIsMoved(string sourcePath, string destinationPath)
    {
        _mock
            .Setup(m => m.Move(sourcePath, destinationPath));
        return this;
    }

    public FileAccessorMockBuilder WhereFilesAreMovedBetweenFolders(
        IEnumerable<string> sourceFiles,
        string sourceDirectory,
        string destinationDirectory)
    {
        sourceFiles.ForEach(sourceFile =>
        {
            _mock
                .Setup(m => m.Move(
                    Path.Combine(sourceDirectory, sourceFile),
                    Path.Combine(destinationDirectory, sourceFile)));
        });
        return this;
    }

    public IFileAccessor Build()
    {
        return _mock.Object;
    }

    public Asserter Assert { get; }

    public class Asserter(Mock<IFileAccessor> mock)
    {
        public Asserter DirectoryExistsCalledFor(string directory)
        {
            mock.Verify(m => m.DirectoryExists(directory));
            return this;
        }

        public Asserter CreateDirectoryCalledFor(string directory)
        {
            mock.Verify(m => m.CreateDirectory(directory));
            return this;
        }

        public Asserter DeleteDirectoryCalledFor(string directory)
        {
            mock.Verify(m => m.DeleteDirectory(directory));
            return this;
        }

        public Asserter FileListForDirectoryCalledFor(string directory)
        {
            mock.Verify(m => m.ListFiles(directory));
            return this;
        }

        public Asserter MoveFileCalledFor(string sourcePath, string destinationPath)
        {
            mock.Verify(m => m.Move(sourcePath, destinationPath));
            return this;
        }
        
        public Asserter MoveBetweenFoldersCalledFor(
            IEnumerable<string> sourceFiles,
            string sourceDirectory,
            string destinationDirectory)
        {
            sourceFiles.ForEach(sourceFile =>
            {
                mock.Verify(m => m.Move(
                    Path.Combine(sourceDirectory, sourceFile),
                    Path.Combine(destinationDirectory, sourceFile)));
            });
            return this;
        }
    }
}
