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

export interface VerticalBarProps extends StackedBarProps {
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

const VerticalBarBlock = ({
  data,
  meta,
  height,
  width,
  labels,
  axes,
  stacked,
  legend,
  renderLegend,
}: VerticalBarProps) => {
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
        margin={{
          left: 30,
        }}
      >
        <CartesianGrid
          strokeDasharray="3 3"
          vertical={axes.minor.showGrid !== false}
          horizontal={axes.major.showGrid !== false}
        />

        <YAxis
          type="number"
          dataKey="value"
          hide={!axes.minor.visible}
          unit={axes.minor.unit}
          scale="auto"
          {...minorDomainTicks}
          width={Number(axes.minor.size)}
        />

        <XAxis
          type="category"
          dataKey="name"
          hide={!axes.major.visible}
          unit={axes.major.unit}
          scale="auto"
          {...majorDomainTicks}
          height={Number(axes.major.size)}
          padding={{ left: 20, right: 20 }}
          tickMargin={10}
        />

        <Tooltip />

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
            x={referenceLine.position}
            label={referenceLine.label}
          />
        ))}

        {axes.minor.referenceLines?.map(referenceLine => (
          <ReferenceLine
            key={`${referenceLine.position}_${referenceLine.label}`}
            y={referenceLine.position}
            label={referenceLine.label}
          />
        ))}
      </BarChart>
    </ResponsiveContainer>
  );
};

export const verticalBarBlockDefinition: ChartDefinition = {
  type: 'verticalbar',
  name: 'Vertical bar',
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
      targetAxis: 'xaxis',
    },
  ],
  axes: {
    major: {
      id: 'major',
      title: 'X Axis (major axis)',
      type: 'major',
      defaults: {
        groupBy: 'timePeriod',
      },
    },
    minor: {
      id: 'minor',
      title: 'Y Axis (minor axis)',
      type: 'minor',
    },
  },
};

export default memo(VerticalBarBlock);
