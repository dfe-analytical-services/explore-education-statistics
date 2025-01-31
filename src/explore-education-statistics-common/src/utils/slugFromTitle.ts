/**
 * Converts a title string into a slug.
 *
 * Duplicates NamingUtils#SlugFromTitle in the backend
 */
export default function slugFromTitle(title: string) {
  return title.replace(/\W+/g, ' ').trim().toLowerCase().replace(/\s+/g, '-');
}
