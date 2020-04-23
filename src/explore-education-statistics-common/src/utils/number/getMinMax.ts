/**
 * Get the min and max numbers from some {@param values}.
 */
export default function getMinMax(
  values: number[],
): { min?: number; max?: number } {
  const { min, max } = values.reduce<{ min: number; max: number }>(
    (acc, value) => {
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
