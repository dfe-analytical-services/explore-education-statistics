import { MappableFilterOption } from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import { FiltersMapping } from '@admin/services/apiDataSetVersionService';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';
import { Dictionary } from '@common/types';
import kebabCase from 'lodash/kebabCase';
import sumBy from 'lodash/sumBy';

export default function getUnmappedFilterErrors(
  mappableFilters: Dictionary<MappableFilterOption[]>,
  filtersMapping: FiltersMapping,
): ErrorSummaryMessage[] {
  const errors: ErrorSummaryMessage[] = [];

  Object.keys(mappableFilters).forEach(filterKey => {
    // For MVP you can't map columns or options in unmapped columns
    if (!filtersMapping.candidates[filterKey]) {
      return;
    }

    const total = sumBy(mappableFilters[filterKey], map =>
      map.mapping.type === 'AutoNone' ? 1 : 0,
    );

    if (total) {
      const filterLabel = filtersMapping.mappings[filterKey].source.label;

      errors.push({
        id: `mappable-table-${kebabCase(filterKey)}`,
        message: `There ${
          total > 1 ? 'are' : 'is'
        } ${total} unmapped ${filterLabel} filter option${
          total > 1 ? 's' : ''
        }`,
      });
    }
  });

  return errors;
}
