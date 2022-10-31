using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IFileTypeService
    {
        Task<bool> HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes);

        bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes);

        Task<bool> IsValidCsvFile(Func<Task<Stream>> streamProvider, string filename);
        
        Task<bool> IsValidCsvFile(IFormFile file);
    }
}