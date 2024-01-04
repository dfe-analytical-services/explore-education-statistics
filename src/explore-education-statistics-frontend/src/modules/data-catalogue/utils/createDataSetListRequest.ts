import getFirst from '@common/utils/getFirst';
import parseNumber from '@common/utils/number/parseNumber';
import isOneOf from '@common/utils/type-guards/isOneOf';
import { DataCataloguePageQuery } from '@frontend/modules/data-catalogue/DataCataloguePageNew';
import {
  DataSetListRequest,
  DataSetOrderParam,
  DataSetSortParam,
  dataSetOrderOptions,
  DataSetOrderOption,
} from '@frontend/services/dataSetService';
import omitBy from 'lodash/omitBy';

export default function createDataSetListRequest(
  query: DataCataloguePageQuery,
): DataSetListRequest {
  const { searchTerm: searchParam, orderBy: orderByParam } =
    getParamsFromQuery(query);

  const { orderBy, sort } = getOrderParams(orderByParam);

  const minSearchCharacters = 3;
  const searchTerm =
    searchParam && searchParam.length >= minSearchCharacters ? searchParam : '';

  return omitBy(
    {
      orderBy,
      page: parseNumber(query.page) ?? 1,
      sort,
      searchTerm,
    },
    value => typeof value === 'undefined',
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

export function getParamsFromQuery(query: DataCataloguePageQuery): {
  searchTerm?: string;
  orderBy: DataSetOrderOption;
} {
  return {
    searchTerm: getFirst(query.searchTerm),
    orderBy:
      query.orderBy && isOneOf(query.orderBy, dataSetOrderOptions)
        ? query.orderBy
        : 'newest',
  };
}
