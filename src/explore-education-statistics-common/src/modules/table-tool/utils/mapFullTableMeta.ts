import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataSubjectMeta } from '@common/services/tableBuilderService';

export default function mapFullTableMeta(
  subjectMeta: TableDataSubjectMeta,
): FullTableMeta {
  const filters = Object.values(subjectMeta.filters).reduce<
    FullTableMeta['filters']
  >((acc, category) => {
    acc[category.legend] = {
      name: category.name,
      options: Object.values(category.options).flatMap(filterGroup =>
        filterGroup.options.map(
          option =>
            new CategoryFilter({
              ...option,
              group: filterGroup.label,
              category: category.legend,
              isTotal: category.totalValue === option.value,
            }),
        ),
      ),
    };

    return acc;
  }, {});

  return {
    ...subjectMeta,
    filters,
    indicators: subjectMeta.indicators.map(
      indicator => new Indicator(indicator),
    ),
    locations: subjectMeta.locations.map(
      location => new LocationFilter(location),
    ),
    timePeriodRange: subjectMeta.timePeriodRange.map(
      (timePeriod, order) => new TimePeriodFilter({ ...timePeriod, order }),
    ),
  };
}
