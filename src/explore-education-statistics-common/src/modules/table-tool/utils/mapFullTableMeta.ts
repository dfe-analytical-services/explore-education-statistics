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

  /**
   * The location hierarchy expects grouping, for example the LAD attribute
   * is grouped by Region. However, the data screener
   * (https://rsconnect/rsc/dfe-published-data-qa/) does not require the data
   * to have all attributes of the hierarchy. When this data is missing the
   * backend returns an empty string for the group label. This can cause table
   * layout problems so we need to not group the locations when this is the
   * case.
   */
  const addLocationGroup = locationEntries.some(
    ([, levelOptions]) =>
      levelOptions.some(levelOption => levelOption.options) &&
      levelOptions.every(levelOption => levelOption.label !== ''),
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
        group: addLocationGroup ? levelOption.label : undefined,
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
