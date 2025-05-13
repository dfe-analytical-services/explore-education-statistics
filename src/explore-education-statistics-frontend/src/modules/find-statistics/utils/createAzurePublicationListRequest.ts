import { odata } from '@azure/search-documents';
import { releaseTypes, ReleaseType } from '@common/services/types/releaseType';
import getFirst from '@common/utils/getFirst';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { FindStatisticsPageQuery } from '@frontend/modules/find-statistics/FindStatisticsPage';
import {
  PublicationSortOption,
  publicationSortOptions,
} from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import {
  AzurePublicationListRequest,
  AzurePublicationOrderByParam,
} from '@frontend/services/azurePublicationService';
import omitBy from 'lodash/omitBy';

export default function createAzurePublicationListRequest(
  query: FindStatisticsPageQuery,
): AzurePublicationListRequest {
  const {
    releaseType,
    search: searchParam,
    sortBy,
    themeId,
  } = getParamsFromQuery(query);

  const { sort } = getSortParams(sortBy);

  let filter: string | undefined;
  if (releaseType && themeId) {
    filter = odata`releaseType eq ${releaseType} and themeId eq ${themeId}`;
  } else if (releaseType) {
    filter = odata`releaseType eq ${releaseType}`;
  } else if (themeId) {
    filter = odata`themeId eq ${themeId}`;
  }

  const minSearchCharacters = 3;
  const search =
    searchParam && searchParam.length >= minSearchCharacters ? searchParam : '';

  return omitBy(
    {
      filter,
      page: parseNumber(query.page) ?? 1,
      releaseType,
      search,
      orderBy: sort,
      themeId,
    },
    value => typeof value === 'undefined',
  );
}

function getSortParams(sortBy: PublicationSortOption): {
  sort: AzurePublicationOrderByParam;
} {
  if (sortBy === 'relevance') {
    return {
      sort: undefined,
    };
  }
  if (sortBy === 'title') {
    return {
      sort: 'title asc',
    };
  }
  if (sortBy === 'oldest') {
    return {
      sort: 'published asc',
    };
  }
  return {
    sort: 'published desc',
  };
}

export function getParamsFromQuery(query: FindStatisticsPageQuery) {
  return {
    page: getFirst(query.page),
    releaseType:
      query.releaseType &&
      isOneOf(query.releaseType, Object.keys(releaseTypes) as ReleaseType[])
        ? query.releaseType
        : undefined,
    search: getFirst(query.search),
    sortBy:
      query.sortBy && isOneOf(query.sortBy, publicationSortOptions)
        ? query.sortBy
        : 'newest',
    themeId: getFirst(query.themeId),
  };
}
