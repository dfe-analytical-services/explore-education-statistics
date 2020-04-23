import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import tableBuilderService, {
  PublicationSubjectMeta,
  TableDataQuery,
} from '@common/services/tableBuilderService';

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
