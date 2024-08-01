import { MappableFilter } from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import { FiltersMapping } from '@admin/services/apiDataSetVersionService';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';
import typedKeys from '@common/utils/object/typedKeys';
import { Dictionary } from '@common/types';

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

    const total = mappableFilters[filterKey]?.filter(
      map => map.mapping.type === 'AutoNone',
    ).length;

    if (total) {
      errors.push({
        id: `mappable-${filterKey}`,
        message: `There ${
          total > 1 ? 'are' : 'is'
        } ${total} unmapped ${filtersMapping?.mappings[filterKey].source
          .label} filter option${total > 1 ? 's' : ''}`,
      });
    }
  });

  return errors;
}
