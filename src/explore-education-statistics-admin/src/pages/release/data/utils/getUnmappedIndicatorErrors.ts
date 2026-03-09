import { mappableTableId } from '@admin/pages/release/data/utils/mappingTableIds';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';
import { MappableIndicator } from '@admin/pages/release/data/utils/getApiDataSetIndicatorMappings';

export default function getUnmappedIndicatorErrors(
  mappableIndicators: MappableIndicator[],
): ErrorSummaryMessage[] {
  const errors: ErrorSummaryMessage[] = [];

  const total = mappableIndicators.filter(
    indicator => indicator.mapping.type === 'AutoNone',
  ).length;

  if (total) {
    errors.push({
      id: mappableTableId('mappable-indicators'),
      message: `There ${total > 1 ? 'are' : 'is'} ${total} unmapped indicator${
        total > 1 ? 's' : ''
      }`,
    });
  }
  return errors;
}
