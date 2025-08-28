using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

public interface IFileTypeService
{
    Task<bool> HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes);

    bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes);

    Task<bool> HasCsvFileType(Stream stream);

    Task<bool> HasZipFileType(IFormFile zipFile);
}
