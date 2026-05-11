import { odata } from '@azure/search-documents';
import { releaseTypes, ReleaseType } from '@common/services/types/releaseType';
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
    releaseType,
    search: searchParam,
    sortBy,
    themeId,
  } = getParamsFromQuery(query);

  const orderBy = getSortParam(sortBy);

  const filter = buildODataFilter({
    releaseType,
    themeId,
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
  releaseType?: string[];
  themeId?: string[];
}

function buildODataFilter(filters: SearchFilters): string | undefined {
  const conditions: string[] = [];

  if (filters.releaseType?.length) {
    const joined = filters.releaseType.join('|');
    conditions.push(odata`search.in(releaseType, ${joined}, '|')`);
  }

  if (filters.themeId?.length) {
    const joined = filters.themeId.join('|');
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
    isOneOf(type, Object.keys(releaseTypes)),
  ) as ReleaseType[];

  return {
    page: getFirst(query.page),
    releaseType: validReleaseTypes.length > 0 ? validReleaseTypes : undefined,
    search: getFirst(query.search),
    sortBy:
      query.sortBy && isOneOf(query.sortBy, publicationSortOptions)
        ? query.sortBy
        : 'newest',
    themeId: getAsArray(query.themeId),
  };
}
