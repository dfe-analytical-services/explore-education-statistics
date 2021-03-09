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
  str.replaceAll(releaseImageApiRegex, `/api/releases/{releaseId}/images`);

/**
 * Replace release id placeholders in a {@param str}
 * with an actual {@param releaseId}.
 *
 * This allows us to create amendments without having
 * to replace all of the release ids in the content.
 */
export const replaceReleaseIdPlaceholders = (str: string, releaseId: string) =>
  str.replaceAll(
    '/api/releases/{releaseId}/images',
    `/api/releases/${releaseId}/images`,
  );
