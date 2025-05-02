import {
  PublicationListRequest,
  PublicationSortParam,
} from '@common/services/publicationService';
import { releaseTypes, ReleaseType } from '@common/services/types/releaseType';
import { SortDirection } from '@common/services/types/sort';
import getFirst from '@common/utils/getFirst';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { FindStatisticsPageQuery } from '@frontend/modules/find-statistics/FindStatisticsPage';
import {
  PublicationSortOption,
  publicationSortOptions,
} from '@frontend/modules/find-statistics/utils/publicationSortOptions';
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

  const { sortDirection, sort } = getSortParams(sortBy);

  const minSearchCharacters = 3;
  const search =
    searchParam && searchParam.length >= minSearchCharacters ? searchParam : '';

  return omitBy(
    {
      page: parseNumber(query.page) ?? 1,
      releaseType,
      search,
      sort,
      sortDirection,
      themeId,
    },
    value => typeof value === 'undefined',
  );
}

function getSortParams(sortBy: PublicationSortOption): {
  sortDirection?: SortDirection;
  sort?: PublicationSortParam;
} {
  if (sortBy === 'relevance') {
    return {
      sort: 'relevance',
      sortDirection: 'Desc',
    };
  }
  if (sortBy === 'title') {
    return {
      sort: 'title',
      sortDirection: 'Asc',
    };
  }
  if (sortBy === 'oldest') {
    return {
      sort: 'published',
      sortDirection: 'Asc',
    };
  }
  return {
    sort: 'published',
    sortDirection: 'Desc',
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
