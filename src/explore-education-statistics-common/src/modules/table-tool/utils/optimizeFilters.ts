import {
  Filter,
  LocationFilter,
} from '@common/modules/table-tool/types/filters';
import last from 'lodash/last';

export class FilterGroup extends Filter {
  constructor(label: string) {
    super({
      label,
      value: label,
    });
  }
}

/**
 * Function to optimize a set of filters for the best viewing experience.
 *
 * Typically we add or remove filters depending on whether they are
 * actually needed for the user to understand the table.
 */
export default function optimizeFilters(
  filters: Filter[],
  headerConfig: Filter[][],
): Filter[] {
  const rowColFilters = last(headerConfig);

  let optimizedFilters = filters;

  // There is only one or zero row/col filter header, and we want to avoid
  // having only a single header as this can often get repeated many times
  // for tables with multiple filter groups.
  // We should try and display these filter group headers instead of the
  // row/col header as they should provide more useful information to the user.
  if (rowColFilters && rowColFilters.length <= 1) {
    optimizedFilters = filters.length > 1 ? filters.slice(0, -1) : filters;
  }

  return (
    optimizedFilters
      // Add additional filter sub groups
      // to our filters if required.
      .flatMap((filter, index) => {
        const firstSubGroup = headerConfig[index][0].group;

        // Don't bother showing a single subgroup as this adds
        // additional groups to a potentially crowded table.
        const hasMultipleSubGroups = headerConfig[index].some(
          header => header.group !== firstSubGroup,
        );

        // The location hierarchy expects grouping, for example the LAD attribute
        // is grouped by Region. However, the data screener
        // (https://rsconnect/rsc/dfe-published-data-qa/) does not require the data
        // to have all attributes of the hierarchy. When this data is missing the
        // backend returns an empty string which causes table layout problems as
        // there is a missing header cell where the group would have been.
        // To fix this, an empty header for the missing group data is added.
        // When the table is rendered these empty header cells are converted to
        // <td> as empty <th>'s cause accessibility problems.
        const isMissingLocationGroup =
          filter instanceof LocationFilter && filter.group === '';

        return hasMultipleSubGroups &&
          ((filter.group && filter.group !== 'Default') ||
            isMissingLocationGroup)
          ? [new FilterGroup(filter.group), filter]
          : filter;
      })
  );
}
