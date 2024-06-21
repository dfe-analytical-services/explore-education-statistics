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
using Microsoft.Extensions.Configuration;
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
            new ("^(application|text)/csv$"),
            new ("^text/plain$"),
        };

        public static readonly string[] AllowedCsvEncodingTypes = [
            "us-ascii",
            "utf-8",
        ];

        private readonly IConfiguration _configuration;

        public FileTypeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetMimeType(IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var mimeType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_TYPE);

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(file.OpenReadStream());
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeType;
        }

        public async Task<bool> HasMatchingMimeType(IFormFile file, IEnumerable<Regex> mimeTypes)
        {
            await using var stream = file.OpenReadStream();
            var mimeType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_TYPE);

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(file.OpenReadStream());
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }

        public bool HasMatchingEncodingType(IFormFile file, IEnumerable<string> encodingTypes)
        {
            using var stream = file.OpenReadStream();
            var encodingType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_ENCODING);
            return encodingTypes.Contains(encodingType);
        }

        public async Task<bool> HasMatchingMimeType(Stream stream, IEnumerable<Regex> mimeTypes)
        {
            var mimeType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_TYPE);

            if (IsMimeTypeNullOrZip(mimeType))
            {
                var fileType = await GetMimeTypeUsingMimeDetective(stream);
                mimeType = fileType?.Mime ?? mimeType;
            }

            return mimeTypes.Any(pattern => pattern.Match(mimeType).Success);
        }

        public bool HasMatchingEncodingType(Stream stream, IEnumerable<string> encodingTypes)
        {
            var encodingType = GuessMagicInfo(stream, MagicOpenFlags.MAGIC_MIME_ENCODING);
            return encodingTypes.Contains(encodingType);
        }

        public async Task<bool> IsValidCsvFile(Func<Task<Stream>> streamProvider, string filename)
        {
            await using var sampleLinesStream = await GetSampleLinesStream(streamProvider, CsvMimeTypeSampleLineCount);

            if (sampleLinesStream == null)
            {
                return false;
            }
            
            if (!await HasMatchingMimeType(sampleLinesStream, AllowedCsvMimeTypes))
            {
                return false;
            }

            await using var encodingStream = await streamProvider.Invoke();

            if (!HasMatchingEncodingType(encodingStream, AllowedCsvEncodingTypes))
            {
                return false;
            }

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
                        break;
                    }
                    
                    lines.Add(nextLine);
                    linesRead++;
                }
                
            }
            catch (Exception e)
            {
                return null;
            }

            var lineStream = new MemoryStream();
            var writer = new StreamWriter(lineStream);
            await writer.WriteAsync(lines.JoinToString('\n'));
            await writer.FlushAsync();
            lineStream.Position = 0;

            return lineStream;
        }

        private async Task<FileType?> GetMimeTypeUsingMimeDetective(Stream stream)
        {
            // Mime Detective is much better at zip files
            return await stream.GetFileTypeAsync();
        }

        private string GuessMagicInfo(Stream fileStream, MagicOpenFlags flag)
        {
            using var reader = new StreamReader(fileStream);

            var dbPath = _configuration.GetValue<string>("MagicFilePath");
            var magic = new Magic(flag, dbPath);

            var bufferSize = reader.BaseStream.Length >= 1024 ? 1024 : (int) reader.BaseStream.Length;
            return magic.Read(reader.BaseStream, bufferSize);
        }

        private bool IsMimeTypeNullOrZip(string mimeType)
        {
            return mimeType is "application/octet-stream" or "application/zip";
        }
    }
}
