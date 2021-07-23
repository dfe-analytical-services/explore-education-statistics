import { FormValues } from '@admin/pages/release/datablocks/components/chart/ChartDataSetsConfiguration';
import { DataSet } from '@common/modules/charts/types/dataSet';
import {
  Indicator,
  LocationFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import cartesian from '@common/utils/cartesian';

interface Options {
  filters: FullTableMeta['filters'];
  indicatorOptions: Indicator[];
  values: FormValues;
}

export default function getSelectedDataSets({
  filters,
  indicatorOptions,
  values,
}: Options): DataSet[] {
  const allOptionsSelectedFilters: string[][] = [];
  const singleOptionSelectedFilters: string[] = [];

  Object.entries(values.filters).forEach(([filterType, filter]) => {
    if (!filter) {
      allOptionsSelectedFilters.push(
        filters[filterType].options.map(option => option.value),
      );
    } else {
      singleOptionSelectedFilters.push(filter);
    }
  });

  const allFilters = allOptionsSelectedFilters.length
    ? cartesian(...allOptionsSelectedFilters).map(filter => {
        return [...filter, ...singleOptionSelectedFilters];
      })
    : [singleOptionSelectedFilters];

  const selectedIndicators = values.indicator
    ? [values.indicator]
    : indicatorOptions.map(indicator => indicator.value);

  // Convert empty strings from form values to undefined
  const timePeriod: DataSet['timePeriod'] = values.timePeriod
    ? values.timePeriod
    : undefined;

  const location: DataSet['location'] = values.location
    ? LocationFilter.parseCompositeId(values.location)
    : undefined;

  if (
    !allOptionsSelectedFilters.length &&
    !singleOptionSelectedFilters.length
  ) {
    return selectedIndicators.map(indicator => {
      return {
        filters: [],
        indicator,
        location,
        timePeriod,
      };
    });
  }

  return allFilters
    .filter(items => items.length > 0)
    .flatMap(items =>
      selectedIndicators.map(indicator => {
        return {
          filters: items,
          indicator,
          location,
          timePeriod,
        };
      }),
    );
}
