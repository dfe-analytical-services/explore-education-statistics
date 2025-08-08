import { ApiDataSetVersionChanges } from '@common/services/types/apiDataSetChanges';

/**
 * Sorts an array of version changes in descending order (newer versions first).
 * @param versions - Array of version changes to sort
 * @returns New sorted array of version changes
 */
export default function sortVersionChanges(
  versions: ApiDataSetVersionChanges[],
): ApiDataSetVersionChanges[] {
  return [...versions].sort((a, b) => {
    const [majorA, minorA, patchA] = a.versionNumber.split('.').map(Number);
    const [majorB, minorB, patchB] = b.versionNumber.split('.').map(Number);

    if (majorA !== majorB) return majorB - majorA;
    if (minorA !== minorB) return minorB - minorA;
    return (patchB || 0) - (patchA || 0);
  });
}
