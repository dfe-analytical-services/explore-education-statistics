import { LocationOption } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import { LocationFilter } from '../types/filters';

export default function mapLocationOptionsToFilters(
  locationOptions: Dictionary<LocationOption[]>,
): LocationFilter[] {
  const locationEntries = Object.entries(locationOptions);

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

  return locations;
}
