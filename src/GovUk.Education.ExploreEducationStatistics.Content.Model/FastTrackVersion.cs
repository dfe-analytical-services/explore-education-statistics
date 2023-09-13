#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class FastTrackVersion : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; init; }
    
    public Guid FastTrackId { get; init; }
        
    public FastTrack FastTrack { get; init; }
    
    public Guid ReleaseId { get; init; }
    
    public Release Release { get; init; }
    
    public Guid DataBlockId { get; init; }
    
    public DataBlock DataBlock { get; init; }
    
    public int Version { get; init; }

    public DateTime? Published { get; set; }
    
    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }
}