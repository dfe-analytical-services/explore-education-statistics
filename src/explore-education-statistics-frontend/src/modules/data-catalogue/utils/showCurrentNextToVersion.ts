export default function showCurrentNextToVersion(
  version: string,
  latestPatchLookup: Record<string, number> | undefined,
): boolean {
  const versionParts = version.split('.');
  let isCurrent = false;
  if (versionParts.length === 3) {
    const patch = Number(versionParts[2]);
    const key = versionParts.slice(0, 2).join('.');
    if (patch === latestPatchLookup?.[key]) {
      isCurrent = true;
    }
  } else {
    // `latestPatchLookup` only contains patch versions, so in this else, we treat everything not in `latestPatchLookup` as current.
    isCurrent = latestPatchLookup?.[version] === undefined;
  }
  return isCurrent;
}
