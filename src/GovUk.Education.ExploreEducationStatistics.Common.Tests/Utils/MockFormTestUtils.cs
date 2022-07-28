#nullable enable
using Microsoft.AspNetCore.Http;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public static class MockFormTestUtils
    {
        public static Mock<IFormFile> CreateFormFileMock(string filename, long size = 10240)
        {
            var formFile = new Mock<IFormFile>();

            formFile.SetupGet(f => f.FileName)
                .Returns(filename);

            formFile.SetupGet(f => f.Length)
                .Returns(size);

            return formFile;
        }

        public static Mock<IFormFile> CreateFormFileMock(string filename, string contentType, long size = 10240)
        {
            var formFile = new Mock<IFormFile>();

            formFile.SetupGet(f => f.FileName)
                .Returns(filename);

            formFile.SetupGet(f => f.ContentType)
                .Returns(contentType);

            formFile.SetupGet(f => f.Length)
                .Returns(size);

            return formFile;
        }
    }
}
