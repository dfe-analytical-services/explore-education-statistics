using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IFileTypeService
    {
        Task<string> GetMimeType(IFormFile file);
        
        Task<bool> HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes);
        
        bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes);
        
        Task<bool> HasMatchingMimeType(CloudBlob blob, IEnumerable<Regex> mimeTypes);
        
        Task<bool> HasMatchingEncodingType(CloudBlob blob, IEnumerable<string> encodingTypes);
    }
}