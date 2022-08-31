import qs, { ParsedUrlQueryInput } from 'querystring';

/**
 * Append query string to the end of a URL
 * given some query parameters.
 */
export default function appendQuery<Params extends ParsedUrlQueryInput>(
  path: string,
  queryParams: Params,
): string {
  const queryString = qs.stringify(queryParams);

  if (path.includes('?')) {
    return `${path}&${queryString}`;
  }

  return `${path}?${queryString}`;
}
