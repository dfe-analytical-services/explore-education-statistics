import {
  AxisConfiguration,
  TickConfig,
} from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import getMinMax from '@common/utils/number/getMinMax';
import parseNumber from '@common/utils/number/parseNumber';
import omit from 'lodash/omit';
import { AxisDomainItem } from 'recharts/types/util/types';

export interface DomainTicks {
  domain?: [AxisDomainItem, AxisDomainItem];
  ticks?: (string | number)[];
}

/**
 * Return a nice rounded number for the min and max axis values:
 * Max:
 * - round up to one significant figure and return the next 'nice' value (even or starting with 5), e.g.
 *   - 222 rounded up to next s.f. is 300, return 400
 *   - 367 rounded up to next s.f. is 400, return 400
 *   - 429 rounded up to next s.f. is 500, return 500
 * Min:
 * - if it's positive return 0
 * - if it's negative round down to one significant figure and return the next 'nice' value (even or starting with 5)
 */
function getNiceMinMaxValue(initialValue: number, isMin = false): number {
  if (initialValue === 0 || (isMin && initialValue > 0)) {
    return 0;
  }

  const valueIsLessThanOne = Math.abs(initialValue) < 1;
  const value = valueIsLessThanOne ? initialValue * 100 : initialValue;
  const numberOf0s = 10 ** Math.floor(Math.log10(Math.abs(value)));

  // round up for max and down for min
  const roundedAndReducedToLessThan10 = !isMin
    ? Math.ceil(value / numberOf0s)
    : Math.floor(value / numberOf0s);

  // adjust up by 1 for max and down for min
  const adjustBy = isMin ? -1 : 1;

  const rounded =
    roundedAndReducedToLessThan10 % 2 && roundedAndReducedToLessThan10 % 5
      ? (roundedAndReducedToLessThan10 + adjustBy) * numberOf0s
      : roundedAndReducedToLessThan10 * numberOf0s;
  return valueIsLessThanOne ? rounded / 100 : rounded;
}

function calculateMinorTicks(
  min: number,
  max: number,
  spacing = 5,
  config?: TickConfig,
): number[] | undefined {
  if (config === 'startEnd') {
    return [min, max];
  }

  let spacingValue = +spacing;

  if (spacingValue <= 0) {
    spacingValue = 1.0;
  }

  if (config === 'custom') {
    const minimumSpacingValue = getNiceMinMaxValue(
      Math.floor((Number(max) - Number(min)) / 100),
    );

    if (spacingValue < minimumSpacingValue) {
      spacingValue = minimumSpacingValue;
    }

    const result = [];

    let [start, end] = [min, max];

    if (start > end) {
      [start, end] = [end, start];
    }

    for (let i = start; i < end; i += spacingValue) {
      result.push(parseFloat(i.toPrecision(10)));
    }

    result.push(max);

    return result;
  }

  return undefined;
}

function calculateMajorTicks(
  config: TickConfig | undefined,
  categories: string[],
  min: number,
  max: number,
  spacing = 1,
): string[] | undefined {
  if (config === 'startEnd') {
    return [categories[min], categories[max]];
  }

  if (config === 'custom') {
    let spacingValue = +spacing;

    if (spacingValue <= 0) {
      spacingValue = 1.0;
    }

    const result = [];

    let [start, end] = [min, max];

    if (start > end) {
      [start, end] = [end, start];
    }

    for (let i = start; i < end; i += spacingValue) {
      result.push(categories[i]);
    }

    result.push(categories[max]);

    return result;
  }

  return undefined;
}

export interface MinorAxisDomainValues {
  min: number;
  max: number;
}

export function calculateMinorAxisDomainValues(
  chartData: ChartData[],
  axis: AxisConfiguration,
): MinorAxisDomainValues {
  const { min = 0, max = 0 } = getMinMax(
    chartData.flatMap(item => Object.values(omit(item, ['name']))),
  );

  return {
    min: parseNumber(axis.min) ?? getNiceMinMaxValue(min, true),
    max: parseNumber(axis.max) ?? getNiceMinMaxValue(max),
  };
}

export function getMinorAxisDomainTicks(
  chartData: ChartData[],
  axis: AxisConfiguration,
): DomainTicks {
  if (!chartData.length) {
    return {};
  }
  const axisDomain = calculateMinorAxisDomainValues(chartData, axis);

  const domain: [AxisDomainItem, AxisDomainItem] = [
    axisDomain.min,
    axisDomain.max,
  ];
  const ticks = calculateMinorTicks(
    axisDomain.min,
    axisDomain.max,
    axis.tickSpacing,
    axis.tickConfig,
  );

  return {
    domain,
    ticks,
  };
}

export function getMajorAxisDomainTicks(
  chartData: ChartData[],
  axis: AxisConfiguration,
): DomainTicks {
  if (!chartData.length) {
    return {};
  }

  const majorAxisCategories = chartData.map(({ name }) => name);

  const min = parseNumber(axis.min) ?? 0;
  const max = parseNumber(axis.max) ?? majorAxisCategories.length - 1;

  const domain: [AxisDomainItem, AxisDomainItem] = [min, max];

  const ticks = calculateMajorTicks(
    axis.tickConfig,
    majorAxisCategories,
    min,
    max,
    axis.tickSpacing,
  );
  return { domain, ticks };
}
