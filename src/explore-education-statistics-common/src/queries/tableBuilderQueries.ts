import tableBuilderService from '@common/services/tableBuilderService';
import formatPretty from '@common/utils/number/formatPretty';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const tableBuilderQueries = createQueryKeys('tableBuilder', {
  getDataBlockTable(releaseVersionId: string, dataBlockParentId: string) {
    return {
      queryKey: [releaseVersionId, dataBlockParentId],
      queryFn: async () =>
        tableBuilderService.getDataBlockTableData(
          releaseVersionId,
          dataBlockParentId,
        ),
    };
  },

  getDataBlockGeoJson(
    releaseVersionId: string,
    dataBlockParentId: string,
    boundaryLevelId: number,
  ) {
    return {
      queryKey: [releaseVersionId, dataBlockParentId, boundaryLevelId],
      queryFn: async () =>
        tableBuilderService.getDataBlockGeoJson(
          releaseVersionId,
          dataBlockParentId,
          boundaryLevelId,
        ),
    };
  },

  getKeyStat(releaseVersionId: string, dataBlockParentId: string) {
    return {
      queryKey: [releaseVersionId, dataBlockParentId],
      queryFn: async () => {
        const tableData = await tableBuilderService.getDataBlockTableData(
          releaseVersionId,
          dataBlockParentId,
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
});

export default tableBuilderQueries;
