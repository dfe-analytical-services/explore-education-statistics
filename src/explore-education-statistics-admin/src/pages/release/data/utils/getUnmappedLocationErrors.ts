import { UnmappedAndManuallyMappedLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';
import { Dictionary } from '@common/types';
import locationLevelsMap from '@common/utils/locationLevelsMap';
import camelCase from 'lodash/camelCase';

export default function getUnmappedLocationErrors(
  unmappedAndManuallyMappedLocations: Dictionary<
    UnmappedAndManuallyMappedLocation[]
  >,
): ErrorSummaryMessage[] {
  const errors: ErrorSummaryMessage[] = [];

  Object.keys(unmappedAndManuallyMappedLocations).forEach(level => {
    const total = unmappedAndManuallyMappedLocations[level].filter(
      map => map.mapping.type === 'AutoNone',
    ).length;
    if (total) {
      errors.push({
        id: `unmapped-${level}`,
        message: `There ${total > 1 ? 'are' : 'is'} ${total} unmapped ${
          total > 1
            ? // TODO remove camelCase when levels are camelCase in BE
              locationLevelsMap[camelCase(level)]?.plural.toLowerCase() ?? level
            : locationLevelsMap[camelCase(level)]?.label.toLowerCase() ?? level
        }`,
      });
    }
  });

  return errors;
}
