#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class BoundaryLevelService(
    IBoundaryLevelRepository boundaryLevelRepository) : IBoundaryLevelService
{
    public async Task<List<BoundaryLevelViewModel>> ListBoundaryLevels()
    {
        var boundaryLevels = await boundaryLevelRepository.ListBoundaryLevels();

        return boundaryLevels
            .Select(MapToViewModel)
            .ToList();
    }

    public async Task<BoundaryLevelViewModel?> GetBoundaryLevel(long id)
    {
        var boundaryLevel = await boundaryLevelRepository.GetBoundaryLevel(id);

        return boundaryLevel == null ? null : MapToViewModel(boundaryLevel);
    }

    public async Task UpdateBoundaryLevel(
        long id,
        string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentNullException(nameof(label));
        }

        await boundaryLevelRepository.UpdateBoundaryLevel(id, label);
    }

    private static BoundaryLevelViewModel MapToViewModel(BoundaryLevel boundaryLevel)
    {
        return new BoundaryLevelViewModel
        {
            Id = boundaryLevel.Id,
            Label = boundaryLevel.Label,
            Level = boundaryLevel.Level.GetEnumValue(),
            Published = boundaryLevel.Published,
        };
    }
}
