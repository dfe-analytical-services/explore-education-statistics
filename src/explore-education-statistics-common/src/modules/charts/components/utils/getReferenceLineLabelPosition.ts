import { Axis, AxisType } from '@common/modules/charts/types/chart';
import { AxisDomain, ViewBox } from 'recharts';

export default function getReferenceLineLabelPosition({
  axis,
  axisType,
  otherAxisDomain = [0, 0],
  otherAxisPosition,
  viewBox,
}: {
  axis: Axis;
  axisType: AxisType;
  otherAxisDomain?: [AxisDomain, AxisDomain];
  otherAxisPosition?: number;
  viewBox?: ViewBox;
}) {
  const { height = 0, width = 0, x = 0, y = 0 } = viewBox ?? {};
  const defaultXPosition = axis === 'x' ? x : width / 2 + x;
  const defaultYPosition = axis === 'y' ? y : height / 2 + y;
  // otherAxisPosition is set as a percentage on minor axis lines so max is 100.
  const maxValue = axisType === 'minor' ? 100 : (otherAxisDomain[1] as number);

  // No custom other axis position so default to middle
  if (otherAxisPosition === undefined || !otherAxisDomain) {
    return {
      xPosition: defaultXPosition,
      yPosition: defaultYPosition,
    };
  }

  // Amount to offset from the top on horizontal bar charts so the label is visible when positioned at 100%
  const yOffset = 15;
  const yPosition = y + height * ((maxValue - otherAxisPosition) / maxValue);

  return axis === 'y'
    ? {
        xPosition: x + width * (otherAxisPosition / maxValue),
        yPosition: defaultYPosition,
      }
    : {
        xPosition: defaultXPosition,
        yPosition:
          axisType === 'minor' && otherAxisPosition === maxValue
            ? yPosition + yOffset
            : yPosition,
      };
}
