using Microsoft.AspNetCore.Http;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public static class MockFormTestUtils
    {
        public static Mock<IFormFile> CreateFormFileMock(string filename)
        {
            var formFile = new Mock<IFormFile>();

            formFile.SetupGet(f => f.FileName)
                .Returns(filename);

            return formFile;
        }
    }
}
