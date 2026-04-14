/* eslint-disable no-restricted-syntax */
import logger from '@common/services/logger';
import withMethods from '@frontend/middleware/api/withMethods';
import { initialiseAzureDataSetsSearchClient } from '@frontend/modules/api/search/initialiseAzureSearchClient';
import { ErrorBody } from '@frontend/modules/api/types/error';
import {
  AzureDataSetListRequest,
  AzureDataSetSearchResult,
} from '@frontend/services/azureDataSetService';
import { PaginatedListWithAzureFacets } from '@frontend/services/azurePublicationService';
import {
  SearchOptions,
  SearchRequestQueryTypeOptions,
} from '@azure/search-documents';
import { NextApiRequest, NextApiResponse } from 'next';
import { DataSetFileSummary } from '@frontend/services/dataSetFileService';
import { geographicLevelCodesMap } from '@common/utils/locationLevelsMap';

interface Request extends NextApiRequest {
  body: {
    searchOptions: AzureDataSetListRequest;
  };
}

type SharedSearchOptionsBase = Partial<
  SearchOptions<AzureDataSetSearchResult>
> &
  SearchRequestQueryTypeOptions;

export default withMethods({
  post: async function searchPublications(
    req: Request,
    res: NextApiResponse<
      PaginatedListWithAzureFacets<DataSetFileSummary> | ErrorBody
    >,
  ) {
    const {
      body: { searchOptions },
    } = req;

    const azureSearchClient = initialiseAzureDataSetsSearchClient();

    try {
      const {
        filter,
        orderBy,
        page = 1,
        pageSize = 10,
        search = '',
      } = searchOptions;

      // TODO EES-7072 reinstate orderBy, check queryType
      const searchOptionsBase: SharedSearchOptionsBase = {
        includeTotalCount: true,
        // orderBy: orderBy ? [orderBy] : undefined,
        orderBy: undefined,
        queryType: 'full',
        searchMode: 'any',
        skip: page > 1 ? (page - 1) * 10 : 0,
        top: 10,
      };

      // Get all search results
      const searchResults = await azureSearchClient.search(search, {
        ...searchOptionsBase,
        filter,
        select: [
          'id',
          'filename',
          'fileExtension',
          'themeId',
          'themeTitle',
          'publicationId',
          'publicationTitle',
          'publicationSlug',
          'releaseId',
          'releaseTitle',
          'releaseSlug',
          'latestData',
          'isSuperseded',
          'published',
          'lastUpdated',
          'api',
          'NumDataFileRows',
          'geographicLevels',
          'indicators',
          'filters',
          'release_type',
          'timePeriodRange',
        ],
      });

      // Now transform response into <PaginatedListWithAzureFacets<AzureDataSetListSummary>>
      const { count = 0, results, facets = {} } = searchResults;

      const dataSetsResult = {
        paging: {
          totalPages: count === 0 ? 0 : Math.floor((count - 1) / pageSize) + 1,
          totalResults: count,
          page,
          pageSize,
        },
        results: [] as DataSetFileSummary[],
        facets,
      };

      for await (const result of results) {
        const { document } = result;
        const {
          id,
          filename,
          fileExtension,
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
          NumDataFileRows,
          geographicLevels,
          indicators,
          filters,
          timePeriodRange,
        } = document;

        dataSetsResult.results.push({
          id,
          fileId: id,
          filename,
          fileSize: 'missing',
          fileExtension,
          title: releaseTitle,
          content: 'missing',
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
          api: api
            ? {
                id: 'missing',
                version: '1.0.1',
              }
            : undefined,
          meta: {
            numDataFileRows: NumDataFileRows,
            geographicLevels: geographicLevels.map(
              code => geographicLevelCodesMap[code].label,
            ),
            timePeriodRange,
            filters,
            indicators,
          },
        });
      }
      return res.status(200).send(dataSetsResult);
    } catch (error) {
      logger.error(error);
      return res
        .status(500)
        .send({ message: 'Something went wrong', status: 500 });
    }
  },
});
