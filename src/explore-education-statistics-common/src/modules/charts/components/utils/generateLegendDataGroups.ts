import {
  CustomDataGroup,
  DataClassification,
} from '@common/modules/charts/types/chart';
import subtract from '@common/utils/math/subtract';
import countDecimalPlaces from '@common/utils/number/countDecimalPlaces';
import formatPretty, {
  defaultMaxDecimalPlaces,
} from '@common/utils/number/formatPretty';
import roundNearest, {
  roundUpToNearest,
} from '@common/utils/number/roundNearest';
import { getScale } from 'color2k';
import clamp from 'lodash/clamp';
import orderBy from 'lodash/orderBy';
import times from 'lodash/times';

export interface LegendDataGroup {
  colour: string;
  decimalPlaces: number;
  min: string;
  minRaw: number;
  max: string;
  maxRaw: number;
}

interface Options {
  colour: string;
  classification?: DataClassification;
  customDataGroups?: CustomDataGroup[];
  decimalPlaces?: number;
  groups: number;
  values: number[];
  unit?: string;
}

/**
 * Creates a range of data classes expressing min/max
 * ranges and a scaled colour for each of those classes.
 *
 * Useful for choropleth maps or where data should be
 * categorised into discrete groups.
 */
export default function generateLegendDataGroups({
  colour,
  classification = 'EqualIntervals',
  customDataGroups,
  decimalPlaces: explicitDecimalPlaces,
  groups,
  values: initialValues,
  unit,
}: Options): LegendDataGroup[] {
  let implicitDecimalPlaces = 0;

  const values = orderBy(initialValues, value => {
    // Figure out the highest number of decimal places in the values if
    // no explicit decimal places have been set. We do this whilst ordering
    // to avoid having to loop over everything again (as an optimisation).
    if (Number.isFinite(value)) {
      const dp = countDecimalPlaces(value) ?? 0;

      if (dp > implicitDecimalPlaces) {
        implicitDecimalPlaces = dp;
      }
    }

    return value;
  });

  let decimalPlaces =
    explicitDecimalPlaces ??
    clamp(implicitDecimalPlaces, 0, defaultMaxDecimalPlaces);

  if (values.length === 0) {
    return [];
  }

  const max = values[values.length - 1];
  const min = values[0];
  const valueRange = subtract(max, min);

  // Only a single group to show.
  if (valueRange === 0) {
    return [
      {
        colour,
        decimalPlaces,
        min: formatPretty(min, unit, decimalPlaces),
        max: formatPretty(max, unit, decimalPlaces),
        minRaw: min,
        maxRaw: max,
      },
    ];
  }

  // Calculate the increment between values by using
  // decimal places expressed as a proportion of 1 e.g.
  // 1 decimal place is 0.1, 2 decimal places is 0.01, etc.
  let valueIncrement = 1 / 10 ** decimalPlaces;

  // Re-calculate if the increment is not small enough to prevent groups
  // overlapping one another e.g. for a range of 0.4, we need an
  // increment of 0.01 rather than 0.1 as we would get group boundaries
  // like 0.1, 0.2, 0.2, 0.3, 0.4, which would not look right.
  if (valueRange !== 0 && valueRange < valueIncrement * groups) {
    decimalPlaces += 1;
    valueIncrement = 1 / 10 ** decimalPlaces;
  }

  const colourScale = getScale('#fff', colour);

  const groupProportion = 1 / groups;
  const lastGroupIndex = groups - 1;

  switch (classification) {
    case 'EqualIntervals': {
      const groupSize = roundUpToNearest(valueRange / groups, valueIncrement);

      return times(groups, index => {
        const i = index / groups;
        // If not the last group, we want to offset the group's max
        // so that it does not overlap with the next group's min.
        const maxOffset = index < lastGroupIndex ? valueIncrement : 0;

        const groupMin = min + index * groupSize;
        const minRaw = roundNearest(groupMin, valueIncrement);
        const maxRaw = clamp(
          roundNearest(
            subtract(groupMin + groupSize, maxOffset),
            valueIncrement,
          ),
          roundNearest(max, valueIncrement),
        );

        return {
          colour: colourScale(i + groupProportion),
          decimalPlaces,
          min: formatPretty(minRaw, unit, decimalPlaces),
          max: formatPretty(maxRaw, unit, decimalPlaces),
          minRaw,
          maxRaw,
        };
      });
    }
    case 'Quantiles': {
      const limits = getQuantileLimits(values, groups);

      return limits.reduce<LegendDataGroup[]>((acc, limit, index) => {
        const nextLimit = limits[index + 1];

        if (nextLimit) {
          // Add offset to the min if not the first group so that
          // it does not overlap with the previous group's max.
          const minOffset = index > 0 ? valueIncrement : 0;

          const minRaw = roundNearest(limit + minOffset, valueIncrement);
          const maxRaw = clamp(
            roundNearest(nextLimit, valueIncrement),
            roundNearest(max, valueIncrement),
          );

          acc.push({
            colour: colourScale((index + 1) * groupProportion),
            decimalPlaces,
            min: formatPretty(minRaw, unit, decimalPlaces),
            max: formatPretty(maxRaw, unit, decimalPlaces),
            minRaw,
            maxRaw,
          });
        }

        return acc;
      }, []);
    }
    case 'Custom':
      return (
        customDataGroups?.map((group, index) => {
          const groupDecimals = Math.max(
            countDecimalPlaces(group.min) ?? 0,
            countDecimalPlaces(group.max) ?? 0,
          );
          const groupValueIncrement = 1 / 10 ** groupDecimals;

          // Adjust the raw values when there are decimals involved so values aren't missed out of the groups.
          // eg. if there's two groups: 40 - 50% and 51% - 60%
          // 50.1% should go in the 40 - 50% group
          // 50.6% should go in the 51 - 60% group
          // To ensure this happens we adjust the raw values to 49.5 - 50.4% and 50.5 - 60.4%
          const minRaw =
            implicitDecimalPlaces !== groupDecimals
              ? Number(
                  (group.min - groupValueIncrement / 2).toFixed(
                    groupDecimals + 1,
                  ),
                )
              : group.min;

          const maxRaw =
            implicitDecimalPlaces !== groupDecimals
              ? Number(
                  (
                    group.max +
                    groupValueIncrement / 2 -
                    groupValueIncrement / 10
                  ).toFixed(groupDecimals + 1),
                )
              : group.max;

          return {
            colour: colourScale((index + 1) * (1 / customDataGroups.length)),
            decimalPlaces: groupDecimals,
            min: formatPretty(group.min, unit, countDecimalPlaces(group.min)),
            max: formatPretty(group.max, unit, countDecimalPlaces(group.max)),
            minRaw,
            maxRaw,
          };
        }) ?? []
      );

    default:
      throw new Error('Invalid data classification');
  }
}

/**
 * This algorithm has been copied from Chroma.js and its behaviour matches
 * d3-scale's as well to produce quantiles that are roughly the same in size.
 *
 * It isn't clear why and how it produces the quantiles that it does (their ranges
 * can be quite irregular), but it seems to be based on math formulas for producing
 * 'weighted averages of consecutive order statistics' according to R's `quantile`
 * function: https://www.rdocumentation.org/packages/stats/versions/3.6.2/topics/quantile
 *
 * It didn't seem practical to try and create an improved version, so we've
 * just left this as a black box function for now.
 */
function getQuantileLimits(values: number[], groups: number): number[] {
  const min = values[0];
  const max = values[values.length - 1];

  const limits = [min];

  for (let i = 1; i < groups; i += 1) {
    // Not sure why/what this is conceptually, but
    // seems to be crucial to the rest of the algorithm.
    const p = ((values.length - 1) * i) / groups;
    const pb = Math.floor(p);

    if (pb === p) {
      limits.push(values[pb]);
    } else {
      const pr = p - pb;

      limits.push(values[pb] * (1 - pr) + values[pb + 1] * pr);
    }
  }

  limits.push(max);

  return limits;
}
