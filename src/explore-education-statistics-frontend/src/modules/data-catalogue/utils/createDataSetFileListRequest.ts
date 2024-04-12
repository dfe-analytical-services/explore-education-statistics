import { SortDirection } from '@common/services/types/sort';
import getFirst from '@common/utils/getFirst';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { DataCataloguePageQuery } from '@frontend/modules/data-catalogue/DataCataloguePageNew';
import {
  DataSetFileSortParam,
  dataSetFileSortOptions,
  DataSetFileSortOption,
  DataSetFileListRequest,
} from '@frontend/services/dataSetFileService';
import omitBy from 'lodash/omitBy';

export default function createDataSetFileListRequest(
  query: DataCataloguePageQuery,
): DataSetFileListRequest {
  const {
    latestOnly,
    sortBy,
    publicationId,
    releaseId,
    searchTerm: searchParam,
    themeId,
  } = getParamsFromQuery(query);

  const { sort, sortDirection } = getSortParams(sortBy);

  const minSearchCharacters = 3;
  const searchTerm =
    searchParam && searchParam.length >= minSearchCharacters ? searchParam : '';

  return omitBy(
    {
      latestOnly,
      page: parseNumber(query.page) ?? 1,
      publicationId,
      releaseId,
      sort,
      sortDirection,
      searchTerm,
      themeId,
    },
    value => typeof value === 'undefined' || !value,
  );
}

function getSortParams(orderBy: DataSetFileSortOption): {
  sort?: DataSetFileSortParam;
  sortDirection?: SortDirection;
} {
  if (orderBy === 'relevance') {
    return {
      sort: 'relevance',
      sortDirection: 'Desc',
    };
  }
  if (orderBy === 'title') {
    return {
      sort: 'title',
      sortDirection: 'Asc',
    };
  }
  if (orderBy === 'oldest') {
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

export function getParamsFromQuery(query: DataCataloguePageQuery) {
  return {
    latestOnly: getFirst(query.latestOnly),
    sortBy:
      query.sortBy && isOneOf(query.sortBy, dataSetFileSortOptions)
        ? query.sortBy
        : 'newest',
    publicationId: getFirst(query.publicationId),
    releaseId: getFirst(query.releaseId),
    searchTerm: getFirst(query.searchTerm),
    themeId: getFirst(query.themeId),
  };
}
