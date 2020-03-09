import {
  ChartProps,
  StackedBarProps,
} from '@common/modules/charts/types/chart';
import HorizontalBarBlock from '@common/modules/charts/components/HorizontalBarBlock';
import Infographic, {
  InfographicChartProps,
} from '@common/modules/charts/components/Infographic';
import LineChartBlock from '@common/modules/charts/components/LineChartBlock';
import VerticalBarBlock from '@common/modules/charts/components/VerticalBarBlock';
import { ChartType } from '@common/services/publicationService';
import dynamic from 'next/dynamic';
import React from 'react';
import { MapProps } from '@common/modules/charts/components/MapBlock';

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

function ChartTypeRenderer({ type, ...chartProps }: ChartRendererProps) {
  switch (type.toLowerCase()) {
    case 'line':
      return <LineChartBlock {...chartProps} />;
    case 'verticalbar':
      return <VerticalBarBlock {...chartProps} />;
    case 'horizontalbar':
      return <HorizontalBarBlock {...chartProps} />;
    case 'map':
      return <DynamicMapBlock {...chartProps} />;
    case 'infographic': {
      return <Infographic {...chartProps} />;
    }
    default:
      return (
        <div>
          Unable to render chart, an unimplemented chart type was requested '
          {type}'
        </div>
      );
  }
}

function ChartRenderer(props: ChartRendererProps) {
  const { data, meta, title } = props;

  // TODO : Temporary sort on the results to get them in date order
  // data.result.sort((a, b) => a.timePeriod.localeCompare(b.timePeriod));

  if (data && meta && data.result.length > 0) {
    return (
      <>
        {title && <h3 className="govuk-heading-s">{title}</h3>}
        <ChartTypeRenderer {...props} />
      </>
    );
  }

  return <div>Unable to render chart, invalid data configured</div>;
}

export default ChartRenderer;
