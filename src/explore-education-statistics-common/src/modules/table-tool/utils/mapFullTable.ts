import { TableDataResponse } from '@common/services/tableBuilderService';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';

export default function mapFullTable(
  unmappedFullTable: TableDataResponse,
): FullTable {
  const subjectMeta = unmappedFullTable.subjectMeta || {
    indicators: [],
    locations: [],
    timePeriodRange: [],
  };

  const filters = Object.values(unmappedFullTable.subjectMeta.filters).reduce<
    Dictionary<CategoryFilter[]>
  >((acc, category) => {
    acc[category.legend] = Object.values(category.options).flatMap(
      filterGroup =>
        filterGroup.options.map(
          option =>
            new CategoryFilter({
              ...option,
              group: filterGroup.label,
              category: category.legend,
              isTotal: category.totalValue === option.value,
            }),
        ),
    );

    return acc;
  }, {});

  return {
    ...unmappedFullTable,
    subjectMeta: {
      ...unmappedFullTable.subjectMeta,
      filters,
      indicators: subjectMeta.indicators.map(
        indicator => new Indicator(indicator),
      ),
      locations: subjectMeta.locations.map(
        location => new LocationFilter(location),
      ),
      timePeriodRange: subjectMeta.timePeriodRange.map(
        timePeriod => new TimePeriodFilter(timePeriod),
      ),
    },
  };
}
