const releaseImageApiRegex = /\/api\/releases\/[A-Za-z0-9-]+\/images/g;

/**
 * Insert release id placeholders into a {@param str}.
 *
 * This allows us to swap in different release ids
 * when we render the content. This is important for
 * amendments, as we want to avoid having to replace all
 * of the content's release ids when it's created.
 */
export const insertReleaseIdPlaceholders = (str: string) =>
  str.replaceAll(
    releaseImageApiRegex,
    `/api/releases/{releaseVersionId}/images`,
  );

/**
 * Replace release id placeholders in a {@param str}
 * with an actual {@param releaseVersionId}.
 *
 * This allows us to create amendments without having
 * to replace all of the release ids in the content.
 *
 * TODO EES-5901 - migrate all content placeholders to be "releaseVersionId"
 * and then remove the legacy "releaseId" checks below.
 */
export const replaceReleaseIdPlaceholders = (
  str: string,
  releaseVersionId: string,
) =>
  str.replaceAll(
    /\/api\/releases\/\{(releaseId|releaseVersionId)}\/images/g,
    `/api/releases/${releaseVersionId}/images`,
  );
