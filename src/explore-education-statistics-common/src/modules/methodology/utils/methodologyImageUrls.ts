const methodologyImageApiRegex = /\/api\/methodologies\/[A-Za-z0-9-]+\/images/g;

/**
 * Insert methodology id placeholders into a {@param str}.
 *
 * This allows us to swap in different methodology ids
 * when we render the content. This is important for
 * amendments, as we want to avoid having to replace all
 * of the content's methodology ids when it's created.
 */
export const insertMethodologyIdPlaceholders = (str: string) =>
  str.replaceAll(
    methodologyImageApiRegex,
    `/api/methodologies/{methodologyId}/images`,
  );

/**
 * Replace methodology id placeholders in a {@param str}
 * with an actual {@param methodologyId}.
 *
 * This allows us to create amendments without having
 * to replace all of the methodology ids in the content.
 */
export const replaceMethodologyIdPlaceholders = (
  str: string,
  methodologyId: string,
) =>
  str.replaceAll(
    '/api/methodologies/{methodologyId}/images',
    `/api/methodologies/${methodologyId}/images`,
  );
