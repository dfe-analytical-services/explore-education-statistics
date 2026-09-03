#nullable enable
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

    public Guid ReleaseVersionId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

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
    public string? Source
    {
        get => ContentBlock.Source;
        set => ContentBlock.Source = value;
    }

    [NotMapped]
    public FullTableQuery Query
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
}
