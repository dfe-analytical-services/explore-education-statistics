import { Dictionary } from '@common/types';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
} from '@common/modules/table-tool/types/filters';
import { DataSet } from '@common/modules/charts/types/dataSet';
import { FormValues } from '@admin/pages/release/datablocks/components/chart/ChartDataSetsConfiguration';
import cartesian from '@common/utils/cartesian';

interface Props {
  filters: Dictionary<{
    name: string;
    options: CategoryFilter[];
  }>;
  indicatorOptions: Indicator[];
  values: FormValues;
}

const formatSelectedDataSets = ({
  filters,
  indicatorOptions,
  values,
}: Props) => {
  const allOptionsSelectedFilters: string[][] = [];
  const singleOptionSelectedFilters: string[] = [];

  Object.keys(values.filters).forEach(filterType => {
    if (!values.filters[filterType]) {
      allOptionsSelectedFilters.push(
        filters[filterType].options.map(option => {
          return option.value;
        }),
      );
    } else {
      singleOptionSelectedFilters.push(values.filters[filterType]);
    }
  });

  const allFilters = allOptionsSelectedFilters.length
    ? cartesian(...allOptionsSelectedFilters).map(filter => {
        return [...filter, ...singleOptionSelectedFilters];
      })
    : [singleOptionSelectedFilters];

  const selectedDataSets: DataSet[] = [];
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
    selectedIndicators.forEach(option => {
      selectedDataSets.push({
        filters: [],
        indicator: option,
        location,
        timePeriod,
      });
    });
  } else {
    allFilters.forEach(items => {
      if (items.length) {
        selectedIndicators.forEach(option => {
          selectedDataSets.push({
            filters: items,
            indicator: option,
            location,
            timePeriod,
          });
        });
      }
    });
  }

  return selectedDataSets;
};
export default formatSelectedDataSets;
