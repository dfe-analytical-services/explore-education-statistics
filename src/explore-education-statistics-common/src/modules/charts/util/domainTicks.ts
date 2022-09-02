import {
  AxisConfiguration,
  TickConfig,
} from '@common/modules/charts/types/chart';
import { ChartData } from '@common/modules/charts/types/dataSet';
import getMinMax from '@common/utils/number/getMinMax';
import parseNumber from '@common/utils/number/parseNumber';
import omit from 'lodash/omit';
import { AxisDomain } from 'recharts';

interface DomainTicks {
  domain?: [AxisDomain, AxisDomain];
  ticks?: (string | number)[];
}

function getNiceMaxValue(maxValue: number): number {
  if (maxValue === 0) {
    return 0;
  }

  const maxIsLessThanOne = Math.abs(maxValue) < 1;
  let max = maxValue;
  if (maxIsLessThanOne) {
    max = maxValue * 100;
  }

  const numberOf0s = 10 ** Math.floor(Math.log10(Math.abs(max)));
  const maxReducedToLessThan10 = Math.ceil(max / numberOf0s);

  if (maxReducedToLessThan10 % 2 && maxReducedToLessThan10 % 5) {
    return maxIsLessThanOne
      ? ((maxReducedToLessThan10 + 1) * numberOf0s) / 100
      : (maxReducedToLessThan10 + 1) * numberOf0s;
  }
  return maxIsLessThanOne
    ? (maxReducedToLessThan10 * numberOf0s) / 100
    : maxReducedToLessThan10 * numberOf0s;
}

function calculateMinorTicks(
  config: TickConfig | undefined,
  min: number,
  max: number,
  spacing = 5,
): number[] | undefined {
  if (config === 'startEnd') {
    return [min, max];
  }

  let spacingValue = +spacing;

  if (spacingValue <= 0) {
    spacingValue = 1.0;
  }

  if (config === 'custom') {
    const minimumSpacingValue = getNiceMaxValue(
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

export function getMinorAxisDomainTicks(
  chartData: ChartData[],
  axis: AxisConfiguration,
): DomainTicks {
  if (!chartData.length) {
    return {};
  }

  const { min = 0, max = 0 } = getMinMax(
    chartData.flatMap(item => Object.values(omit(item, ['name']))),
  );

  const axisMin = parseNumber(axis.min) ?? min;
  const axisMax = parseNumber(axis.max) ?? getNiceMaxValue(max);

  const domain: [AxisDomain, AxisDomain] = [axisMin, axisMax];
  const ticks = calculateMinorTicks(
    axis.tickConfig,
    axisMin,
    axisMax,
    axis.tickSpacing,
  );

  return { domain, ticks };
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

  const domain: [AxisDomain, AxisDomain] = [min, max];
  const ticks = calculateMajorTicks(
    axis.tickConfig,
    majorAxisCategories,
    min,
    max,
    axis.tickSpacing,
  );

  return { domain, ticks };
}
