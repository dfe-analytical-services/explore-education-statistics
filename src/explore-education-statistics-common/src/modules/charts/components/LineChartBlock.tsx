import '@common/modules/charts/components/charts.scss';
import CustomTooltip from '@common/modules/charts/components/CustomTooltip';
import {
  AxisConfiguration,
  ChartDefinition,
  ChartProps,
  ChartSymbol,
} from '@common/modules/charts/types/chart';
import {
  ChartData,
  createSortedAndMappedDataForAxis,
  generateMajorAxis,
  generateMinorAxis,
  getKeysForChart,
  populateDefaultChartProps,
} from '@common/modules/charts/util/chartUtils';
import { Dictionary } from '@common/types';

import React from 'react';
import {
  CartesianGrid,
  Legend,
  LegendType,
  Line,
  LineChart,
  ReferenceLine,
  ResponsiveContainer,
  Symbols,
  SymbolsProps,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

const LineStyles: Dictionary<string> = {
  solid: '',
  dashed: '5 5',
  dotted: '2 2',
};

// eslint-disable-next-line react/display-name
const generateDot = (symbol: string | undefined) => (props: SymbolsProps) => {
  // eslint-disable-line react/display-name

  if (symbol === 'none' || symbol === undefined || symbol === '')
    return undefined;

  const chartSymbol: ChartSymbol = symbol as ChartSymbol;

  return <Symbols {...props} type={chartSymbol} />;
};

const generateLegendType = (symbol: LegendType | undefined): LegendType => {
  if (symbol === 'none' || symbol === undefined) return 'line';
  return symbol;
};

export interface LineChartProps extends ChartProps {
  axes: {
    major: AxisConfiguration;
    minor: AxisConfiguration;
  };
}

const LineChartBlock = ({
  data,
  meta,
  height,
  axes,
  labels,
  legend,
  width,
  renderLegend,
}: LineChartProps) => {
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
      <LineChart
        data={chartData}
        margin={{
          left: 30,
        }}
      >
        <Tooltip content={CustomTooltip} />

        {legend && legend !== 'none' && (
          <Legend content={renderLegend} align="left" layout="vertical" />
        )}

        <CartesianGrid
          strokeDasharray="3 3"
          horizontal={axes.minor.showGrid !== false}
          vertical={axes.major.showGrid !== false}
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

        {keysForChart.map(name => (
          <Line
            key={name}
            {...populateDefaultChartProps(name, labels[name])}
            type="linear"
            legendType={generateLegendType(labels[name] && labels[name].symbol)}
            dot={generateDot(labels[name] && labels[name].symbol)}
            strokeWidth="2"
            strokeDasharray={
              labels[name] &&
              labels[name].lineStyle &&
              LineStyles[labels[name].lineStyle || 'solid']
            }
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
      </LineChart>
    </ResponsiveContainer>
  );
};

const definition: ChartDefinition = {
  type: 'line',
  name: 'Line',
  capabilities: {
    dataSymbols: true,
    stackable: false,
    lineStyle: true,
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
      type: 'line',
      title: 'Line',
      entryCount: 'multiple',
      targetAxis: 'xaxis',
    },
  ],
  axes: {
    major: {
      id: 'xaxis',
      title: 'X Axis (major axis)',
      type: 'major',
      defaults: {
        groupBy: 'timePeriod',
      },
    },
    minor: {
      id: 'yaxis',
      title: 'Y Axis (minor axis)',
      type: 'minor',
    },
  },
};

LineChartBlock.definition = definition;

export default LineChartBlock;
