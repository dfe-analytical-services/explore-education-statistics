import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import useMounted from '@common/hooks/useMounted';
import AxisLabel from '@common/modules/charts/components/AxisLabel';
import { Label } from '@common/modules/charts/types/chart';
import { LegendPosition } from '@common/modules/charts/types/legend';
import classNames from 'classnames';
import React, { ReactNode, useState } from 'react';
import { LegendProps } from 'recharts';
import { DefaultLegendContent } from 'recharts/lib/component/DefaultLegendContent';

interface Props {
  children: ReactNode;
  height: number;
  legend?: LegendProps;
  legendPosition?: LegendPosition;
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
  const [renderCount, setRenderCount] = useState(0);

  const [handleResize] = useDebouncedCallback(() => {
    setRenderCount(renderCount + 1);
  }, 500);

  const { isMounted } = useMounted(() => {
    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
    };
  });

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

      <div className="govuk-!-margin-bottom-6" key={renderCount}>
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
