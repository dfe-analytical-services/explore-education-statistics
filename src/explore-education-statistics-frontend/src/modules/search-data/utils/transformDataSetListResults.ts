/* eslint-disable no-restricted-syntax */
import { SearchIterator } from '@azure/search-documents';
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
    | 'releaseId'
    | 'releaseTitle'
    | 'releaseSlug'
    | 'latestData'
    | 'isSuperseded'
    | 'published'
    | 'lastUpdated'
    | 'api'
    | 'numDataFileRows'
    | 'geographicLevelsLabels'
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
      releaseId,
      releaseTitle,
      releaseSlug,
      latestData,
      isSuperseded,
      published,
      lastUpdated,
      api,
      numDataFileRows,
      geographicLevelsLabels: geographicLevels,
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
        geographicLevels,
        timePeriodRange,
        filters,
        indicators,
      },
    });
  }

  return transformedResults;
}
