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
      order: category.order,
    };

    return acc;
  }, {});

  const locationEntries = Object.entries(subjectMeta.locations);
  const hasNestedLocations = locationEntries.some(([, levelOptions]) =>
    levelOptions.some(levelOption => levelOption.options),
  );

  const locations = locationEntries.flatMap(([level, levelOptions]) =>
    levelOptions.flatMap(levelOption => {
      if (levelOption.options) {
        return levelOption.options.map(
          option =>
            new LocationFilter({
              ...option,
              level,
              group: levelOption.label,
            }),
        );
      }

      return new LocationFilter({
        ...levelOption,
        level,
        // Make this location have a `group` of itself if there are
        // any nested locations at all. This is to prevent asymmetric
        // tables when the user chooses location levels that have a
        // hierarchy (e.g. local authorities) and are flat (e.g. country).
        // By setting the `group` to itself, the location should have a
        // cross span of 2 and we should get a nice symmetric table.
        group: hasNestedLocations ? levelOption.label : undefined,
      });
    }),
  );

  return {
    ...subjectMeta,
    filters,
    indicators: subjectMeta.indicators.map(
      indicator => new Indicator(indicator),
    ),
    locations,
    timePeriodRange: subjectMeta.timePeriodRange.map(
      (timePeriod, order) => new TimePeriodFilter({ ...timePeriod, order }),
    ),
  };
}
