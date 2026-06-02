/* eslint-disable no-restricted-syntax */
import withMethods from '@frontend/middleware/api/withMethods';
import logger from '@common/services/logger';
import { PublicationListSummary } from '@common/services/publicationService';
import { PaginatedList } from '@common/services/types/pagination';
import { ReleaseType } from '@common/services/types/releaseType';
import {
  SearchOptions,
  SearchRequestQueryTypeOptions,
} from '@azure/search-documents';
import { NextApiRequest, NextApiResponse } from 'next';
import { initialiseAzurePublicationsSearchClient } from '@frontend/modules/api/search/initialiseAzureSearchClient';
import { ErrorBody } from '@frontend/modules/api/types/error';
import {
  AzurePublicationListRequest,
  AzurePublicationSearchResult,
} from '@frontend/services/azurePublicationService';

interface Request extends NextApiRequest {
  body: {
    searchOptions: AzurePublicationListRequest;
  };
}

type SharedSearchOptionsBase = Partial<
  SearchOptions<AzurePublicationSearchResult>
> &
  SearchRequestQueryTypeOptions;

export default withMethods({
  post: async function searchStatisticalReleases(
    req: Request,
    res: NextApiResponse<PaginatedList<PublicationListSummary> | ErrorBody>,
  ) {
    const {
      body: { searchOptions },
    } = req;

    const azureSearchClient = initialiseAzurePublicationsSearchClient();

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
        queryType: !orderBy ? 'semantic' : undefined,
        searchMode: 'any',
        semanticSearchOptions: {
          configurationName: 'semantic-configuration-1',
        },
        scoringProfile: 'scoring-profile-1',
        skip: page > 1 ? (page - 1) * 10 : 0,
        top: 10,
      };

      // Get all search results
      const searchResults = await azureSearchClient.search(search, {
        ...searchOptionsBase,
        highlightFields: 'title,summary,content-3',
        filter,
        select: [
          'content',
          'releaseSlug',
          'releaseType',
          'releaseVersionId',
          'publicationSlug',
          'published',
          'summary',
          'themeTitle',
          'title',
        ],
      });

      // Now transform response into <PaginatedList<PublicationListSummary>>
      const { count = 0, results } = searchResults;

      const publicationsResult = {
        paging: {
          totalPages: count === 0 ? 0 : Math.floor((count - 1) / pageSize) + 1,
          totalResults: count,
          page,
          pageSize,
        },
        results: [] as PublicationListSummary[],
      };

      for await (const result of results) {
        const { document } = result;
        const {
          themeTitle,
          title,
          summary,
          publicationSlug: slug,
          published,
          releaseVersionId: id,
          releaseSlug: latestReleaseSlug,
          releaseType: type,
        } = document;

        publicationsResult.results.push({
          theme: themeTitle,
          title,
          summary,
          highlightContent: result.highlights?.content?.join(' ... ') || null,
          highlightSummary: result.highlights?.summary?.join(' ... ') || null,
          highlightTitle: result.highlights?.title?.join(' ... ') || null,
          published: published.toString(),
          id,
          rank: result.score,
          slug,
          latestReleaseSlug,
          type: type as ReleaseType,
        });
      }
      return res.status(200).send(publicationsResult);
    } catch (error) {
      logger.error(error);
      return res
        .status(500)
        .send({ message: 'Something went wrong', status: 500 });
    }
  },
});
