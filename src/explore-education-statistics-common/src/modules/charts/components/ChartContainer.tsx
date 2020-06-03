import useMounted from '@common/hooks/useMounted';
import AxisLabel from '@common/modules/charts/components/AxisLabel';
import { ChartProps, Label } from '@common/modules/charts/types/chart';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { LegendProps } from 'recharts';
import DefaultLegendContent from 'recharts/lib/component/DefaultLegendContent';

interface Props {
  children: ReactNode;
  height: number;
  legend?: LegendProps;
  legendPosition?: ChartProps['legend'];
  yAxisWidth?: number;
  yAxisLabel?: Label;
  xAxisHeight?: number;
  xAxisLabel?: Label;
}

const ChartContainer = ({
  children,
  height,
  legend,
  legendPosition,
  yAxisLabel,
  yAxisWidth = 0,
  xAxisLabel,
  xAxisHeight = 0,
}: Props) => {
  const { isMounted } = useMounted();

  const yAxisLabelWidth = yAxisLabel?.text ? yAxisLabel?.width ?? 100 : 0;

  if (!isMounted) {
    return null;
  }

  return (
    <>
      {legendPosition === 'top' && legend && (
        <div aria-hidden className="govuk-!-margin-bottom-6">
          <DefaultLegendContent {...legend} />
        </div>
      )}

      <div className="govuk-!-margin-bottom-6">
        <div
          className={classNames('dfe-flex', {
            'dfe-align-items--center': !yAxisLabel?.rotated,
          })}
        >
          {yAxisLabel?.text && (
            <AxisLabel
              data-testid="y-axis-label"
              width={
                yAxisLabel.rotated ? height - xAxisHeight : yAxisLabelWidth
              }
              rotated={yAxisLabel.rotated}
              style={{
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
            data-testid="x-axis-label"
            className="dfe-flex dfe-justify-content--center"
            width={xAxisLabel.width}
            style={{
              marginLeft: yAxisWidth + yAxisLabelWidth,
            }}
          >
            {xAxisLabel.text}
          </AxisLabel>
        )}
      </div>

      {legendPosition === 'bottom' && legend && (
        <div aria-hidden className="govuk-!-margin-bottom-6">
          <DefaultLegendContent {...legend} />
        </div>
      )}
    </>
  );
};

export default ChartContainer;
