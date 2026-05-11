import logger from '@common/services/logger';
import { PaginatedList } from '@common/services/types/pagination';
import withMethods from '@frontend/middleware/api/withMethods';
import { initialiseAzureDataSetsSearchClient } from '@frontend/modules/api/search/initialiseAzureSearchClient';
import { ErrorBody } from '@frontend/modules/api/types/error';
import transformDataSetListResults from '@frontend/modules/search-data/utils/transformDataSetListResults';
import {
  AzureDataSetListRequest,
  AzureDataSetIndexItem,
} from '@frontend/services/azureDataSetService';
import { DataSetFileSummary } from '@frontend/services/dataSetFileService';
import {
  SearchOptions,
  SearchRequestQueryTypeOptions,
} from '@azure/search-documents';
import { NextApiRequest, NextApiResponse } from 'next';

interface Request extends NextApiRequest {
  body: {
    searchOptions: AzureDataSetListRequest;
  };
}

type SharedSearchOptionsBase = Partial<SearchOptions<AzureDataSetIndexItem>> &
  SearchRequestQueryTypeOptions;

export default withMethods({
  post: async function searchDatasets(
    req: Request,
    res: NextApiResponse<PaginatedList<DataSetFileSummary> | ErrorBody>,
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

      const searchOptionsBase: SharedSearchOptionsBase = {
        includeTotalCount: true,
        orderBy: orderBy ? [orderBy] : undefined,
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
          'fileId',
          'filename',
          'fileExtension',
          'fileSize',
          'title',
          'content',
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
          'numDataFileRows',
          'geographicLevelsLabels',
          'indicators',
          'filters',
          'releaseType',
          'timePeriodRange',
        ],
      });

      const { count = 0, results } = searchResults;

      const transformedResultsArray = await transformDataSetListResults(
        results,
      );

      const dataSetsResult = {
        paging: {
          totalPages: count === 0 ? 0 : Math.floor((count - 1) / pageSize) + 1,
          totalResults: count,
          page,
          pageSize,
        },
        results: transformedResultsArray,
      };

      return res.status(200).send(dataSetsResult);
    } catch (error) {
      logger.error(error);
      return res
        .status(500)
        .send({ message: 'Something went wrong', status: 500 });
    }
  },
});
