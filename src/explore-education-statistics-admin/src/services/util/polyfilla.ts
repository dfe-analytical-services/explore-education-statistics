export type Polyfilla<T> = (original: T) => T;

export const polyfill = <T>(original: T, polyfilla: Polyfilla<T>) => {
  if (process.env.USE_MOCK_API !== 'true') {
    return polyfilla(original);
  }
  return original;
};