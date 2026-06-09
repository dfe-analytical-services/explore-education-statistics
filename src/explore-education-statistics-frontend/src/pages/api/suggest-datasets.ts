import logger from '@common/services/logger';
import withMethods from '@frontend/middleware/api/withMethods';
import { initialiseAzureDataSetsSearchClient } from '@frontend/modules/api/search/initialiseAzureSearchClient';
import { ErrorBody } from '@frontend/modules/api/types/error';
import {
  AzureDataSetIndexItem,
  AzureDataSetListRequest,
  AzureDataSetSuggestResult,
} from '@frontend/services/azureDataSetService';
import { NextApiRequest, NextApiResponse } from 'next';

interface Request extends NextApiRequest {
  body: {
    searchOptions: AzureDataSetListRequest;
  };
}

type SelectedDatasetFields = Pick<
  AzureDataSetIndexItem,
  'dataSetFileId' | 'title' | 'content'
>;

export default withMethods({
  post: async function suggestDatasets(
    req: Request,
    res: NextApiResponse<AzureDataSetSuggestResult[] | ErrorBody>,
  ) {
    const {
      body: { searchOptions },
    } = req;

    const azureSearchClient = initialiseAzureDataSetsSearchClient();

    try {
      const { filter, search = '' } = searchOptions;

      const suggestResults =
        search?.length > 2
          ? await azureSearchClient.suggest(
              search,
              'suggester-dataset-search',
              {
                select: ['dataSetFileId', 'title', 'content'],
                filter,
                searchFields: ['title', 'content'],
                useFuzzyMatching: true,
                top: 3,
                highlightPostTag: '</strong>',
                highlightPreTag: '<strong>',
              },
            )
          : null;

      let resultsToReturn = [] as AzureDataSetSuggestResult[];
      if (suggestResults?.results) {
        resultsToReturn = suggestResults?.results.map(result => {
          const document = result.document as SelectedDatasetFields;
          return {
            summary: document.content,
            dataSetFileId: document.dataSetFileId,
            title: document.title,
            highlightedMatch: result.text,
          } as AzureDataSetSuggestResult;
        });
      }
      return res.status(200).send(resultsToReturn);
    } catch (error) {
      logger.error(error);
      return res
        .status(500)
        .send({ message: 'Something went wrong', status: 500 });
    }
  },
});
