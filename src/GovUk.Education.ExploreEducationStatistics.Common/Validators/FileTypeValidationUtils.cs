using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators
{
    public static class FileTypeValidationUtils
    {
        public static readonly Regex[] AllowedCsvMimeTypes = {
            new ("^(application|text)/csv$"),
            new ("^text/plain$")
        };

        public static readonly string[] AllowedCsvEncodingTypes = [
            "us-ascii",
            "utf-8"
        ];

        public static readonly string[] AllowedArchiveEncodingTypes =
        [
            "binary"
        ];

        public static readonly Regex[] AllowedAncillaryFileTypes =
        [
            new ("^image/.*"),
            new ("^(application|text)/csv$"),
            new ("^text/plain$"),
            new ("^application/pdf$"),
            new ("^application/msword$"),
            new ("^application/vnd.ms-excel$"),
            new ("^application/vnd.openxmlformats(.*)$"),
            new ("^application/vnd.oasis.opendocument(.*)$"),
            new ("^application/CDFV2$"),
            new ("^(application)/zip$"),
            new ("^(application)/x-compressed$")
        ];

        public static readonly Regex[] AllowedChartFileTypes =
        [
            new ("^image/.*")
        ];

        public static readonly Regex[] AllowedImageMimeTypes =
        [
            new ("^image/.*")
        ];

        public static readonly Regex[] AllowedArchiveMimeTypes =
        [
            new ("^(application)/zip$"),
            new ("^(application)/x-compressed$")
        ];

        public static readonly Dictionary<FileType, IEnumerable<Regex>> AllowedMimeTypesByFileType =
            new()
            {
                { Ancillary, AllowedAncillaryFileTypes },
                { Chart, AllowedChartFileTypes },
                { Data, AllowedCsvMimeTypes },
                { Metadata, AllowedCsvMimeTypes },
                { DataZip, AllowedArchiveMimeTypes },
                { BulkDataZip, AllowedArchiveMimeTypes },
                { Image, AllowedImageMimeTypes }
            };
    }
}
