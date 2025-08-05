import withMethods from '@frontend/middleware/api/withMethods';
import logger from '@common/services/logger';
import { NextApiRequest, NextApiResponse } from 'next';
import initialiseAzureSearchClient from '@frontend/modules/api/search/initialiseAzureSearchClient';
import { ErrorBody } from '@frontend/modules/api/types/error';
import {
  AzurePublicationListRequest,
  AzurePublicationSuggestResult,
} from '@frontend/services/azurePublicationService';

interface Request extends NextApiRequest {
  body: {
    searchOptions: AzurePublicationListRequest;
  };
}

export default withMethods({
  post: async function suggestPublications(
    req: Request,
    res: NextApiResponse<AzurePublicationSuggestResult[] | ErrorBody>,
  ) {
    const {
      body: { searchOptions },
    } = req;

    const azureSearchClient = initialiseAzureSearchClient();

    try {
      const { filter, search = '' } = searchOptions;

      const suggestResults =
        search?.length > 2
          ? await azureSearchClient.suggest(search, 'suggester-1', {
              select: ['releaseSlug', 'title', 'summary', 'publicationSlug'],
              filter,
              searchFields: ['title', 'summary'],
              useFuzzyMatching: true,
              top: 3,
              highlightPostTag: '</strong>',
              highlightPreTag: '<strong>',
            })
          : null;

      let resultsToReturn = [] as AzurePublicationSuggestResult[];
      if (suggestResults?.results) {
        resultsToReturn = suggestResults?.results.map(result => {
          return {
            ...result.document,
            highlightedMatch: result.text,
          } as AzurePublicationSuggestResult;
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
