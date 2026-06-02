import { odata } from '@azure/search-documents';
import {
  releaseTypes as statisticsReleaseTypes,
  ReleaseType,
} from '@common/services/types/releaseType';
import getAsArray from '@common/utils/getAsArray';
import getFirst from '@common/utils/getFirst';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import {
  PublicationSortOption,
  publicationSortOptions,
} from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import { SearchDataPageQuery } from '@frontend/modules/search-data/SearchDataPage';
import { AzureDataSetListRequest } from '@frontend/services/azureDataSetService';
import { AzureOrderByParam } from '@frontend/services/azurePublicationService';
import omitBy from 'lodash/omitBy';

export default function createStatisticalReleasesListRequest(
  query: SearchDataPageQuery,
): AzureDataSetListRequest {
  const {
    releaseTypes,
    search: searchParam,
    sortBy,
    themeIds,
  } = getParamsFromQuery(query);

  const orderBy = getSortParam(sortBy);

  const filter = buildODataFilter({
    releaseTypes,
    themeIds,
  });

  const minSearchCharacters = 3;
  const search =
    searchParam && searchParam.length >= minSearchCharacters ? searchParam : '';

  return omitBy(
    {
      filter,
      page: parseNumber(query.page) ?? 1,
      search,
      orderBy,
    },
    value => typeof value === 'undefined',
  );
}

interface SearchFilters {
  releaseTypes?: string[];
  themeIds?: string[];
}

function buildODataFilter(filters: SearchFilters): string | undefined {
  const conditions: string[] = [];

  if (filters.releaseTypes?.length) {
    const joined = filters.releaseTypes.join('|');
    conditions.push(odata`search.in(releaseType, ${joined}, '|')`);
  }

  if (filters.themeIds?.length) {
    const joined = filters.themeIds.join('|');
    conditions.push(odata`search.in(themeId, ${joined}, '|')`);
  }

  // Combine everything or return undefined if no filters were provided
  return conditions.length > 0 ? conditions.join(' and ') : undefined;
}

function getSortParam(sortBy: PublicationSortOption): AzureOrderByParam {
  switch (sortBy) {
    case 'relevance':
      return undefined;
    case 'title':
      return 'title asc';
    case 'oldest':
      return 'published asc';
    default:
      return 'published desc';
  }
}

export function getParamsFromQuery(query: SearchDataPageQuery) {
  const releaseTypesArray = getAsArray(query.releaseType) ?? [];

  const validReleaseTypes = releaseTypesArray.filter(type =>
    isOneOf(type, Object.keys(statisticsReleaseTypes)),
  ) as ReleaseType[];

  return {
    page: getFirst(query.page),
    releaseTypes: validReleaseTypes.length > 0 ? validReleaseTypes : undefined,
    search: getFirst(query.search),
    sortBy:
      query.sortBy && isOneOf(query.sortBy, publicationSortOptions)
        ? query.sortBy
        : 'newest',
    themeIds: getAsArray(query.themeId),
  };
}
