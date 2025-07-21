export default function filterHighestPatchVersions(
  versions: string[],
): string[] {
  const result: string[] = [];
  const grouped: Record<string, string[]> = {};

  // Group versions by major.minor
  versions.forEach(version => {
    const parts = version.split('.');
    const key = parts.length > 2 ? `${parts[0]}.${parts[1]}` : version;
    if (!grouped[key]) {
      grouped[key] = [];
    }
    grouped[key].push(version);
  });

  Object.values(grouped).forEach(group => {
    // Find all patch versions in the group
    const patchVersions = group.filter(v => v.split('.').length === 3);
    if (patchVersions.length > 0) {
      // Keep only the highest patch version
      const highestPatch = patchVersions.reduce((max, curr) => {
        const patchNum = Number(curr.split('.')[2]);
        const maxPatchNum = Number(max.split('.')[2]);
        return patchNum > maxPatchNum ? curr : max;
      }, patchVersions[0]);
      result.push(highestPatch);
    } else {
      // No patch versions, keep the major.minor version
      result.push(group[0]);
    }
  });

  return result;
}
