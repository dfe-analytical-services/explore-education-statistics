/**
 * What the current user is allowed to do in the data uploads section.
 *
 * These are decided once by `ReleaseDataUploadsSection` and passed down, rather
 * than each row reading `useAuthContext` for itself. They are user- and
 * release-scoped, so the answer is the same for every row.
 *
 * Per-row conditions — import status, whether a file has an API data set, which
 * screener warnings have been acknowledged — stay in the row that owns the data
 * they derive from.
 */
export default interface DataUploadsPermissions {
  /** Whether the release is still editable at all. */
  canUpdateRelease: boolean;
  /** BAU users may import a data set the screener rejected. */
  canOverrideScreenerResult: boolean;
  /** Whether the user could remove a linked API data set themselves. */
  canManagePublicApiDataSets: boolean;
}
