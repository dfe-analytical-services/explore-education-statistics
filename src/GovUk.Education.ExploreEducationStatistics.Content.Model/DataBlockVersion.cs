#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataBlockVersion : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    public Guid DataBlockParentId { get; set; }

    public DataBlockParent DataBlockParent { get; set; }

    public Guid ReleaseId { get; set; }

    public Release Release { get; set; }

    public Guid ContentBlockId { get; set; }

    public DataBlock ContentBlock { get; set; }

    public int Version { get; set; }

    public DateTime? Published { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    //
    // TODO EES-4467 - delegate methods to underlying ContentBlock of type "DataBlock" are included below.
    // This is to make the effort in removing DataBlocks from the ContentBlock table easier in the future, as
    // their DataBlock fields will eventually be incorporated into DataBlockVersion.
    //
    public ContentSection? ContentSection
    {
        get => ContentBlock.ContentSection;
        set => ContentBlock.ContentSection = value;
    }

    public Guid? ContentSectionId
    {
        get => ContentBlock.ContentSectionId;
        set => ContentBlock.ContentSectionId = value;
    }

    public int Order
    {
        get => ContentBlock.Order;
        set => ContentBlock.Order = value;
    }

    public List<Comment> Comments
    {
        get => ContentBlock.Comments;
        set => ContentBlock.Comments = value;
    }

    public DateTime? Locked
    {
        get => ContentBlock.Locked;
        set => ContentBlock.Locked = value;
    }

    public DateTime? LockedUntil => ContentBlock.LockedUntil;

    public User? LockedBy
    {
        get => ContentBlock.LockedBy;
        set => ContentBlock.LockedBy = value;
    }

    public Guid? LockedById
    {
        get => ContentBlock.LockedById;
        set => ContentBlock.LockedById = value;
    }

    public string Heading
    {
        get => ContentBlock.Heading;
        set => ContentBlock.Heading = value;
    }

    public string Name
    {
        get => ContentBlock.Name;
        set => ContentBlock.Name = value;
    }

    public string Source
    {
        get => ContentBlock.Source;
        set => ContentBlock.Source = value;
    }

    public ObservationQueryContext Query
    {
        get => ContentBlock.Query;
        set => ContentBlock.Query = value;
    }

    public List<IChart> Charts
    {
        get => ContentBlock.Charts;
        set => ContentBlock.Charts = value;
    }

    public TableBuilderConfiguration Table
    {
        get => ContentBlock.Table;
        set => ContentBlock.Table = value;
    }

    public DataBlockVersion Clone(Release amendment)
    {
        var now = DateTime.UtcNow;
        var clonedContentBlock = (ContentBlock.Clone(now) as DataBlock)!;

        var copy = (MemberwiseClone() as DataBlockVersion)!;
        copy.Id = Guid.NewGuid();
        copy.Created = DateTime.UtcNow;
        copy.Updated = null;
        copy.Published = null;
        copy.Release = amendment;
        copy.ReleaseId = amendment.Id;
        copy.Version = Version + 1;
        copy.ContentBlock = clonedContentBlock;
        copy.ContentBlockId = clonedContentBlock.Id;

        return copy;
    }
}