using System.Text.Json.Nodes;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Extensions;

public class DataSetDocumentExtensionsTests
{
    public class ToJsonTests
    {
        [Fact]
        public void PropertiesAreNamedAsTheSearchIndexFields()
        {
            var dataSetDocument = new DataSetDocumentBuilder()
                .WithApi(
                    new DataSetDocumentApi { Id = new Guid("0d1e2f3a-4b5c-4d6e-8f70-8192a3b4c504"), Version = "1.2" }
                )
                .Build();

            var json = dataSetDocument.ToJson();

            const string expected = """
                {
                  "fileId": "3d6cf2b9-6b0a-4b0e-9d6f-0f5b7c1f6a01",
                  "dataSetFileId": "5f1f4d0e-2c57-4a4e-8a52-7f0d5b9f3c02",
                  "subjectId": "9a7b2c1d-4e3f-4a5b-8c6d-1e2f3a4b5c03",
                  "filename": "data-file.csv",
                  "fileExtension": "csv",
                  "fileSize": "10 Kb",
                  "title": "Data Set Title",
                  "content": "This is a data set summary.",
                  "themeId": "498e449b-d093-43dc-959c-bc3a2c23fdfb",
                  "themeTitle": "Theme Title",
                  "publicationId": "8db69a42-ef47-441b-bed5-5e91d012194f",
                  "publicationTitle": "Publication Title",
                  "publicationSlug": "publication-slug",
                  "releaseId": "08228000-71de-4a1a-9adf-35f3d2bd061a",
                  "releaseVersionId": "a20f1068-4298-495d-8d80-c662ca72731d",
                  "releaseTitle": "Academic year 2024/25",
                  "releaseSlug": "2024-25",
                  "releaseType": "OfficialStatistics",
                  "latestData": true,
                  "isSuperseded": false,
                  "published": "2025-02-21T09:30:00+00:00",
                  "lastUpdated": "2025-03-14T10:15:00+00:00",
                  "api": { "id": "0d1e2f3a-4b5c-4d6e-8f70-8192a3b4c504", "version": "1.2" },
                  "numDataFileRows": 1234,
                  "geographicLevelDetails": [
                    { "code": "LA", "label": "Local authority", "csvOnly": false },
                    { "code": "NAT", "label": "National", "csvOnly": true }
                  ],
                  "timePeriodRange": { "from": "2020/21", "to": "2024/25" },
                  "filters": ["Filter 1", "Filter 2"],
                  "indicators": ["Indicator 1", "Indicator 2"],
                  "publishingOrganisationIds": [
                    "7cbcfe03-9f7e-478a-8512-a1a5e0ca793b",
                    "7c3252c6-6e94-4a34-8762-ff52aae5c0c0"
                  ],
                  "publishingOrganisationTitles": ["Department for Education", "Ofsted"]
                }
                """;
            Assert.True(
                JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(json)),
                $"Unexpected JSON: {json}"
            );
        }

        [Fact]
        public void WhenDataSetHasNoApi_ThenApiIsNull()
        {
            var dataSetDocument = new DataSetDocumentBuilder().Build();

            var json = dataSetDocument.ToJson();

            var jsonObject = JsonNode.Parse(json)!.AsObject();
            Assert.True(jsonObject.ContainsKey("api"));
            Assert.Null(jsonObject["api"]);
        }
    }
}
