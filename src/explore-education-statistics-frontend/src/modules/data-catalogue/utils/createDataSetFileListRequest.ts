import { SortDirection } from '@common/services/types/sort';
import getFirst from '@common/utils/getFirst';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { DataCataloguePageQuery } from '@frontend/modules/data-catalogue/DataCataloguePage';
import {
  DataSetFileSortParam,
  DataSetFileListRequest,
} from '@frontend/services/dataSetFileService';
import {
  dataSetFileSortOptions,
  DataSetFileSortOption,
} from '@frontend/modules/data-catalogue/utils/dataSetFileSortOptions';

import omitBy from 'lodash/omitBy';

export default function createDataSetFileListRequest(
  query: DataCataloguePageQuery,
): DataSetFileListRequest {
  const {
    dataSetType,
    latestOnly,
    sortBy,
    publicationId,
    releaseId,
    geographicLevel,
    searchTerm: searchParam,
    themeId,
  } = getParamsFromQuery(query);

  const { sort, sortDirection } = getSortParams({
    sortBy,
    filterByRelease: !!releaseId,
  });

  const minSearchCharacters = 3;
  const searchTerm =
    searchParam && searchParam.length >= minSearchCharacters ? searchParam : '';

  return omitBy(
    {
      dataSetType,
      latestOnly,
      page: parseNumber(query.page) ?? 1,
      publicationId,
      releaseId,
      geographicLevel,
      sort,
      sortDirection,
      searchTerm,
      themeId,
    },
    value => typeof value === 'undefined' || !value,
  );
}

function getSortParams({
  filterByRelease,
  sortBy,
}: {
  filterByRelease?: boolean;
  sortBy: DataSetFileSortOption;
}): {
  sort?: DataSetFileSortParam;
  sortDirection?: SortDirection;
} {
  if (filterByRelease) {
    return {
      sort: 'natural',
    };
  }
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

export function getParamsFromQuery(query: DataCataloguePageQuery) {
  return {
    dataSetType: getFirst(query.dataSetType),
    latestOnly: getFirst(query.latestOnly),
    sortBy:
      query.sortBy && isOneOf(query.sortBy, dataSetFileSortOptions)
        ? query.sortBy
        : 'newest',
    publicationId: getFirst(query.publicationId),
    releaseId: getFirst(query.releaseVersionId),
    geographicLevel: getFirst(query.geographicLevel),
    searchTerm: getFirst(query.searchTerm),
    themeId: getFirst(query.themeId),
  };
}
