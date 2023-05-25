import { Filter } from '@common/modules/table-tool/types/filters';
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
 * Optimize a set of filters for the best viewing experience.
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

        return hasMultipleSubGroups &&
          filter.group &&
          filter.group !== 'Default'
          ? [new FilterGroup(filter.group), filter]
          : filter;
      })
  );
}
