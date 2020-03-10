import '@common/modules/charts/components/charts.scss';
import {
  ChartDefinition,
  StackedBarProps,
} from '@common/modules/charts/types/chart';
import {
  ChartData,
  conditionallyAdd,
  createSortedAndMappedDataForAxis,
  generateMajorAxis,
  generateMinorAxis,
  getKeysForChart,
  populateDefaultChartProps,
} from '@common/modules/charts/util/chartUtils';
import React from 'react';
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

export type VerticalBarProps = StackedBarProps;

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
          vertical={axes.minor?.showGrid !== false}
          horizontal={axes.major.showGrid !== false}
        />

        <YAxis
          type="number"
          dataKey="value"
          hide={axes.minor?.visible === false}
          unit={axes.minor?.unit}
          scale="auto"
          {...minorDomainTicks}
          width={conditionallyAdd(axes.minor?.size)}
        />

        <XAxis
          type="category"
          dataKey="name"
          hide={axes.major.visible === false}
          unit={axes.major.unit}
          scale="auto"
          {...majorDomainTicks}
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

        {axes.minor?.referenceLines?.map(referenceLine => (
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

const definition: ChartDefinition = {
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
  },

  data: [
    {
      type: 'bar',
      title: 'Bar',
      entryCount: 1,
      targetAxis: 'xaxis',
    },
  ],

  axes: [
    {
      id: 'major',
      title: 'X Axis',
      type: 'major',
      defaultDataType: 'timePeriod',
    },
    {
      id: 'minor',
      title: 'Y Axis',
      type: 'minor',
    },
  ],

  requiresGeoJson: false,
};

VerticalBarBlock.definition = definition;

export default VerticalBarBlock;
