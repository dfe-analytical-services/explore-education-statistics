import { MappableLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import { mappableTableId } from '@admin/pages/release/data/utils/mappingTableIds';
import { ErrorSummaryMessage } from '@common/components/ErrorSummary';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import typedKeys from '@common/utils/object/typedKeys';

export default function getUnmappedLocationErrors(
  mappableLocations: Partial<Record<LocationLevelKey, MappableLocation[]>>,
): ErrorSummaryMessage[] {
  const errors: ErrorSummaryMessage[] = [];

  typedKeys(mappableLocations).forEach(level => {
    const total = mappableLocations[level]?.filter(
      map => map.mapping.type === 'AutoNone',
    ).length;

    if (total) {
      errors.push({
        id: mappableTableId(level),
        message: `There ${total > 1 ? 'are' : 'is'} ${total} unmapped ${
          total > 1
            ? locationLevelsMap[level]?.plural.toLowerCase() ?? level
            : locationLevelsMap[level]?.label.toLowerCase() ?? level
        }`,
      });
    }
  });

  return errors;
}
