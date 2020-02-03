using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IFileTypeService
    {
        string GetMimeType(IFormFile file);

        string GetMimeEncoding(IFormFile file);

        bool HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes);
    }
}