/**
 * This is used as a temporary stop-gap to allow the front end to work against the
 * real API whilst the API is under active development.
 *
 * A use case for this would be for example if a back-end concept or set of data
 * does not yet exist and will not for some time.  The missing data can be supplied
 * using an implementation of this type.
 */
export type Polyfilla<T> = (original: T) => T;

/**
 * Polyfills the data provided if using the real API (mock API data is not affected).
 *
 * @param original
 * @param polyfilla
 */
export const polyfill = <T>(original: T, polyfilla: Polyfilla<T>) => {
  if (process.env.USE_MOCK_API !== 'true') {
    return polyfilla(original);
  }
  return original;
};