import { MappableFilter } from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import { FiltersMapping } from '@admin/services/apiDataSetVersionService';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';
import typedKeys from '@common/utils/object/typedKeys';
import { Dictionary } from '@common/types';
import sumBy from 'lodash/sumBy';

export default function getUnmappedFilterErrors(
  mappableFilters: Dictionary<MappableFilter[]>,
  filtersMapping: FiltersMapping,
): ErrorSummaryMessage[] {
  const errors: ErrorSummaryMessage[] = [];

  typedKeys(mappableFilters).forEach(filterKey => {
    // For MVP you can't map columns or options in unmapped columns
    if (!filtersMapping.candidates[filterKey]) {
      return;
    }

    const total = sumBy(mappableFilters[filterKey], map =>
      map.mapping.type === 'AutoNone' ? 1 : 0,
    );

    if (total) {
      const filterLabel = filtersMapping?.mappings[filterKey].source.label;

      errors.push({
        id: `mappable-${filterKey}`,
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
