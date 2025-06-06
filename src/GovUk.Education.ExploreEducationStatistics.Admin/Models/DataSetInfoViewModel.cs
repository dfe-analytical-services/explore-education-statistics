#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public record DataSetInfoViewModel
{
    public Guid? Id { get; set; }

    public string Extension => Path.GetExtension(DataFileName).TrimStart('.');

    public string? Summary { get; set; }

    public required string DataSetTitle { get; init; } // Name

    public Guid? DataFileId { get; set; }

    public required string DataFileName { get; init; } // FileName

    public required long DataFileSize { get; set; } // Size, make the FE convert this to a string (see File.DisplaySize())

    public int? Rows { get; set; }

    public Guid? MetaFileId { get; set; }

    public required string MetaFileName { get; init; }

    public required long MetaFileSize { get; set; } // Make the FE convert this to a string (see File.DisplaySize())

    //[JsonConverter(typeof(EnumToEnumValueJsonConverter<DataImportStatus>))]
    public DataImportStatus Status { get; set; }

    public DataFilePermissions Permissions { get; set; } = new();

    public Guid? PublicApiDataSetId { get; set; }

    public string? PublicApiDataSetVersion { get; set; }

    public DataSetUploadViewModel? DataSetUpload { get; set; }

    public DateTime? Created { get; set; }

    public string? UserName { get; set; }

    public Guid? ReplacedBy { get; set; }

    //public FileType Type { get; set; } = FileType.Data;
}
