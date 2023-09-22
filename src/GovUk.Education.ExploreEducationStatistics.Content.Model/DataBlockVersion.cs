#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataBlockVersion : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; init; }
    
    public Guid DataBlockParentId { get; init; }
        
    public DataBlockParent DataBlockParent { get; init; }
    
    public Guid ReleaseVersionId { get; init; }
    
    public Release ReleaseVersion { get; init; }
    
    public Guid DataBlockId { get; init; }
    
    public DataBlock DataBlock { get; init; }
    
    public int Version { get; init; }

    public DateTime? Published { get; set; }
    
    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }
}