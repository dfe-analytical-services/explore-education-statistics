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
import dynamic from 'next/dynamic';
import React, { memo, useMemo } from 'react';

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
  const { data, meta, title } = props;

  const chart = useMemo(() => {
    const { type } = props;

    switch (type.toLowerCase()) {
      case 'line':
        return <LineChartBlock {...props} />;
      case 'verticalbar':
        return <VerticalBarBlock {...props} />;
      case 'horizontalbar':
        return <HorizontalBarBlock {...props} />;
      case 'map':
        return <DynamicMapBlock {...props} />;
      case 'infographic': {
        return <Infographic {...props} />;
      }
      default:
        return <div>Unable to render invalid chart type</div>;
    }
  }, [props]);

  // TODO : Temporary sort on the results to get them in date order
  // data.result.sort((a, b) => a.timePeriod.localeCompare(b.timePeriod));

  if (data && meta && data.result.length > 0) {
    return (
      <>
        {title && <h3 className="govuk-heading-s">{title}</h3>}
        {chart}
      </>
    );
  }

  return <div>Unable to render chart, invalid data configured</div>;
}

export default memo(ChartRenderer);
