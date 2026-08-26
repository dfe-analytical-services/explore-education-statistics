import getDefaultTableHeaderConfig from '@common/modules/table-tool/utils/getDefaultTableHeadersConfig';
import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderService, {
  FullTableQuery,
} from '@common/services/tableBuilderService';
import formatPretty from '@common/utils/number/formatPretty';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const tableBuilderQueries = createQueryKeys('tableBuilder', {
  getDataBlockTable(releaseVersionId: string, dataBlockId: string) {
    return {
      queryKey: [releaseVersionId, dataBlockId],
      queryFn: async () =>
        tableBuilderService.getDataBlockTableData(
          releaseVersionId,
          dataBlockId,
        ),
    };
  },

  getDataBlockGeoJson(
    releaseVersionId: string,
    dataBlockId: string,
    boundaryLevelId: number,
  ) {
    return {
      queryKey: [releaseVersionId, dataBlockId, boundaryLevelId],
      queryFn: async () =>
        tableBuilderService.getDataBlockGeoJson(
          releaseVersionId,
          dataBlockId,
          boundaryLevelId,
        ),
    };
  },

  getKeyStat(releaseVersionId: string, dataBlockId: string) {
    return {
      queryKey: [releaseVersionId, dataBlockId],
      queryFn: async () => {
        const tableData = await tableBuilderService.getDataBlockTableData(
          releaseVersionId,
          dataBlockId,
        );

        const [indicator] = tableData.subjectMeta.indicators;
        const indicatorValue = tableData.results[0]?.measures[indicator.value];

        return {
          title: indicator.label,
          value:
            indicator.unit === 'string'
              ? indicatorValue
              : formatPretty(
                  indicatorValue,
                  indicator.unit,
                  indicator.decimalPlaces,
                ),
        };
      },
    };
  },

  getFullTable(query: FullTableQuery, releaseVersionId?: string) {
    return {
      queryKey: ['fullTable', query, releaseVersionId],
      queryFn: async () => {
        const tableData = await tableBuilderService.getTableData(
          query,
          releaseVersionId,
        );
        if (!tableData.results.length || !tableData.subjectMeta) {
          throw new Error(
            'No data available for the options selected. Please try again with different options.',
          );
        }

        const tableMapped = mapFullTable(tableData);
        const tableHeadersConfig = getDefaultTableHeaderConfig(tableMapped);
        return {
          table: tableMapped,
          tableHeaders: tableHeadersConfig,
        };
      },
    };
  },
});

export default tableBuilderQueries;
