using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Releases.Dtos;

public class ContentBlockBaseDtoTests
{
    // The public frontend switches on these `type` discriminators to decide how to render each
    // block, silently rendering nothing for an unrecognised one. They must stay in step with the
    // BlockViewModel types in explore-education-statistics-common/src/services/publicationService.ts.
    [Theory]
    [MemberData(nameof(ContentBlocks))]
    public void WhenSerializing_UsesExpectedTypeDiscriminator(ContentBlockBaseDto contentBlock, string expectedType)
    {
        var serialized = JObject.Parse(JsonConvert.SerializeObject(contentBlock));

        Assert.Equal(expectedType, serialized["type"]?.Value<string>());
    }

    public static TheoryData<ContentBlockBaseDto, string> ContentBlocks() =>
        new()
        {
            {
                new DataBlockVersionLinkDto
                {
                    Id = Guid.NewGuid(),
                    DataBlockVersion = new DataBlockVersionDto
                    {
                        DataBlockVersionId = Guid.NewGuid(),
                        DataBlockId = Guid.NewGuid(),
                        Charts = new List<IChart>(),
                        Heading = "Data block heading",
                        Name = "Data block name",
                        Query = new FullTableQuery(),
                        Source = null,
                        Table = new TableBuilderConfiguration(),
                    },
                },
                "DataBlockVersionLink"
            },
            {
                new EmbedBlockLinkDto
                {
                    Id = Guid.NewGuid(),
                    EmbedBlock = new EmbedBlockDto
                    {
                        EmbedBlockId = Guid.NewGuid(),
                        Title = "Embed block title",
                        Url = "https://departmentforeducation.shinyapps.io/embed-block",
                    },
                },
                "EmbedBlock"
            },
            {
                new HtmlBlockDto { Id = Guid.NewGuid(), Body = "<p>Html block body</p>" },
                "HtmlBlock"
            },
        };
}
