using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public static partial class FileTypeValidationUtils
{
    [GeneratedRegex("^(application|text)/csv$")] private static partial Regex CsvPattern();
    [GeneratedRegex("^text/plain$")] private static partial Regex PlainPattern();
    [GeneratedRegex("^application/pdf$")] private static partial Regex PdfPattern();
    [GeneratedRegex("^application/msword$")] private static partial Regex MsWordPattern();
    [GeneratedRegex("^application/vnd.ms-excel$")] private static partial Regex MsExcelPattern();
    [GeneratedRegex("^application/vnd.openxmlformats(.*)$")] private static partial Regex OpenXmlPattern();
    [GeneratedRegex("^application/vnd.oasis.opendocument(.*)$")] private static partial Regex OpenDocumentPattern();
    [GeneratedRegex("^application/CDFV2$")] private static partial Regex CompositeDocumentPattern();
    [GeneratedRegex("^image/.*")] private static partial Regex ImagePattern();
    [GeneratedRegex("^(application)/zip$")] private static partial Regex ZipPattern();
    [GeneratedRegex("^(application)/x-compressed$")] private static partial Regex CompressedPattern();

    public static readonly Regex[] AllowedCsvMimeTypes = {
        CsvPattern(),
        PlainPattern(),
    };

    public static readonly string[] AllowedCsvEncodingTypes = [
        "us-ascii",
        "utf-8"
    ];

    public static readonly Regex[] AllowedZipFileMimeTypes =
    [
        ZipPattern(),
        CompressedPattern(),
    ];

    public static readonly string[] AllowedZipFileEncodingTypes =
    [
        "binary"
    ];

    public static readonly Regex[] AllowedAncillaryFileTypes =
    [
        ImagePattern(),
        CsvPattern(),
        PlainPattern(),
        PdfPattern(),
        MsWordPattern(),
        MsExcelPattern(),
        OpenXmlPattern(),
        OpenDocumentPattern(),
        CompositeDocumentPattern(),
        ZipPattern(),
        CompressedPattern(),
    ];

    public static readonly Regex[] AllowedChartFileTypes =
    [
        ImagePattern(),
    ];

    public static readonly Regex[] AllowedImageMimeTypes =
    [
        ImagePattern(),
    ];

    public static ImmutableDictionary<FileType, Regex[]> AllowedMimeTypesByFileType
        => ImmutableDictionary.CreateRange(
            new KeyValuePair<FileType, Regex[]>[] {
                KeyValuePair.Create(Ancillary, AllowedAncillaryFileTypes),
                KeyValuePair.Create(Chart, AllowedChartFileTypes),
                KeyValuePair.Create(Data, AllowedCsvMimeTypes),
                KeyValuePair.Create(Metadata, AllowedCsvMimeTypes),
                KeyValuePair.Create(DataZip, AllowedZipFileMimeTypes),
                KeyValuePair.Create(BulkDataZip, AllowedZipFileMimeTypes),
                KeyValuePair.Create(Image, AllowedImageMimeTypes),
            });

    public static bool IsAllowedCsvMimeType(string mimeType)
        => AllowedCsvMimeTypes.Any(pattern => pattern.Match(mimeType).Success);

    public static bool IsAllowedCsvEncodingType(string encodingType)
        => AllowedCsvEncodingTypes.Contains(encodingType);

    public static bool IsAllowedMimeTypeForFileType(
        string mimeType,
        FileType type) => type switch
        {
            Ancillary => AllowedAncillaryFileTypes.Any(pattern => pattern.Match(mimeType).Success),
            Chart => AllowedChartFileTypes.Any(pattern => pattern.Match(mimeType).Success),
            Data => IsAllowedCsvMimeType(mimeType),
            Metadata => IsAllowedCsvMimeType(mimeType),
            DataZip => AllowedZipFileMimeTypes.Any(pattern => pattern.Match(mimeType).Success),
            BulkDataZip => AllowedZipFileMimeTypes.Any(pattern => pattern.Match(mimeType).Success),
            Image => AllowedImageMimeTypes.Any(pattern => pattern.Match(mimeType).Success),
            _ => false
        };
}
