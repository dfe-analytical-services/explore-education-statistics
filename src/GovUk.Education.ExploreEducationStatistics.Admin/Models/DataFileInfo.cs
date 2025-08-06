#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public record DataFileInfo : FileInfo
{
    public override FileType Type { get; set; } = FileType.Data;

    public Guid? MetaFileId { get; set; }

    public string MetaFileName { get; set; } = string.Empty;

    public int? Rows { get; set; }

    public Guid? ReplacedBy { get; set; }

    public ReplacementDataFileInfo? ReplacedByDataFile { get; set; }

    [JsonConverter(typeof(EnumToEnumValueJsonConverter<DataImportStatus>))]
    public DataImportStatus Status { get; set; }

    public DataFilePermissions Permissions { get; set; } = new();

    public Guid? PublicApiDataSetId { get; set; }

    public string? PublicApiDataSetVersion { get; set; }

    public DataFileInfo() { }

    [SetsRequiredMembers]
    public DataFileInfo(
        ReleaseFile releaseFile,
        DataImport dataImport,
        DataFilePermissions permissions)
    {
        Id = releaseFile.FileId;
        FileName = releaseFile.File.Filename;
        Name = releaseFile.Name ?? "Unknown";
        Size = releaseFile.File.DisplaySize();
        MetaFileId = dataImport.MetaFile.Id;
        MetaFileName = dataImport.MetaFile.Filename;
        ReplacedBy = releaseFile.File.ReplacedById;
        ReplacedByDataFile = null;
        Rows = dataImport.TotalRows;
        UserName = releaseFile.File.CreatedBy?.Email ?? "";
        Status = dataImport.Status;
        Created = releaseFile.File.Created;
        Permissions = permissions;
        PublicApiDataSetId = releaseFile.PublicApiDataSetId;
        PublicApiDataSetVersion = releaseFile.PublicApiDataSetVersionString;
    }
}

public record ReplacementDataFileInfo : DataFileInfo
{
    public bool HasValidReplacementPlan { get; set; }

    [SetsRequiredMembers]
    public ReplacementDataFileInfo(
        ReleaseFile releaseFile,
        DataImport dataImport,
        DataFilePermissions permissions,
        bool hasValidReplacementPlan) : base(releaseFile, dataImport, permissions)
    {
        HasValidReplacementPlan = hasValidReplacementPlan;
    }
}
