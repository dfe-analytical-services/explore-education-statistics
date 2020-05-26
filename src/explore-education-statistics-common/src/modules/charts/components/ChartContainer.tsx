import AxisLabel from '@common/modules/charts/components/AxisLabel';
import { Label } from '@common/modules/charts/types/chart';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  height: number;
  yAxisWidth?: number;
  yAxisLabel?: Label;
  xAxisHeight?: number;
  xAxisLabel?: Label;
}

const ChartContainer = ({
  children,
  height,
  yAxisLabel,
  yAxisWidth = 0,
  xAxisLabel,
  xAxisHeight = 0,
}: Props) => {
  const yAxisLabelWidth = yAxisLabel?.text ? yAxisLabel?.width ?? 100 : 0;

  return (
    <>
      <div
        className={classNames('dfe-flex', {
          'dfe-align-items--center': !yAxisLabel?.rotated,
        })}
      >
        {yAxisLabel?.text && (
          <AxisLabel
            width={yAxisLabel.rotated ? height - xAxisHeight : yAxisLabelWidth}
            rotated={yAxisLabel.rotated}
            style={{
              flexBasis: yAxisLabelWidth,
              marginBottom: xAxisHeight,
              marginRight: 10,
            }}
          >
            {yAxisLabel.text}
          </AxisLabel>
        )}

        {children}
      </div>

      {xAxisLabel?.text && (
        <AxisLabel
          className="dfe-flex dfe-justify-content--center"
          width={xAxisLabel.width}
          style={{
            marginLeft: yAxisWidth + yAxisLabelWidth,
          }}
        >
          {xAxisLabel.text}
        </AxisLabel>
      )}
    </>
  );
};

export default ChartContainer;
