#nullable enable
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public static class MockFormTestUtils
{
    public static Mock<IFormFile> CreateFormFileMock(string filename, long size = 10240)
    {
        var formFile = new Mock<IFormFile>();

        formFile.SetupGet(f => f.FileName).Returns(filename);

        formFile.SetupGet(f => f.Length).Returns(size);

        return formFile;
    }

    public static Mock<IFormFile> CreateFormFileMock(
        string filename,
        string contentType,
        long size = 10240
    )
    {
        var formFile = new Mock<IFormFile>();

        formFile.SetupGet(f => f.FileName).Returns(filename);

        formFile.SetupGet(f => f.ContentType).Returns(contentType);

        formFile.SetupGet(f => f.Length).Returns(size);

        return formFile;
    }

    public static IFormFile CreateFormFileFromResource(string fileName, string? newFileName = null)
    {
        var filePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources",
            fileName
        );

        return CreateFormFileFromResourceWithPath(filePath, newFileName ?? fileName);
    }

    public static string GetPathForFile(string fileName)
    {
        return Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Resources",
            fileName
        );
    }

    public static IFormFile CreateFormFileFromResourceWithPath(string filePath, string fileName)
    {
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.OpenReadStream()).Returns(() => File.OpenRead(filePath));

        formFile.Setup(f => f.FileName).Returns(() => fileName);

        return formFile.Object;
    }
}
