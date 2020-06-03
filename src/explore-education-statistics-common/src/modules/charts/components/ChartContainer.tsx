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
              rotated={yAxisLabel.rotated}
              style={{
                flexBasis: yAxisLabel.rotated
                  ? yAxisLabel.width || 50
                  : yAxisLabel.width || 100,
                maxWidth: '40%',
                marginBottom: xAxisHeight,
                marginRight: 10,
              }}
              textStyle={{
                maxWidth: '100%',
                width: yAxisLabel.rotated
                  ? height - xAxisHeight
                  : yAxisLabel.width || 100,
              }}
            >
              {yAxisLabel.text}
            </AxisLabel>
          )}

          <div
            style={{
              width: '100%',
            }}
          >
            {children}

            {xAxisLabel?.text && (
              <AxisLabel
                data-testid="x-axis-label"
                className="dfe-flex dfe-justify-content--center"
                style={{
                  marginLeft: yAxisWidth,
                }}
                textStyle={{
                  maxWidth: xAxisLabel.width,
                }}
              >
                {xAxisLabel.text}
              </AxisLabel>
            )}
          </div>
        </div>
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
