/* eslint-disable no-restricted-syntax */
import { SearchIterator } from '@azure/search-documents';
import sortPublishingOrganisationTitles from '@frontend/modules/search-data/utils/sortPublishingOrganisationTitles';
import { AzureDataSetIndexItem } from '@frontend/services/azureDataSetService';
import { DataSetFileSummary } from '@frontend/services/dataSetFileService';

export default async function transformDataSetListResults(
  results: SearchIterator<
    AzureDataSetIndexItem,
    | 'dataSetFileId'
    | 'fileId'
    | 'filename'
    | 'fileExtension'
    | 'fileSize'
    | 'title'
    | 'content'
    | 'themeId'
    | 'themeTitle'
    | 'publicationId'
    | 'publicationTitle'
    | 'publicationSlug'
    | 'publishingOrganisationTitles'
    | 'releaseId'
    | 'releaseTitle'
    | 'releaseSlug'
    | 'latestData'
    | 'isSuperseded'
    | 'published'
    | 'lastUpdated'
    | 'api'
    | 'numDataFileRows'
    | 'geographicLevelDetails'
    | 'indicators'
    | 'filters'
    | 'releaseType'
    | 'timePeriodRange'
  >,
): Promise<DataSetFileSummary[]> {
  const transformedResults: DataSetFileSummary[] = [];

  for await (const result of results) {
    const { document } = result;
    const {
      dataSetFileId,
      fileId,
      fileSize,
      filename,
      fileExtension,
      title,
      content,
      themeId,
      themeTitle,
      publicationId,
      publicationTitle,
      publicationSlug,
      publishingOrganisationTitles,
      releaseId,
      releaseTitle,
      releaseSlug,
      latestData,
      isSuperseded,
      published,
      lastUpdated,
      api,
      numDataFileRows,
      geographicLevelDetails,
      indicators,
      filters,
      timePeriodRange,
    } = document;

    transformedResults.push({
      id: dataSetFileId,
      fileId,
      filename,
      fileSize,
      fileExtension,
      title,
      content,
      theme: {
        id: themeId,
        title: themeTitle,
      },
      publication: {
        id: publicationId,
        title: publicationTitle,
        slug: publicationSlug,
      },
      publishingOrganisationTitles: sortPublishingOrganisationTitles(
        publishingOrganisationTitles,
      ),
      release: {
        id: releaseId,
        title: releaseTitle,
        slug: releaseSlug,
      },
      latestData,
      isSuperseded,
      published: new Date(published),
      lastUpdated,
      api: api && api.id && api.id.length > 0 ? api : undefined,
      meta: {
        numDataFileRows,
        geographicLevels: geographicLevelDetails
          .filter(level => !level.csvOnly)
          .map(level => level.label),
        geographicLevelsCsvOnly: geographicLevelDetails
          .filter(level => level.csvOnly)
          .map(level => level.label),
        timePeriodRange,
        filters,
        indicators,
      },
    });
  }

  return transformedResults;
}
