using System.Collections.Generic;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static class FileTypeValidationUtils
{
    public static readonly string[] ZipEncodingTypes = {
        "binary",
    };

    public static readonly Regex[] AllowedAncillaryFileTypes = {
        new(@"^image/.*"),
        new(@"^(application|text)/csv$"),
        new(@"^text/plain$"),
        new(@"^application/pdf$"),
        new(@"^application/msword$"),
        new(@"^application/vnd.ms-excel$"),
        new(@"^application/vnd.openxmlformats(.*)$"),
        new(@"^application/vnd.oasis.opendocument(.*)$"),
        new(@"^application/CDFV2$"),
        new(@"^(application)/zip$"),
        new(@"^(application)/x-compressed$")
    };

    public static readonly Regex[] AllowedChartFileTypes = {
        new(@"^image/.*") 
    };

    public static readonly Regex[] AllowedImageMimeTypes = {
        new(@"^image/.*") 
    };

    public static readonly Regex[] AllowedArchiveMimeTypes = {
        new(@"^(application)/zip$"),
        new(@"^(application)/x-compressed$")
    };
    
    public static readonly Dictionary<FileType, IEnumerable<Regex>> AllowedMimeTypesByFileType = 
        new()
        {
            { Ancillary, AllowedAncillaryFileTypes },
            { Chart, AllowedChartFileTypes },
            { DataZip, AllowedArchiveMimeTypes },
            { Image, AllowedImageMimeTypes }
        };
}
