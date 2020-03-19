import '@common/modules/charts/components/charts.scss';
import {
  AxisConfiguration,
  ChartDefinition,
  StackedBarProps,
} from '@common/modules/charts/types/chart';
import {
  ChartData,
  createSortedAndMappedDataForAxis,
  generateMajorAxis,
  generateMinorAxis,
  getKeysForChart,
  populateDefaultChartProps,
} from '@common/modules/charts/util/chartUtils';
import parseNumber from '@common/utils/number/parseNumber';
import React, { memo } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

export interface HorizontalBarProps extends StackedBarProps {
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

const HorizontalBarBlock = ({
  data,
  meta,
  height,
  width,
  stacked = false,
  labels,
  axes,
  legend,
  renderLegend,
}: HorizontalBarProps) => {
  if (
    axes === undefined ||
    axes.major === undefined ||
    axes.minor === undefined ||
    data === undefined ||
    meta === undefined
  )
    return <div>Unable to render chart, chart incorrectly configured</div>;

  const chartData: ChartData[] = createSortedAndMappedDataForAxis(
    axes.major,
    data.result,
    meta,
    labels,
  );

  const keysForChart = getKeysForChart(chartData);

  const minorDomainTicks = generateMinorAxis(chartData, axes.minor);
  const majorDomainTicks = generateMajorAxis(chartData, axes.major);

  return (
    <ResponsiveContainer width={width || '100%'} height={height || 300}>
      <BarChart
        data={chartData}
        layout="vertical"
        stackOffset={stacked ? 'sign' : undefined}
        margin={{
          left: 30,
        }}
      >
        <CartesianGrid
          strokeDasharray="3 3"
          horizontal={axes.minor?.showGrid !== false}
          vertical={axes.major.showGrid !== false}
        />

        <XAxis
          type="number"
          dataKey="value"
          hide={!axes.minor.visible}
          unit={axes.minor.unit}
          scale="auto"
          {...minorDomainTicks}
          height={parseNumber(axes.minor.size)}
          padding={{ left: 20, right: 20 }}
          tickMargin={10}
        />

        <YAxis
          type="category"
          dataKey="name"
          hide={!axes.major.visible}
          unit={axes.major.unit}
          scale="auto"
          {...majorDomainTicks}
          width={parseNumber(axes.major.size)}
        />

        <Tooltip cursor={false} />

        {legend && legend !== 'none' && (
          <Legend content={renderLegend} align="left" layout="vertical" />
        )}

        {keysForChart.map(name => (
          <Bar
            key={name}
            {...populateDefaultChartProps(name, labels[name])}
            stackId={stacked ? 'a' : undefined}
          />
        ))}

        {axes.major.referenceLines?.map(referenceLine => (
          <ReferenceLine
            key={`${referenceLine.position}_${referenceLine.label}`}
            y={referenceLine.position}
            label={referenceLine.label}
          />
        ))}

        {axes.minor.referenceLines?.map(referenceLine => (
          <ReferenceLine
            key={`${referenceLine.position}_${referenceLine.label}`}
            x={referenceLine.position}
            label={referenceLine.label}
          />
        ))}
      </BarChart>
    </ResponsiveContainer>
  );
};

export const horizontalBarBlockDefinition: ChartDefinition = {
  type: 'horizontalbar',
  name: 'Horizontal bar',
  capabilities: {
    dataSymbols: false,
    stackable: true,
    lineStyle: false,
    gridLines: true,
    canSize: true,
    fixedAxisGroupBy: false,
    hasAxes: true,
    hasReferenceLines: true,
    hasLegend: true,
    requiresGeoJson: false,
  },
  options: {
    defaults: {
      height: 300,
      legend: 'top',
    },
  },
  data: [
    {
      type: 'bar',
      title: 'Bar',
      entryCount: 1,
      targetAxis: 'yaxis',
    },
  ],
  axes: {
    major: {
      id: 'major',
      title: 'Y Axis (major axis)',
      type: 'major',
      defaults: {
        groupBy: 'timePeriod',
      },
    },
    minor: {
      id: 'minor',
      title: 'X Axis (minor axis)',
      type: 'minor',
    },
  },
};

export default memo(HorizontalBarBlock);
