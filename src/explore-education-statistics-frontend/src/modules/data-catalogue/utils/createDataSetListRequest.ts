import getFirst from '@common/utils/getFirst';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { DataCataloguePageQuery } from '@frontend/modules/data-catalogue/DataCataloguePageNew';
import {
  DataSetOrderParam,
  DataSetSortParam,
  dataSetOrderOptions,
  DataSetOrderOption,
  DataSetListRequest,
} from '@frontend/services/dataSetService';
import omitBy from 'lodash/omitBy';

export default function createDataSetListRequest(
  query: DataCataloguePageQuery,
): DataSetListRequest {
  const {
    latest,
    orderBy: orderByParam,
    publicationId,
    releaseId,
    searchTerm: searchParam,
    themeId,
  } = getParamsFromQuery(query);

  const { orderBy, sort } = getOrderParams(orderByParam);

  const minSearchCharacters = 3;
  const searchTerm =
    searchParam && searchParam.length >= minSearchCharacters ? searchParam : '';

  return omitBy(
    {
      latest,
      orderBy,
      page: parseNumber(query.page) ?? 1,
      publicationId,
      releaseId,
      sort,
      searchTerm,
      themeId,
    },
    value => typeof value === 'undefined' || !value,
  );
}

function getOrderParams(orderBy: DataSetOrderOption): {
  orderBy?: DataSetOrderParam;
  sort?: DataSetSortParam;
} {
  if (orderBy === 'relevance') {
    return {
      orderBy: 'relevance',
      sort: 'desc',
    };
  }
  if (orderBy === 'title') {
    return {
      orderBy: 'title',
      sort: 'asc',
    };
  }
  if (orderBy === 'oldest') {
    return {
      orderBy: 'published',
      sort: 'asc',
    };
  }
  return {
    orderBy: 'published',
    sort: 'desc',
  };
}

export function getParamsFromQuery(query: DataCataloguePageQuery) {
  return {
    latest: getFirst(query.latest),
    orderBy:
      query.orderBy && isOneOf(query.orderBy, dataSetOrderOptions)
        ? query.orderBy
        : 'newest',
    publicationId: getFirst(query.publicationId),
    releaseId: getFirst(query.releaseId),
    searchTerm: getFirst(query.searchTerm),
    themeId: getFirst(query.themeId),
  };
}
