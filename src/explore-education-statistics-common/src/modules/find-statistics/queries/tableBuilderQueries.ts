import tableBuilderService from '@common/services/tableBuilderService';
import { createQueryKeys } from '@lukemorales/query-key-factory';

const tableBuilderQueries = createQueryKeys('tableBuilder', {
  getGeoJson(
    releaseId: string,
    dataBlockParentId: string,
    boundaryLevelId: number,
  ) {
    return {
      queryKey: ['geoJson', releaseId, dataBlockParentId, boundaryLevelId],
      queryFn: () =>
        tableBuilderService.getLocationGeoJson(
          releaseId,
          dataBlockParentId,
          boundaryLevelId,
        ),
    };
  },
});

export default tableBuilderQueries;
