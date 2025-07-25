/* eslint-disable no-restricted-syntax */
import withMethods from '@frontend/middleware/api/withMethods';
import logger from '@common/services/logger';
import { PublicationListSummary } from '@common/services/publicationService';
import { ReleaseType } from '@common/services/types/releaseType';
import { odata, SearchOptions } from '@azure/search-documents';
import { NextApiRequest, NextApiResponse } from 'next';
import initialiseAzureSearchClient from '@frontend/modules/api/search/initialiseAzureSearchClient';
import { ErrorBody } from '@frontend/modules/api/types/error';
import {
  AzurePublicationListRequest,
  AzurePublicationSearchResult,
  PaginatedListWithAzureFacets,
} from '@frontend/services/azurePublicationService';

interface Request extends NextApiRequest {
  body: {
    searchOptions: AzurePublicationListRequest;
  };
}

export default withMethods({
  post: async function searchPublications(
    req: Request,
    res: NextApiResponse<
      PaginatedListWithAzureFacets<PublicationListSummary> | ErrorBody
    >,
  ) {
    const {
      body: { searchOptions },
    } = req;

    const azureSearchClient = initialiseAzureSearchClient();

    try {
      const {
        filter,
        orderBy,
        page = 1,
        pageSize = 10,
        releaseType,
        search = '',
        themeId,
      } = searchOptions;

      const searchOptionsBase = {
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
      } as Pick<
        SearchOptions<AzurePublicationSearchResult>,
        | 'includeTotalCount'
        | 'orderBy'
        | ('queryType' & 'semanticSearchOptions')
        | 'scoringProfile'
        | 'searchMode'
        | 'skip'
        | 'top'
      >;

      // Get all search results
      const searchResults = await azureSearchClient.search(search, {
        ...searchOptionsBase,
        highlightFields: 'title,summary,content-3',
        facets: ['themeId,count:150,sort:count', 'releaseType'],
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

      // If a theme filter is selected - let's get the facet counts
      // for as if we hadn't filtered by theme too
      const themeFacetResults = themeId
        ? await azureSearchClient.search(search, {
            ...searchOptionsBase,
            facets: ['themeId,count:150,sort:count'],
            select: [],
            // Filter still needs to account for releaseType if it is present
            filter: releaseType
              ? odata`releaseType eq ${releaseType}`
              : undefined,
          })
        : null;

      // If a releaseType filter is selected - let's get the facet counts
      // for as if we hadn't filtered by releaseType too
      const releaseTypeFacetResults = releaseType
        ? await azureSearchClient.search(search, {
            ...searchOptionsBase,
            facets: ['releaseType'],
            select: [],
            // Filter still needs to account for themeId if it is present
            filter: themeId ? odata`themeId eq ${themeId}` : undefined,
          })
        : null;

      // Now transform response into <PaginatedListWithAzureFacets<PublicationListSummary>>
      const { count = 0, results, facets = {} } = searchResults;
      const facetsCombined = {
        ...facets,
        ...themeFacetResults?.facets,
        ...releaseTypeFacetResults?.facets,
      };
      const publicationsResult = {
        paging: {
          totalPages: count === 0 ? 0 : Math.floor((count - 1) / pageSize) + 1,
          totalResults: count,
          page,
          pageSize,
        },
        results: [] as PublicationListSummary[],
        facets: facetsCombined,
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
