/**
 * This is used as a temporary stop-gap to allow the front end to work against the
 * real API whilst the API is under active development.
 *
 * A use case for this would be for example if a back-end concept or set of data
 * does not yet exist and will not for some time.  The missing data can be supplied
 * using an implementation of this type.
 */
export type Polyfilla<T> = (original: T) => T;

export default {};
