#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

// TODO EES-7584 Remove once every DataSetFileVersionGeographicLevel.CsvOnly has been set
[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class DataSetFileVersionGeographicLevelsMigrationController(
    ContentDbContext contentDbContext,
    IPrivateBlobStorageService privateBlobStorageService,
    ILogger<DataSetFileVersionGeographicLevelsMigrationController> logger
) : ControllerBase
{
    private const string GeographicLevelColumn = "geographic_level";

    public class MigrationResult
    {
        public bool IsDryRun { get; set; }

        public int Processed { get; set; }

        public int Remaining { get; set; }

        public Dictionary<Guid, List<string>> Added { get; set; } = new();

        public List<string> Errors { get; set; } = [];
    }

    [HttpPut("migrate-csv-only-geographic-levels")]
    public async Task<MigrationResult> MigrateCsvOnlyGeographicLevels(
        [FromQuery] bool dryRun = true,
        [FromQuery] int num = 20,
        [FromQuery] int skip = 0,
        CancellationToken cancellationToken = default
    )
    {
        var files = await contentDbContext
            .Files.Include(f => f.DataSetFileVersionGeographicLevels)
            .Where(f => f.DataSetFileVersionGeographicLevels.Any(gl => gl.CsvOnly == null))
            .OrderBy(f => f.Id)
            .Skip(skip)
            .Take(num)
            .ToListAsync(cancellationToken);

        var result = new MigrationResult { IsDryRun = dryRun };

        foreach (var file in files)
        {
            var csvGeographicLevels = await GetCsvGeographicLevels(file, result.Errors, cancellationToken);

            if (csvGeographicLevels == null)
            {
                continue;
            }

            var addedGeographicLevels = SetCsvOnly(file, csvGeographicLevels, result.Errors);

            if (addedGeographicLevels.Count > 0)
            {
                result.Added.Add(file.Id, addedGeographicLevels.Select(gl => gl.GetEnumLabel()).ToList());
            }

            result.Processed++;
        }

        if (!dryRun)
        {
            await contentDbContext.SaveChangesAsync(cancellationToken);
        }

        result.Remaining = await contentDbContext.Files.CountAsync(
            f => f.DataSetFileVersionGeographicLevels.Any(gl => gl.CsvOnly == null),
            cancellationToken
        );

        return result;
    }

    private async Task<HashSet<GeographicLevel>?> GetCsvGeographicLevels(
        File file,
        List<string> errors,
        CancellationToken cancellationToken
    )
    {
        var streamProvider = () => GetDataFileStream(file, cancellationToken);
        var csvGeographicLevelLabels = new HashSet<string>();

        try
        {
            var csvHeaders = await CsvUtils.GetCsvHeaders(streamProvider);
            var geographicLevelColumnIndex = csvHeaders.FindIndex(header => header.Equals(GeographicLevelColumn));

            if (geographicLevelColumnIndex == -1)
            {
                AddError(errors, file, $"CSV has no {GeographicLevelColumn} column");
                return null;
            }

            await CsvUtils.ForEachRow(
                streamProvider,
                (rowValues, _, _) =>
                {
                    csvGeographicLevelLabels.Add(rowValues[geographicLevelColumnIndex]);
                    return Task.FromResult(true);
                }
            );

            return csvGeographicLevelLabels.Select(EnumToEnumLabelConverter<GeographicLevel>.FromProvider).ToHashSet();
        }
        catch (Exception e)
        {
            AddError(errors, file, $"could not read the CSV: {e.Message}");
            return null;
        }
    }

    private async Task<Stream> GetDataFileStream(File file, CancellationToken cancellationToken)
    {
        var stream = await privateBlobStorageService.GetDownloadStream(
            PrivateReleaseFiles,
            file.Path(),
            cancellationToken: cancellationToken
        );

        return stream.IsLeft
            ? throw new InvalidOperationException($"no blob found for File {file.Id} at {file.Path()}")
            : stream.Right;
    }

    private List<GeographicLevel> SetCsvOnly(
        File file,
        HashSet<GeographicLevel> csvGeographicLevels,
        List<string> errors
    )
    {
        foreach (var geographicLevel in file.DataSetFileVersionGeographicLevels)
        {
            if (csvGeographicLevels.Contains(geographicLevel.GeographicLevel))
            {
                geographicLevel.CsvOnly = false;
            }
            else
            {
                // Every recorded geographic level was imported from a row of this CSV, so this suggests
                // the file has been replaced, or that we are reading the wrong blob. Leave the row unset
                // so that the data set is revisited.
                AddError(
                    errors,
                    file,
                    $"geographic level {geographicLevel.GeographicLevel.GetEnumLabel()} is recorded but is not in the CSV"
                );
            }
        }

        var csvOnlyGeographicLevels = csvGeographicLevels
            .Except(file.DataSetFileVersionGeographicLevels.Select(gl => gl.GeographicLevel))
            .OrderBy(gl => gl)
            .ToList();

        contentDbContext.DataSetFileVersionGeographicLevels.AddRange(
            csvOnlyGeographicLevels.Select(gl => new DataSetFileVersionGeographicLevel
            {
                DataSetFileVersionId = file.Id,
                GeographicLevel = gl,
                CsvOnly = true,
            })
        );

        return csvOnlyGeographicLevels;
    }

    private void AddError(List<string> errors, File file, string message)
    {
        var error = $"File {file.Id} ({file.Filename}): {message}";
        logger.LogError("{Error}", error);
        errors.Add(error);
    }
}
