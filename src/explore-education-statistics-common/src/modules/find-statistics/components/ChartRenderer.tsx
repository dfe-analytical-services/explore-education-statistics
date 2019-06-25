import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import {
  Axis,
  ChartDataSet,
  ChartType,
  ReferenceLine,
} from '@common/services/publicationService';
import dynamic from 'next-server/dynamic';
import React from 'react';
import {
  DataBlockData,
  DataBlockMetadata,
} from '@common/services/dataBlockService';

const DynamicMapBlock = dynamic(
  () => import('@common/modules/find-statistics/components/charts/MapBlock'),
  {
    ssr: false,
  },
);

export interface ChartRendererProps {
  type: ChartType;
  indicators: string[];
  data: DataBlockData;
  meta: DataBlockMetadata;
  xAxis: Axis;
  yAxis: Axis;
  height?: number;
  width?: number;
  stacked?: boolean;
  referenceLines?: ReferenceLine[];
  dataSets?: ChartDataSet[];
}

function ChartRenderer(props: ChartRendererProps) {
  const {
    data,
    height,
    width,
    meta,
    indicators,
    referenceLines,
    stacked,
    type,
    xAxis = { title: '' },
    yAxis = { title: '' },
    dataSets,
  } = props;

  const labels = Object.entries(meta.indicators).reduce(
    (results, [key, indicator]) => ({ ...results, [key]: indicator.label }),
    {},
  );

  // TODO : Temporary sort on the results to get them in date order
  data.result.sort((a, b) => {
    if (a.year < b.year) {
      return -1;
    }

    if (a.year > b.year) {
      return 1;
    }

    return 0;
  });

  switch (type.toLowerCase()) {
    case 'line':
      return (
        <LineChartBlock
          indicators={indicators}
          data={data}
          meta={meta}
          labels={labels}
          xAxis={xAxis}
          yAxis={yAxis}
          height={height}
          width={width}
          referenceLines={referenceLines}
          dataSets={dataSets}
        />
      );
    case 'verticalbar':
      return (
        <VerticalBarBlock
          indicators={indicators}
          data={data}
          meta={meta}
          labels={labels}
          xAxis={xAxis}
          yAxis={yAxis}
          height={height}
          width={width}
          referenceLines={referenceLines}
        />
      );
    case 'horizontalbar':
      return (
        <HorizontalBarBlock
          indicators={indicators}
          data={data}
          meta={meta}
          labels={labels}
          xAxis={xAxis}
          yAxis={yAxis}
          height={height}
          width={width}
          stacked={stacked}
          referenceLines={referenceLines}
        />
      );
    case 'map':
      return (
        <DynamicMapBlock
          indicators={indicators}
          data={data}
          meta={meta}
          labels={labels}
          xAxis={xAxis}
          yAxis={yAxis}
          height={height}
          width={width}
        />
      );
    default:
      return <div>[ Unimplemented chart type requested ${type} ]</div>;
  }
}

export default ChartRenderer;
