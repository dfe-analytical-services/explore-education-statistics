import { AxisConfiguration } from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import {
  GeoJsonFeature,
  TableDataResult,
} from '@common/services/tableBuilderService';

export interface MapDataSetCategory extends DataSetCategory {
  geoJson: GeoJsonFeature;
  filter: LocationFilter;
}

// EES-5401 pass geojson into here
export default function createMapDataSetCategories(
  axis: AxisConfiguration,
  data: TableDataResult[],
  meta: FullTableMeta,
): MapDataSetCategory[] {
  return createDataSetCategories({
    axisConfiguration: axis,
    data,
    meta,
    includeNonNumericData: true,
  })
    .map(category => {
      return {
        ...category,
        // EES-5401 instead of using meta pass in the new geojson and use it to add the geojson to the dataSetCategories
        geoJson: meta.locations.find(
          location => location.id === category.filter.id,
        )?.geoJson,
      };
    })
    .filter(category => !!category?.geoJson) as MapDataSetCategory[];
}
