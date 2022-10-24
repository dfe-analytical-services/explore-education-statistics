using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IFileTypeService
    {
        Task<string> GetMimeType(IFormFile file);

        Task<bool> HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes);

        bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes);

        Task<bool> HasMatchingMimeType(Stream stream, IEnumerable<Regex> mimeTypes);

        bool HasMatchingEncodingType(Stream stream, IEnumerable<string> encodingTypes);

        Task<bool> IsValidCsvDataOrMetaFile(
            Func<Task<Stream>> streamProvider,
            string filename);
        
        Task<bool> IsValidCsvDataOrMetaFile(IFormFile file);
    }
}