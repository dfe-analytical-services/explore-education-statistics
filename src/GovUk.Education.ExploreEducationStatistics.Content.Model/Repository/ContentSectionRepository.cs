#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
    
public class ContentSectionRepository : IContentSectionRepository
{
    private readonly ContentDbContext _contentDbContext;

    public ContentSectionRepository(ContentDbContext contentDbContext)
    {
        _contentDbContext = contentDbContext;
    }

    public async Task<List<T>> GetAllContentBlocks<T>(Guid releaseId) where T : ContentBlock
    {
        return await _contentDbContext
            .ContentBlocks
            .Where(block => block.ReleaseId == releaseId)
            .OfType<T>()
            .ToListAsync();
    }
}
