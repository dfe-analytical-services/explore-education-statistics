import kebabCase from 'lodash/kebabCase';

/**
 * Generates an ID from a heading with an optional prefix
 */
export default function generateIdFromHeading(
  heading: string,
  idPrefix = 'section',
): string {
  return `${idPrefix}-${kebabCase(heading)}`;
}
