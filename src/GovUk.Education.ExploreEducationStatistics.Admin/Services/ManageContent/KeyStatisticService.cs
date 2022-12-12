#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class KeyStatisticService : IKeyStatisticService
{
    private readonly ContentDbContext _context;
    private readonly IMapper _mapper;

    public KeyStatisticService(ContentDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async void CreateKeyStatisticDataBlock(KeyStatisticDataBlockCreateRequest request)
    {
        var keyStatisticDataBlock = _mapper.Map<KeyStatisticDataBlock>(request);
        await _context.AddAsync(keyStatisticDataBlock);
        await _context.SaveChangesAsync();
    }

    public async void UpdateKeyStatisticDataBlock(KeyStatisticDataBlockUpdateRequest request)
    {
        var keyStat = await _context
            .KeyStatisticsDataBlock
            .SingleAsync(ks =>
                ks.Id == request.Id);

        _context.Update(keyStat);

        keyStat.Trend = request.Trend;
        keyStat.GuidanceTitle = request.GuidanceTitle;
        keyStat.GuidanceText = request.GuidanceText;

        await _context.SaveChangesAsync();
    }

    public async void Delete(Guid releaseId, Guid keyStatisticId)
    {
        var keyStat = await _context
            .KeyStatistics
            .SingleAsync(ks =>
                ks.Id == keyStatisticId);

        _context.Remove(keyStat);

        await _context.SaveChangesAsync();
    }

    public async void Reorder(Guid releaseId, Dictionary<Guid, int> newKeyStatisticOrder)
    {
        var keyStatistics = _context.KeyStatistics
            .Where(ks => ks.ReleaseId == releaseId)
            .ToList();

        var keyValuePairs = newKeyStatisticOrder.ToList();

        if (keyStatistics.Count != keyValuePairs.Count)
        {
            throw new ArgumentException("newKeyStatisticOrder.Count must equal release's key statistics count");
        }

        // @MarkFix make this better - don't find one at a time
        keyValuePairs.ForEach(kvp =>
        {
            var (keyStatisticId, newOrder) = kvp;
            var matchingKeyStat = keyStatistics.Find(ks => ks.Id == keyStatisticId);
            if (matchingKeyStat is not null)
            {
                matchingKeyStat.Order = newOrder;
                _context.Update(matchingKeyStat);
            }
        });

        await _context.SaveChangesAsync();
    }
}
