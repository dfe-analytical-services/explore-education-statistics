import {
  CategoryFilter,
  Indicator,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataSubjectMeta } from '@common/services/tableBuilderService';
import mapLocationOptionsToFilters from './mapLocationOptionsToFilters';

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
      order: category.order,
    };

    return acc;
  }, {});

  return {
    ...subjectMeta,
    filters,
    indicators: subjectMeta.indicators.map(
      indicator => new Indicator(indicator),
    ),
    locations: mapLocationOptionsToFilters(subjectMeta.locations),
    timePeriodRange: subjectMeta.timePeriodRange.map(
      (timePeriod, order) => new TimePeriodFilter({ ...timePeriod, order }),
    ),
  };
}
