import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { TableDataSubjectMeta } from '@common/services/tableBuilderService';

export default function mapFullTableMeta({
  locationsHierarchical,
  ...subjectMeta
}: TableDataSubjectMeta): FullTableMeta {
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

  const locations = Object.entries(locationsHierarchical).flatMap(
    ([level, levelOptions]) =>
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
