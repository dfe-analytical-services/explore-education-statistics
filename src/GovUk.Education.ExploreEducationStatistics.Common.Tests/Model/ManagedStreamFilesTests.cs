using System.IO.Compression;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Model;

public abstract class ManagedStreamFilesTests
{
    public class ManagedStreamFormFileTests : ManagedStreamFilesTests
    {
        [Fact]
        public async Task GivenAFormFile_WhenGettingBasicDetails_ReturnsBasicDetails()
        {
            var formFile = MockFile("data-file.csv");

            var managedFile = new ManagedStreamFormFile(MockFile("data-file.csv"));
            await using var stream = managedFile.GetStream();
            
            Assert.Equal(formFile.FileName, managedFile.Name);
            Assert.Equal(stream.Length, managedFile.Length);
        }
        
        [Fact]
        public async Task GivenNoStreamIsOpened_WhenDisposeIsCalled_IsOk()
        {
            var managedFile = new ManagedStreamFormFile(MockFile("data-file.csv"));
            await managedFile.DisposeAsync();
        }
        
        [Fact]
        public async Task GivenFormFile_WhenDisposeIsCalled_BackingFormFileStreamIsDisposed()
        {
            var formFile = MockFile("data-file.csv");
            var formFileStream = formFile.OpenReadStream();
            
            var managedFile = new ManagedStreamFormFile(formFile);
            
            AssertStreamNotDisposed(formFileStream);
            await managedFile.DisposeAsync();
            AssertStreamDisposed(formFileStream);
        }
        
        [Fact]
        public async Task GivenStreamIsOpened_WhenDisposeIsCalled_IsDisposed()
        {
            var managedFile = new ManagedStreamFormFile(MockFile("data-file.csv"));
            var stream = managedFile.GetStream();

            AssertStreamNotDisposed(stream);
            await managedFile.DisposeAsync();
            AssertStreamDisposed(stream);
        }
        
        [Fact]
        public async Task GivenStreamIsOpened_WhenCalledAgain_ReturnsSameStream()
        {
            var managedFile = new ManagedStreamFormFile(MockFile("data-file.csv"));
            await using var stream1 = managedFile.GetStream();
            await using var stream2 = managedFile.GetStream();
            Assert.Same(stream1, stream2);
        }
        
        private static IFormFile MockFile(string fileName)
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Strict);
            var stream = "test content".ToStream();
            
            fileMock.Setup(formFile => formFile.OpenReadStream()).Returns(stream);
            fileMock.Setup(formFile => formFile.FileName).Returns(fileName);
            fileMock.Setup(formFile => formFile.Name).Returns(fileName);
            fileMock.Setup(formFile => formFile.Length).Returns(stream.Length);
            return fileMock.Object;
        }
    }

    public class ManagedStreamZipFormFileTests : ManagedStreamFilesTests
    {
        [Fact]
        public async Task GivenAZipFormFile_WhenGettingEntries_EntriesAreReturnedWithBasicDetails()
        {
            var formFile = MockFile("data-file.zip", numberOfEntries: 2);
            await using var managedFile = new ManagedStreamZipFormFile(formFile);

            var entries = managedFile.GetEntries();
            Assert.Equal(2, entries.Count);
            
            Assert.Equal("entry-1.csv", entries[0].Name);
            Assert.Equal("test content 1".Length, entries[0].Length);

            Assert.Equal("entry-2.csv", entries[1].Name);
            Assert.Equal("test content 2".Length, entries[1].Length);
        }
        
        [Fact]
        public async Task GivenNoStreamIsOpened_WhenDisposeIsCalled_IsOk()
        {
            var managedFile = new ManagedStreamZipFormFile(MockFile("data-file.csv"));
            await managedFile.DisposeAsync();
        }
        
        [Fact]
        public async Task GivenZipFile_WhenDisposeIsCalled_BackingArchiveStreamIsDisposed()
        {
            var formFile = MockFile("data-file.csv");
            var formFileStream = formFile.OpenReadStream();
            
            var managedFile = new ManagedStreamZipFormFile(formFile);
            
            AssertStreamNotDisposed(formFileStream);
            await managedFile.DisposeAsync();
            AssertStreamDisposed(formFileStream);
        }

        [Fact]
        public async Task GivenZipEntryStreamsAreOpened_WhenCalledMultipleTimes_AreDifferent()
        {
            var managedFile = new ManagedStreamZipFormFile(MockFile("data-file.csv"));
            var entry = managedFile.GetEntries().Single();
            await using var entryStream1 = entry.GetStream();
            await using var entryStream2 = entry.GetStream();
            Assert.NotSame(entryStream1, entryStream2);
        }
        
        [Fact]
        public async Task GivenZipEntryStreamsAreOpened_WhenCalledMultipleTimesForSameEntry_AreAllDisposedCorrectly()
        {
            var managedFile = new ManagedStreamZipFormFile(MockFile("data-file.csv"));
            var entry = managedFile.GetEntries().Single();
            var entryStream1 = entry.GetStream();
            var entryStream2 = entry.GetStream();
            
            AssertStreamNotDisposed(entryStream1);
            AssertStreamNotDisposed(entryStream2);
            await managedFile.DisposeAsync();
            AssertStreamDisposed(entryStream1);
            AssertStreamDisposed(entryStream2);
        }
        
        [Fact]
        public async Task GivenZipEntryStreamsAreOpened_WhenCalledMultipleTimesForDifferentEntries_AreDisposed()
        {
            var managedFile = new ManagedStreamZipFormFile(MockFile("data-file.csv", numberOfEntries: 2));
            var entry1 = managedFile.GetEntries().First();
            var entry2 = managedFile.GetEntries().Last();
            var entry1Stream = entry1.GetStream();
            var entry2Stream = entry2.GetStream();
            
            AssertStreamNotDisposed(entry1Stream);
            AssertStreamNotDisposed(entry2Stream);
            await managedFile.DisposeAsync();
            AssertStreamDisposed(entry1Stream);
            AssertStreamDisposed(entry2Stream);
        }
        
        private static IFormFile MockFile(string fileName, int numberOfEntries = 1)
        {
            var fileMock = new Mock<IFormFile>(MockBehavior.Strict);
        
            var stream = new MemoryStream();

            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true);

            for (var i = 0; i < numberOfEntries; i++)
            {
                var entry = archive.CreateEntry($"entry-{i + 1}.csv");
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream);
                writer.Write($"test content {i + 1}");    
            }
            
            fileMock.Setup(formFile => formFile.OpenReadStream()).Returns(stream);
            fileMock.Setup(formFile => formFile.FileName).Returns(fileName);
            fileMock.Setup(formFile => formFile.Name).Returns(fileName);
            fileMock.Setup(formFile => formFile.Length).Returns(stream.Length);
            return fileMock.Object;
        }
    }

    private static int ReadFromStream(Stream stream)
    {
        return stream.Read([], 0, 0);
    }

    private static void AssertStreamDisposed(Stream stream)
    {
        Assert.Throws<ObjectDisposedException>(() => ReadFromStream(stream));
    }

    private static void AssertStreamNotDisposed(Stream stream)
    {
        ReadFromStream(stream);
    }
}
