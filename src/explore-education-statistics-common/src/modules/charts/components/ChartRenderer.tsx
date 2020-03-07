import HorizontalBarBlock from '@common/modules/charts/components/HorizontalBarBlock';
import Infographic, {
  InfographicChartProps,
} from '@common/modules/charts/components/Infographic';
import LineChartBlock from '@common/modules/charts/components/LineChartBlock';
import { MapProps } from '@common/modules/charts/components/MapBlock';
import VerticalBarBlock from '@common/modules/charts/components/VerticalBarBlock';
import {
  ChartProps,
  StackedBarProps,
} from '@common/modules/charts/types/chart';
import { ChartType } from '@common/services/publicationService';
import omitBy from 'lodash/fp/omitBy';
import isEqual from 'lodash/isEqual';
import omit from 'lodash/omit';
import dynamic from 'next/dynamic';
import React, { memo, useMemo, useState } from 'react';
import { ContentRenderer, LegendProps } from 'recharts';
import DefaultLegendContent from 'recharts/es6/component/DefaultLegendContent';

const DynamicMapBlock = dynamic(
  () => import('@common/modules/charts/components/MapBlock'),
  {
    ssr: false,
  },
);

export interface ChartRendererProps
  extends ChartProps,
    StackedBarProps,
    MapProps,
    InfographicChartProps {
  type: ChartType | 'unknown';
}

function ChartRenderer(props: ChartRendererProps) {
  const { data, meta, title, type, legend } = props;

  const [legendProps, setLegendProps] = useState<LegendProps>();

  const renderLegend: ContentRenderer<LegendProps> = useMemo(
    () => nextProps => {
      const nextLegendProps = omit(nextProps, 'content');
      // Omit functions from equality check as they will never be equal
      const omitFunctions = omitBy(value => typeof value !== 'function');

      if (
        !isEqual(omitFunctions(nextLegendProps), omitFunctions(legendProps))
      ) {
        setLegendProps(nextLegendProps);
      }

      return null;
    },
    [legendProps],
  );

  const chart = useMemo(() => {
    switch (type.toLowerCase()) {
      case 'line':
        return <LineChartBlock {...props} renderLegend={renderLegend} />;
      case 'verticalbar':
        return <VerticalBarBlock {...props} renderLegend={renderLegend} />;
      case 'horizontalbar':
        return <HorizontalBarBlock {...props} renderLegend={renderLegend} />;
      case 'map':
        return <DynamicMapBlock {...props} renderLegend={renderLegend} />;
      case 'infographic': {
        return <Infographic {...props} />;
      }
      default:
        return <p>Unable to render invalid chart type</p>;
    }
  }, [props, renderLegend, type]);

  // TODO : Temporary sort on the results to get them in date order
  // data.result.sort((a, b) => a.timePeriod.localeCompare(b.timePeriod));

  if (data && meta && data.result.length > 0) {
    return (
      <>
        {title && <h3 className="govuk-heading-s">{title}</h3>}

        {legend === 'top' && type !== 'infographic' && legendProps && (
          <div className="govuk-!-margin-bottom-6">
            <DefaultLegendContent {...legendProps} />
          </div>
        )}

        <div className="govuk-!-margin-bottom-6">{chart}</div>

        {legend === 'bottom' && type !== 'infographic' && legendProps && (
          <div className="govuk-!-margin-bottom-6">
            <DefaultLegendContent {...legendProps} />
          </div>
        )}
      </>
    );
  }

  return <p>Unable to render chart, invalid data configured</p>;
}

export default memo(ChartRenderer);
