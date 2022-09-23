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

export default function createMapDataSetCategories(
  axis: AxisConfiguration,
  data: TableDataResult[],
  meta: FullTableMeta,
): MapDataSetCategory[] {
  return createDataSetCategories(axis, data, meta)
    .map(category => {
      return {
        ...category,
        geoJson: meta.locations.find(
          location => location.id === category.filter.id,
        )?.geoJson?.[0],
      };
    })
    .filter(category => !!category?.geoJson) as MapDataSetCategory[];
}
