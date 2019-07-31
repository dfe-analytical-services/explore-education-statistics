using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public static class FileUtils
    {
        public static async Task<string> UploadToTemporaryFile(IFormFile file)
        {
            var path = Path.GetTempFileName();
            if (file.Length > 0)
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return path;
        }
    }
}