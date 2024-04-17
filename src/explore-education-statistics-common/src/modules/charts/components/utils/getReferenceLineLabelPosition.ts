import { Axis, AxisType } from '@common/modules/charts/types/chart';
import { CartesianViewBox } from 'recharts/types/util/types';

interface LabelPosition {
  x: number;
  y: number;
}

export default function getReferenceLineLabelPosition({
  axis,
  axisType,
  otherAxisDomainMax,
  otherAxisDomainMin,
  otherAxisPosition,
  perpendicularLine,
  viewBox,
}: {
  axis: Axis;
  axisType: AxisType;
  otherAxisDomainMax?: number;
  otherAxisDomainMin?: number;
  otherAxisPosition?: number;
  perpendicularLine?: boolean;
  viewBox?: CartesianViewBox;
}): LabelPosition {
  const { height = 0, width = 0, x = 0, y = 0 } = viewBox ?? {};
  const defaultXPosition = axis === 'y' ? width / 2 + x : x;
  const defaultYPosition = axis === 'x' ? height / 2 + y : y;

  // No custom other axis position or domain so default to middle
  if (
    otherAxisPosition === undefined ||
    otherAxisDomainMax === undefined ||
    otherAxisDomainMin === undefined ||
    otherAxisPosition < otherAxisDomainMin ||
    otherAxisPosition > otherAxisDomainMax ||
    perpendicularLine
  ) {
    return {
      x: defaultXPosition,
      y: defaultYPosition,
    };
  }

  // Amount to offset from the top on horizontal bar charts so the label is visible when positioned at 100%
  const yOffset = 15;

  const yPosition =
    y +
    height * ((otherAxisDomainMax - otherAxisPosition) / otherAxisDomainMax);

  return axis === 'y'
    ? {
        x: x + width * (otherAxisPosition / otherAxisDomainMax),
        y: defaultYPosition,
      }
    : {
        x: defaultXPosition,
        y:
          axisType === 'minor' && otherAxisPosition === otherAxisDomainMax
            ? yPosition + yOffset
            : yPosition,
      };
}
