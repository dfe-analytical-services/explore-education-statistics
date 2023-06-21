import {
  PublicationListRequest,
  PublicationOrderParam,
  PublicationSortOption,
  publicationSortOptions,
  PublicationSortParam,
} from '@common/services/publicationService';
import { releaseTypes, ReleaseType } from '@common/services/types/releaseType';
import getFirst from '@common/utils/getFirst';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { FindStatisticsPageQuery } from '@frontend/modules/find-statistics/FindStatisticsPage';
import omitBy from 'lodash/omitBy';

export default function createPublicationListRequest(
  query: FindStatisticsPageQuery,
): PublicationListRequest {
  const {
    releaseType,
    search: searchParam,
    sortBy,
    themeId,
  } = getParamsFromQuery(query);

  const { order, sort } = getSortParams(sortBy);

  const minSearchCharacters = 3;
  const search =
    searchParam && searchParam.length >= minSearchCharacters ? searchParam : '';

  return omitBy(
    {
      order,
      page: parseNumber(query.page) ?? 1,
      sort,
      releaseType,
      search,
      themeId,
    },
    value => typeof value === 'undefined',
  );
}

function getSortParams(
  sortBy: PublicationSortOption,
): {
  order?: PublicationOrderParam;
  sort?: PublicationSortParam;
} {
  if (sortBy === 'relevance') {
    return {
      order: 'desc',
      sort: 'relevance',
    };
  }
  if (sortBy === 'title') {
    return {
      order: 'asc',
      sort: 'title',
    };
  }
  if (sortBy === 'oldest') {
    return {
      order: 'asc',
      sort: 'published',
    };
  }
  return {
    order: 'desc',
    sort: 'published',
  };
}

export function getParamsFromQuery(query: FindStatisticsPageQuery) {
  return {
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
