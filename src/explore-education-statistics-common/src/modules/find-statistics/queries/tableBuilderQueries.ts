import mapFullTable from '@common/modules/table-tool/utils/mapFullTable';
import tableBuilderService from '@common/services/tableBuilderService';
import formatPretty from '@common/utils/number/formatPretty';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const tableBuilderQueries = createQueryKeys('tableBuilder', {
  getDataBlockTable(
    releaseId: string,
    dataBlockParentId: string,
    boundaryLevelId?: number,
  ) {
    return {
      queryKey: [releaseId, dataBlockParentId, boundaryLevelId],
      queryFn: async () => {
        const [tableData, locationGeoJson] = await Promise.all([
          tableBuilderService.getDataBlockTableData(
            releaseId,
            dataBlockParentId,
          ),
          boundaryLevelId !== undefined
            ? tableBuilderService.getLocationGeoJson(
                releaseId,
                dataBlockParentId,
                boundaryLevelId,
              )
            : undefined,
        ]);

        return mapFullTable({
          ...tableData,
          subjectMeta: {
            ...tableData.subjectMeta,
            locations: locationGeoJson ?? tableData.subjectMeta.locations,
          },
        });
      },
    };
  },
  getKeyStat(releaseId: string, dataBlockParentId: string) {
    return {
      queryKey: [releaseId, dataBlockParentId],
      queryFn: async () => {
        const tableData = await tableBuilderService.getDataBlockTableData(
          releaseId,
          dataBlockParentId,
        );

        const [indicator] = tableData.subjectMeta.indicators;
        const indicatorValue = tableData.results[0]?.measures[indicator.value];

        return {
          title: indicator.label,
          value: formatPretty(
            indicatorValue,
            indicator.unit,
            indicator.decimalPlaces,
          ),
        };
      },
    };
  },
});

export default tableBuilderQueries;
