/**
 * Add Jest mocks to any functions on the given object.
 *
 * Mocks will only be applied on properties that are
 * on the actual instance (and not the prototype).
 */
function mockObject<T>(
  instance: Record<string, unknown>,
  defaultImplementation?: () => T,
): jest.Mocked<T> {
  return Object.entries(instance).reduce((acc, [key, value]) => {
    if (typeof value === 'function') {
      return {
        ...acc,
        [key]: jest.fn(defaultImplementation),
      };
    }

    return {
      ...acc,
      [key]: value,
    };
  }, {}) as jest.Mocked<T>;
}

export default mockObject;
