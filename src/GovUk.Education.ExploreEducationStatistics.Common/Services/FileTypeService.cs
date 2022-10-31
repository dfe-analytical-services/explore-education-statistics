#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MimeDetective.Extensions;
using FileType = MimeDetective.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class FileTypeService : IFileTypeService
    {
        /// <summary>
        /// Number of lines to use as a content sample when validating a CSV file's Mime type.
        /// </summary>
        /// <remarks>
        /// Without limiting this, the entire file contents is used in determining whether or not the given file is
        /// of type application/csv by the Mime library.
        ///</remarks>
        private const int CsvMimeTypeSampleLineCount = 1000;
        
        public static readonly Regex[] AllowedCsvMimeTypes = {
            new Regex(@"^(application|text)/csv$"),
            new Regex(@"^text/plain$")
        };

        public static readonly string[] AllowedCsvEncodingTypes = {
            "us-ascii",
            "utf-8"
        };

        private readonly ILogger<FileTypeService> _logger;

        public FileTypeService(ILogger<FileTypeService> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetMimeType(IFormFile file)
        {
            // The Mime project is generally very good at determining mime types from file contents
            var mimeType = GetMimeTypeUsingMimeProject(file.OpenReadStream());

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(file.OpenReadStream());
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeType;
        }

        public async Task<bool> HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes)
        {
            var mimeType = GetMimeTypeUsingMimeProject(file.OpenReadStream());

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(file.OpenReadStream());
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }

        public bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes)
        {
            var encodingType = GetMimeEncoding(file.OpenReadStream());
            return encodingTypes.Any(pattern => pattern.Equals(encodingType));
        }

        public async Task<bool> HasMatchingMimeType(Stream stream, IEnumerable<Regex> mimeTypes)
        {
            var mimeType = GetMimeTypeUsingMimeProject(stream);

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(stream);
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }

        public bool HasMatchingEncodingType(Stream stream, IEnumerable<string> encodingTypes)
        {
            var encodingType = GetMimeEncoding(stream);
            return encodingTypes.Any(pattern => pattern.Equals(encodingType));
        }

        public async Task<bool> IsValidCsvFile(Func<Task<Stream>> streamProvider, string filename)
        {
            _logger.LogDebug("Validating that {FileName} has a CSV mime type", filename);

            await using var sampleLinesStream = await GetSampleLinesStream(streamProvider, CsvMimeTypeSampleLineCount);

            if (sampleLinesStream == null)
            {
                _logger.LogError("Unable to get sample lines for checking the mime type of {FileName}", filename);
                return false;
            }
            
            if (!await HasMatchingMimeType(sampleLinesStream, AllowedCsvMimeTypes))
            {
                _logger.LogDebug("{FileName} does not have a valid CSV mime type", filename);
                return false;
            }

            _logger.LogDebug("{FileName} has a valid CSV mime type", filename);

            _logger.LogDebug("Validating that {FileName} has a valid CSV character encoding", filename);

            await using var encodingStream = await streamProvider.Invoke();

            if (!HasMatchingEncodingType(encodingStream, AllowedCsvEncodingTypes))
            {
                _logger.LogDebug("{FileName} does not have a valid CSV content encoding", filename);
                return false;
            }

            _logger.LogDebug("{FileName} has a valid CSV content encoding", filename);
            
            return true;
        }

        public Task<bool> IsValidCsvFile(IFormFile file)
        {
            return IsValidCsvFile(() => Task.FromResult(file.OpenReadStream()), file.FileName);
        }

        /// <summary>
        /// Obtain a set of sample lines of the given file, for the purposes of checking the file's mime type and
        /// content encoding.
        /// </summary>
        private async Task<Stream?> GetSampleLinesStream(Func<Task<Stream>> fileStreamProvider, int sampleLineCount)
        {
            using var streamReader = new StreamReader(await fileStreamProvider.Invoke());

            var lines = new List<string>();
            
            try
            {
                var linesRead = 0;

                while (linesRead < sampleLineCount && !streamReader.EndOfStream)
                {
                    var nextLine = await streamReader.ReadLineAsync();

                    if (nextLine == null)
                    {
                        _logger.LogError("Unable to read next sample line {LineNumber} from CSV", linesRead + 1);
                        break;
                    }
                    
                    lines.Add(nextLine);
                    linesRead++;
                }
                
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to read sample lines from CSV - {ErrorMessage}", e.Message);
                return null;
            }

            var lineStream = new MemoryStream();
            var writer = new StreamWriter(lineStream);
            await writer.WriteAsync(lines.JoinToString('\n'));
            await writer.FlushAsync();
            lineStream.Position = 0;

            return lineStream;
        }

        private string GetMimeTypeUsingMimeProject(Stream stream)
        {
            // The Mime project is generally very good at determining mime types from file contents
            return GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_TYPE);
        }

        private async Task<FileType?> GetMimeTypeUsingMimeDetective(Stream stream)
        {
            // Mime Detective is much better at zip files
            return await stream.GetFileTypeAsync();
        }

        private string GetMimeEncoding(Stream stream)
        {
            return GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_ENCODING);
        }

        private string GuessMagicInfo(Stream fileStream, MagicOpenFlags flag)
        {
            using var reader = new StreamReader(fileStream);
            var magic = new Magic(flag);
            var bufferSize = reader.BaseStream.Length >= 1024 ? 1024 : (int) reader.BaseStream.Length;
            return magic.Read(reader.BaseStream, bufferSize);
        }

        private bool IsMimeTypeNullOrZip(string mimeType)
        {
            return mimeType == null ||
                   mimeType == "application/octet-stream" ||
                   mimeType == "application/zip";
        }
    }
}
