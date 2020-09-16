using System.Collections.Generic;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators
{
    public class FileTypeValidationUtils
    {
        public static readonly Regex[] AllowedCsvMimeTypes = {
            new Regex(@"^(application|text)/csv$"),
            new Regex(@"text/plain$")
        };

        public static readonly string[] CsvEncodingTypes = {
            "us-ascii",
            "utf-8"
        };
            
        public static readonly string[] ZipEncodingTypes = {
            "binary",
        };
        
        public static readonly Regex[] AllowedChartFileTypes = {
            new Regex(@"^image/.*") 
        };
        
        public static readonly Regex[] AllowedAncillaryFileTypes = {
            new Regex(@"^image/.*"),
            new Regex(@"^(application|text)/csv$"),
            new Regex(@"^text/plain$"),
            new Regex(@"^application/pdf$"),
            new Regex(@"^application/msword$"),
            new Regex(@"^application/vnd.ms-excel$"),
            new Regex(@"^application/vnd.openxmlformats(.*)$"),
            new Regex(@"^application/vnd.oasis.opendocument(.*)$"),
            new Regex(@"^application/CDFV2$"),
            new Regex(@"^(application)/zip$"),
            new Regex(@"^(application)/x-compressed$")
        };

        public static readonly Regex[] AllowedArchiveMimeTypes = {
            new Regex(@"^(application)/zip$"),
            new Regex(@"^(application)/x-compressed$")
        };
        
        public static readonly Dictionary<ReleaseFileTypes, IEnumerable<Regex>> AllowedMimeTypesByFileType = 
            new Dictionary<ReleaseFileTypes, IEnumerable<Regex>>
            {
                { ReleaseFileTypes.Ancillary, AllowedAncillaryFileTypes },
                { ReleaseFileTypes.Chart, AllowedChartFileTypes },
                { ReleaseFileTypes.Data, AllowedCsvMimeTypes },
                { ReleaseFileTypes.DataZip, AllowedArchiveMimeTypes }
            };
    }
}