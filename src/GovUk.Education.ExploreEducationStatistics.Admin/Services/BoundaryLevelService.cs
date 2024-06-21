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

public class BoundaryLevelService : IBoundaryLevelService
{
    private readonly IBoundaryLevelRepository _boundaryLevelRepository;

    public BoundaryLevelService(
        IBoundaryLevelRepository boundaryLevelRepository)
    {
        _boundaryLevelRepository = boundaryLevelRepository;
    }

    public async Task<List<BoundaryLevelViewModel>> Get()
    {
        var boundaryLevels = await _boundaryLevelRepository.Get();

        return boundaryLevels
            .Select(Map)
            .ToList();
    }

    public async Task<BoundaryLevelViewModel> Get(long id)
    {
        var boundaryLevel = await _boundaryLevelRepository.Get(id);

        return Map(boundaryLevel);
    }

    public async Task<BoundaryLevelViewModel> UpdateLabel(
        long id,
        string label)
    {
        if (id == 0)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return string.IsNullOrWhiteSpace(label)
            ? throw new ArgumentNullException(nameof(label))
            : Map(await _boundaryLevelRepository.Update(id, label));
    }

    private static BoundaryLevelViewModel Map(BoundaryLevel boundaryLevel)
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
