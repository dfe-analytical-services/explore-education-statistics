#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
    // EES-4640 - delegate methods to underlying ContentBlock of type "DataBlock" are included below.
    // This is to make the effort in removing DataBlocks from the ContentBlock table easier for EES-4640, as
    // their DataBlock fields will eventually be incorporated into DataBlockVersion. These delegate methods
    // help DataBlockVersion to encapsulate properties from the underlying ContentBlock DataBlock.
    //
    [NotMapped]
    public ContentSection? ContentSection
    {
        get => ContentBlock.ContentSection;
        set => ContentBlock.ContentSection = value;
    }

    [NotMapped]
    public Guid? ContentSectionId
    {
        get => ContentBlock.ContentSectionId;
        set => ContentBlock.ContentSectionId = value;
    }

    [NotMapped]
    public int Order
    {
        get => ContentBlock.Order;
        set => ContentBlock.Order = value;
    }

    [NotMapped]
    public List<Comment> Comments
    {
        get => ContentBlock.Comments;
        set => ContentBlock.Comments = value;
    }

    [NotMapped]
    public DateTime? Locked
    {
        get => ContentBlock.Locked;
        set => ContentBlock.Locked = value;
    }

    [NotMapped]
    public DateTime? LockedUntil => ContentBlock.LockedUntil;

    [NotMapped]
    public User? LockedBy
    {
        get => ContentBlock.LockedBy;
        set => ContentBlock.LockedBy = value;
    }

    [NotMapped]
    public Guid? LockedById
    {
        get => ContentBlock.LockedById;
        set => ContentBlock.LockedById = value;
    }

    [NotMapped]
    public string Heading
    {
        get => ContentBlock.Heading;
        set => ContentBlock.Heading = value;
    }

    [NotMapped]
    public string Name
    {
        get => ContentBlock.Name;
        set => ContentBlock.Name = value;
    }

    [NotMapped]
    public string Source
    {
        get => ContentBlock.Source;
        set => ContentBlock.Source = value;
    }

    [NotMapped]
    public ObservationQueryContext Query
    {
        get => ContentBlock.Query;
        set => ContentBlock.Query = value;
    }

    [NotMapped]
    public List<IChart> Charts
    {
        get => ContentBlock.Charts;
        set => ContentBlock.Charts = value;
    }

    [NotMapped]
    public TableBuilderConfiguration Table
    {
        get => ContentBlock.Table;
        set => ContentBlock.Table = value;
    }

    // TODO - EES-4637 - we need to decide on how we're being consistent with Created dates in Release Amendments.
    public DataBlockVersion Clone(Release amendment)
    {
        var now = DateTime.UtcNow;
        var clonedContentBlock = (ContentBlock.Clone(now) as DataBlock)!;
        clonedContentBlock.ReleaseId = amendment.Id;
        clonedContentBlock.Release = amendment;

        var copy = (MemberwiseClone() as DataBlockVersion)!;

        // Keep a one-to-one relationship between DataBlockVersions and ContentBlocks of type "DataBlock".
        // This will make it easier to migrate DataBlocks out of the ContentBlock table in the future stages.
        copy.Id = clonedContentBlock.Id;
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