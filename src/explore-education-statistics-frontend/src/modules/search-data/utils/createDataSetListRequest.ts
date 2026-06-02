import { odata } from '@azure/search-documents';
import {
  releaseTypes as statisticsReleaseTypes,
  ReleaseType,
} from '@common/services/types/releaseType';
import getAsArray from '@common/utils/getAsArray';
import getFirst from '@common/utils/getFirst';
import locationLevelsMap, {
  GeographicLevelCode,
} from '@common/utils/locationLevelsMap';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import {
  PublicationSortOption,
  publicationSortOptions,
} from '@frontend/modules/find-statistics/utils/publicationSortOptions';
import { SearchDataPageQuery } from '@frontend/modules/search-data/SearchDataPage';
import { AzureDataSetListRequest } from '@frontend/services/azureDataSetService';
import { AzureOrderByParam } from '@frontend/services/azurePublicationService';
import { DataSetType } from '@frontend/services/dataSetFileService';
import omitBy from 'lodash/omitBy';

export default function createDataSetListRequest(
  query: SearchDataPageQuery,
): AzureDataSetListRequest {
  const {
    dataSetType,
    geographicLevels,
    latestDataOnly,
    publicationIds,
    releaseTypes,
    search: searchParam,
    sortBy,
    themeIds,
  } = getParamsFromQuery(query);

  const orderBy = getSortParam(sortBy);

  const filter = buildODataFilter({
    dataSetType,
    geographicLevels,
    latestDataOnly,
    publicationIds,
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
  geographicLevels?: string[];
  dataSetType: string;
  latestDataOnly?: boolean;
  publicationIds?: string[];
  releaseTypes?: string[];
  themeIds?: string[];
}

function buildODataFilter(filters: SearchFilters): string | undefined {
  const conditions: string[] = [];
  const themeAndPubConditions: string[] = [];

  if (filters.themeIds?.length) {
    const joined = filters.themeIds.join('|');
    themeAndPubConditions.push(odata`search.in(themeId, ${joined}, '|')`);
  }

  if (filters.publicationIds?.length) {
    const joined = filters.publicationIds.join('|');
    themeAndPubConditions.push(odata`search.in(publicationId, ${joined}, '|')`);
  }

  if (themeAndPubConditions.length > 0) {
    // If both theme and publication ids, we want to match either, so join them with 'or' and wrap in parentheses.
    // If only one exists, just use it as is.
    const combinedGroup =
      themeAndPubConditions.length > 1
        ? `(${themeAndPubConditions.join(' or ')})`
        : themeAndPubConditions[0];

    conditions.push(combinedGroup);
  }

  if (filters.releaseTypes?.length) {
    const joined = filters.releaseTypes.join('|');
    conditions.push(odata`search.in(releaseType, ${joined}, '|')`);
  }

  if (filters.geographicLevels?.length) {
    const joined = filters.geographicLevels.join('|');
    conditions.push(
      odata`geographicLevels/any(g: search.in(g, ${joined}, '|'))`,
    );
  }

  if (filters.latestDataOnly) {
    conditions.push(odata`latestData eq true`);
  }

  if (filters.dataSetType === 'api') {
    // conditions.push(odata`api eq ${filters.hasApiDataSet}`);
    // "ne" stands for "not equal".
    conditions.push(`api/id ne null and api/id ne ''`);
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

  const geographicLevelsArray = getAsArray(query.geographicLevel) ?? [];

  const validGeographicLevels = geographicLevelsArray.filter(type =>
    isOneOf(
      type,
      Object.values(locationLevelsMap).map(level => level.code),
    ),
  ) as GeographicLevelCode[];

  return {
    dataSetType: query.dataSetType === 'api' ? 'api' : ('all' as DataSetType),
    geographicLevels:
      validGeographicLevels.length > 0 ? validGeographicLevels : undefined,
    latestDataOnly: !query.latestDataOnly || query.latestDataOnly !== 'false',
    page: getFirst(query.page),
    publicationIds: getAsArray(query.publicationId),
    releaseTypes: validReleaseTypes.length > 0 ? validReleaseTypes : undefined,
    search: getFirst(query.search),
    sortBy:
      query.sortBy && isOneOf(query.sortBy, publicationSortOptions)
        ? query.sortBy
        : 'newest',
    themeIds: getAsArray(query.themeId),
  };
}
