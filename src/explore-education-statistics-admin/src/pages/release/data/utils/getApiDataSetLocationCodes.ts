import { LocationCandidate } from '@admin/services/apiDataSetVersionService';
import getLocationCodeEntries from '@common/utils/getLocationCodeEntries';

export default function getApiDataSetLocationCodes(
  candidate: LocationCandidate,
): string {
  const entries = getLocationCodeEntries(candidate);

  if (entries.length === 1) {
    const entry = entries[0];

    if (entry.key === 'code' || entry.key === 'id') {
      return entries[0].value;
    }
  }

  return entries.map(entry => `${entry.label}: ${entry.value}`).join(', ');
}
