#nullable enable
using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class BoundaryLevelService(
    IBoundaryLevelRepository boundaryLevelRepository,
    StatisticsDbContext context) : IBoundaryLevelService
{
    public async Task<Either<ActionResult, List<BoundaryLevel>>> ListBoundaryLevels(
        CancellationToken cancellationToken = default)
    {
        return await context.BoundaryLevel.ToListAsync(cancellationToken);
    }

    public async Task<Either<ActionResult, BoundaryLevel>> GetBoundaryLevel(
        long id, CancellationToken cancellationToken = default)
    {
        return await context.BoundaryLevel.FirstOrNotFoundAsync(level => level.Id == id, cancellationToken);
    }

    public async Task<Either<ActionResult, BoundaryLevel>> CreateBoundaryLevel(
        GeographicLevel level,
        string label,
        DateTime published,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        string fileContents;
        using (var stream = file.OpenReadStream())
        using (var reader = new StreamReader(stream))
        {
            fileContents = await reader.ReadToEndAsync();
        }

        FeatureCollection? featureCollection;

        try
        {
            featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(fileContents, new JsonSerializerSettings { CheckAdditionalContent = false });
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException("Failed to create boundary level: deserialisation failed due to malformed GeoJSON", ex);
        }

        return await boundaryLevelRepository.CreateBoundaryLevel(level, label, published, featureCollection!, cancellationToken);

    }

    public async Task<Either<ActionResult, BoundaryLevel>> UpdateBoundaryLevel(
        long id,
        string label,
        CancellationToken cancellationToken = default)
    {
        return await context.BoundaryLevel
            .FirstOrNotFoundAsync(level => level.Id == id, cancellationToken)
            .OnSuccess(async level =>
            {
                level.Label = label;
                await context.SaveChangesAsync(cancellationToken);
                return level;
            });
    }
}
