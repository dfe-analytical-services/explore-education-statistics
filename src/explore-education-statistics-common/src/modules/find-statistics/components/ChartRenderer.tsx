import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import { ChartType } from '@common/services/publicationService';
import dynamic from 'next-server/dynamic';
import React from 'react';
import { ChartProps, StackedBarProps } from './charts/ChartFunctions';

const DynamicMapBlock = dynamic(
  () => import('@common/modules/find-statistics/components/charts/MapBlock'),
  {
    ssr: false,
  },
);

export interface ChartRendererProps extends ChartProps, StackedBarProps {
  type: ChartType;
}

function ChartRenderer(props: ChartRendererProps) {
  const { type, data, meta, ...remainingProps } = props;

  // TODO : Temporary sort on the results to get them in date order
  data.result.sort((a, b) => a.timePeriod.localeCompare(b.timePeriod));

  if (data && meta && data.result.length > 0) {
    const chartProps: ChartProps = {
      data,
      meta,
      legend: 'top',
      legendHeight: '50',
      ...remainingProps,
    };

    switch (type.toLowerCase()) {
      case 'line':
        return <LineChartBlock {...chartProps} />;
      case 'verticalbar':
        return <VerticalBarBlock {...chartProps} />;
      case 'horizontalbar':
        return <HorizontalBarBlock {...chartProps} />;
      case 'map':
        return <DynamicMapBlock {...chartProps} />;
      default:
        return <div>[ Unimplemented chart type requested ${type} ]</div>;
    }
  }

  return <div>Invalid data specified</div>;
}

export default ChartRenderer;
