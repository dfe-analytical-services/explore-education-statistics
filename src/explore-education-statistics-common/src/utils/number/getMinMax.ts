interface MinMax {
  min?: number;
  max?: number;
}

/**
 * Get the min and max numbers from some {@param values}.
 *
 * A custom {@param iteratee} function can be provided if we
 * wish to modify the values as we iterate over them.
 */
export default function getMinMax<TValue>(
  values: TValue[],
  iteratee?: (value: TValue) => unknown,
): MinMax {
  const { min, max } = values.reduce<{ min: number; max: number }>(
    (acc, rawValue) => {
      const value = iteratee?.(rawValue) ?? rawValue;

      if (typeof value !== 'number') {
        return acc;
      }

      if (value < acc.min) {
        acc.min = value;
      }

      if (value > acc.max) {
        acc.max = value;
      }

      return acc;
    },
    {
      min: Number.POSITIVE_INFINITY,
      max: Number.NEGATIVE_INFINITY,
    },
  );

  return {
    min: Number.isFinite(min) ? min : undefined,
    max: Number.isFinite(max) ? max : undefined,
  };
}
