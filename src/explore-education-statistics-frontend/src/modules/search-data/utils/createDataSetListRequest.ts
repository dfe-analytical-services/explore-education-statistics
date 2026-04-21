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
import { SearchDataPageQuery } from '@frontend/modules/search-data/SearchDataPage';
import { AzureDataSetListRequest } from '@frontend/services/azureDataSetService';
import { AzureOrderByParam } from '@frontend/services/azurePublicationService';
import omitBy from 'lodash/omitBy';

export default function createDataSetListRequest(
  query: FindStatisticsPageQuery,
): AzureDataSetListRequest {
  const {
    dataSetType,
    latestDataOnly,
    releaseType,
    search: searchParam,
    sortBy,
    themeId,
  } = getParamsFromQuery(query);

  const orderBy = getSortParam(sortBy);

  // TOOD-7072 finish converting filters into arrays
  const filter = buildODataFilter({
    dataSetType,
    latestDataOnly,
    releaseType: releaseType ? [releaseType] : undefined,
    themeId: themeId ? [themeId.toUpperCase()] : undefined,
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
  geographicLevels?: string[];
  dataSetType: string;
  latestDataOnly?: boolean;
  releaseId?: string[];
  releaseType?: string[];
  themeId?: string[];
}

// TOOD-7072 add tests
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

  if (filters.geographicLevels?.length) {
    const joined = filters.geographicLevels.join('|');
    conditions.push(odata`search.in(geographicLevels, ${joined}, '|')`);
  }

  if (filters.releaseId?.length) {
    const joined = filters.releaseId.join('|');
    conditions.push(odata`search.in(releaseId, ${joined}, '|')`);
  }

  if (filters.latestDataOnly) {
    conditions.push(odata`latestData eq true`);
  }

  if (filters.dataSetType === 'api') {
    // conditions.push(odata`api eq ${filters.hasApiDataSet}`);
    // "ne" stands for "not equal".
    conditions.push(`api/id ne null and api/id ne ''`);
  }

  // 5. Combine everything or return undefined if no filters were provided
  return conditions.length > 0 ? conditions.join(' and ') : undefined;
}

// export function createDataSetSuggestRequest(
//   query: FindStatisticsPageQuery,
//   searchTerm: string,
// ): AzurePublicationListRequest {
//   const { releaseType, themeId } = getParamsFromQuery(query);

//   let filter: string | undefined;
//   if (releaseType && themeId) {
//     filter = odata`releaseType eq ${releaseType} and themeId eq ${themeId}`;
//   } else if (releaseType) {
//     filter = odata`releaseType eq ${releaseType}`;
//   } else if (themeId) {
//     filter = odata`themeId eq ${themeId}`;
//   }

//   return omitBy(
//     {
//       filter,
//       releaseType,
//       search: searchTerm,
//       themeId,
//     },
//     value => typeof value === 'undefined',
//   );
// }

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
  return {
    dataSetType: query.dataSetType === 'api' ? 'api' : 'all',
    latestDataOnly: !query.latestDataOnly || query.latestDataOnly !== 'false',
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
