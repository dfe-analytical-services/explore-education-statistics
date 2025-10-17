using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;

public class ReleaseContentDtoBuilder
{
    private Guid _releaseId = Guid.NewGuid();
    private Guid _releaseVersionId = Guid.NewGuid();
    private ContentSectionDto[] _content = [new ContentSectionDtoBuilder().Build()];
    private ContentSectionDto _headlinesSection = new ContentSectionDtoBuilder().Build();

    private KeyStatisticBaseDto[] _keyStatistics =
    [
        new KeyStatisticDataBlockDtoBuilder().Build(),
        new KeyStatisticTextDtoBuilder().Build(),
    ];

    private ContentSectionDto _summarySection = new ContentSectionDtoBuilder().Build();
    private ContentSectionDto _keyStatisticsSecondarySection = new ContentSectionDtoBuilder().Build();

    public ReleaseContentDto Build() =>
        new()
        {
            ReleaseId = _releaseId,
            ReleaseVersionId = _releaseVersionId,
            Content = _content,
            HeadlinesSection = _headlinesSection,
            KeyStatistics = _keyStatistics,
            KeyStatisticsSecondarySection = _keyStatisticsSecondarySection,
            SummarySection = _summarySection,
        };

    public ReleaseContentDtoBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }

    public ReleaseContentDtoBuilder WithReleaseVersionId(Guid releaseVersionId)
    {
        _releaseVersionId = releaseVersionId;
        return this;
    }

    public ReleaseContentDtoBuilder WithContent(ContentSectionDto[] content)
    {
        _content = content;
        return this;
    }

    public ReleaseContentDtoBuilder WithHeadlinesSection(ContentSectionDto headlinesSection)
    {
        _headlinesSection = headlinesSection;
        return this;
    }

    public ReleaseContentDtoBuilder WithKeyStatistics(KeyStatisticBaseDto[] keyStatistics)
    {
        _keyStatistics = keyStatistics;
        return this;
    }

    public ReleaseContentDtoBuilder WithKeyStatisticsSecondarySection(ContentSectionDto keyStatisticsSecondarySection)
    {
        _keyStatisticsSecondarySection = keyStatisticsSecondarySection;
        return this;
    }

    public ReleaseContentDtoBuilder WithSummarySection(ContentSectionDto summarySection)
    {
        _summarySection = summarySection;
        return this;
    }
}

public class ContentSectionDtoBuilder
{
    private Guid _id = Guid.NewGuid();

    private ContentBlockBaseDto[] _content =
    [
        new DataBlockDtoBuilder().Build(),
        new EmbedBlockLinkDtoBuilder().Build(),
        new HtmlBlockDtoBuilder().Build(),
    ];

    private string? _heading = "Heading";

    public ContentSectionDto Build() =>
        new()
        {
            Id = _id,
            Content = _content,
            Heading = _heading,
        };

    public ContentSectionDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ContentSectionDtoBuilder WithContent(ContentBlockBaseDto[] content)
    {
        _content = content;
        return this;
    }

    public ContentSectionDtoBuilder WithHeading(string? heading)
    {
        _heading = heading;
        return this;
    }
}

public class DataBlockDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private DataBlockVersionDto _dataBlockVersion = new DataBlockVersionDtoBuilder().Build();

    public DataBlockDto Build() => new() { Id = _id, DataBlockVersion = _dataBlockVersion };

    public DataBlockDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public DataBlockDtoBuilder WithDataBlockVersion(DataBlockVersionDto dataBlockVersion)
    {
        _dataBlockVersion = dataBlockVersion;
        return this;
    }
}

public class DataBlockVersionDtoBuilder
{
    private Guid _dataBlockVersionId = Guid.NewGuid();
    private Guid _dataBlockParentId = Guid.NewGuid();
    private List<IChart> _charts = [];
    private string _heading = "Heading";
    private string _name = "Name";
    private FullTableQuery _query = new();
    private string? _source = "Source";
    private TableBuilderConfiguration _table = new();

    public DataBlockVersionDto Build() =>
        new()
        {
            DataBlockVersionId = _dataBlockVersionId,
            DataBlockParentId = _dataBlockParentId,
            Charts = _charts,
            Heading = _heading,
            Name = _name,
            Query = _query,
            Source = _source,
            Table = _table,
        };

    public DataBlockVersionDtoBuilder WithDataBlockVersionId(Guid dataBlockVersionId)
    {
        _dataBlockVersionId = dataBlockVersionId;
        return this;
    }

    public DataBlockVersionDtoBuilder WithDataBlockParentId(Guid dataBlockParentId)
    {
        _dataBlockParentId = dataBlockParentId;
        return this;
    }

    public DataBlockVersionDtoBuilder WithCharts(List<IChart> charts)
    {
        _charts = charts;
        return this;
    }

    public DataBlockVersionDtoBuilder WithHeading(string heading)
    {
        _heading = heading;
        return this;
    }

    public DataBlockVersionDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public DataBlockVersionDtoBuilder WithQuery(FullTableQuery query)
    {
        _query = query;
        return this;
    }

    public DataBlockVersionDtoBuilder WithSource(string? source)
    {
        _source = source;
        return this;
    }

    public DataBlockVersionDtoBuilder WithTable(TableBuilderConfiguration table)
    {
        _table = table;
        return this;
    }
}

public class EmbedBlockLinkDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private EmbedBlockDto _embedBlock = new EmbedBlockDtoBuilder().Build();

    public EmbedBlockLinkDto Build() => new() { Id = _id, EmbedBlock = _embedBlock };

    public EmbedBlockLinkDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EmbedBlockLinkDtoBuilder WithEmbedBlock(EmbedBlockDto embedBlock)
    {
        _embedBlock = embedBlock;
        return this;
    }
}

public class EmbedBlockDtoBuilder
{
    private Guid _embedBlockId = Guid.NewGuid();
    private string _title = "Title";
    private string _url = "Url";

    public EmbedBlockDto Build() =>
        new()
        {
            EmbedBlockId = _embedBlockId,
            Title = _title,
            Url = _url,
        };

    public EmbedBlockDtoBuilder WithEmbedBlockId(Guid embedBlockId)
    {
        _embedBlockId = embedBlockId;
        return this;
    }

    public EmbedBlockDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public EmbedBlockDtoBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }
}

public class HtmlBlockDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _body = "Body";

    public HtmlBlockDto Build() => new() { Id = _id, Body = _body };

    public HtmlBlockDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public HtmlBlockDtoBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }
}

public class KeyStatisticDataBlockDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _dataBlockVersionId = Guid.NewGuid();
    private Guid _dataBlockParentId = Guid.NewGuid();
    private string? _guidanceText = "Guidance Text";
    private string? _guidanceTitle = "Guidance Title";
    private string? _trend = "Trend";

    public KeyStatisticDataBlockDto Build() =>
        new()
        {
            Id = _id,
            DataBlockVersionId = _dataBlockVersionId,
            DataBlockParentId = _dataBlockParentId,
            GuidanceText = _guidanceText,
            GuidanceTitle = _guidanceTitle,
            Trend = _trend,
        };

    public KeyStatisticDataBlockDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public KeyStatisticDataBlockDtoBuilder WithDataBlockVersionId(Guid dataBlockVersionId)
    {
        _dataBlockVersionId = dataBlockVersionId;
        return this;
    }

    public KeyStatisticDataBlockDtoBuilder WithDataBlockParentId(Guid dataBlockParentId)
    {
        _dataBlockParentId = dataBlockParentId;
        return this;
    }

    public KeyStatisticDataBlockDtoBuilder WithGuidanceText(string? guidanceText)
    {
        _guidanceText = guidanceText;
        return this;
    }

    public KeyStatisticDataBlockDtoBuilder WithGuidanceTitle(string? guidanceTitle)
    {
        _guidanceTitle = guidanceTitle;
        return this;
    }

    public KeyStatisticDataBlockDtoBuilder WithTrend(string? trend)
    {
        _trend = trend;
        return this;
    }
}

public class KeyStatisticTextDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string? _guidanceText = "Guidance Text";
    private string? _guidanceTitle = "Guidance Title";
    private string _statistic = "Statistic";
    private string _title = "Title";
    private string? _trend = "Trend";

    public KeyStatisticTextDto Build() =>
        new()
        {
            Id = _id,
            GuidanceText = _guidanceText,
            GuidanceTitle = _guidanceTitle,
            Statistic = _statistic,
            Title = _title,
            Trend = _trend,
        };

    public KeyStatisticTextDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public KeyStatisticTextDtoBuilder WithGuidanceText(string? guidanceText)
    {
        _guidanceText = guidanceText;
        return this;
    }

    public KeyStatisticTextDtoBuilder WithGuidanceTitle(string? guidanceTitle)
    {
        _guidanceTitle = guidanceTitle;
        return this;
    }

    public KeyStatisticTextDtoBuilder WithStatistic(string statistic)
    {
        _statistic = statistic;
        return this;
    }

    public KeyStatisticTextDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public KeyStatisticTextDtoBuilder WithTrend(string? trend)
    {
        _trend = trend;
        return this;
    }
}
