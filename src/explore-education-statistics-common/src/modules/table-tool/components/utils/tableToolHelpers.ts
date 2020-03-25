import tableBuilderService, {
  PublicationSubjectMeta,
  TableDataQuery,
} from '@common/modules/table-tool/services/tableBuilderService';
import { CategoryFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/tableHeaders';
import { DataBlockMetadata } from '@common/services/dataBlockService';
import { Dictionary } from '@common/types';
import mapValuesWithKeys from '@common/utils/mapValuesWithKeys';
import sortBy from 'lodash/sortBy';

export const transformTableMetaFiltersToCategoryFilters = (
  filters: DataBlockMetadata['filters'] | FullTableMeta['filters'],
): Dictionary<CategoryFilter[]> => {
  return mapValuesWithKeys(filters, (filterKey, filterValue) =>
    sortBy(Object.values(filterValue.options), o => o.label)
      .flatMap(options => {
        return options.options.map(option => ({
          ...option,
          filterGroup: options.label,
        }));
      })
      .map(
        filter =>
          new CategoryFilter(filter, filter.value === filterValue.totalValue),
      ),
  );
};

export const getDefaultSubjectMeta = (): PublicationSubjectMeta => ({
  timePeriod: {
    hint: '',
    legend: '',
    options: [],
  },
  locations: {},
  indicators: {},
  filters: {},
});

export const executeTableQuery = async (query: TableDataQuery) => {
  const rawTableData = await tableBuilderService.getTableData(query);

  const table = mapFullTable(rawTableData);
  const tableHeaders = getDefaultTableHeaderConfig(table.subjectMeta);

  return {
    table,
    tableHeaders,
  };
};
