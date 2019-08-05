import {
  ChartDataB,
  ChartDefinition,
  ChartProps,
  conditionallyAdd,
  createSortedAndMappedDataForAxis,
  createDataForAxis,
  CustomToolTip,
  GenerateMajorAxis,
  GenerateMinorAxis,
  getKeysForChart,
  populateDefaultChartProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { ChartSymbol } from '@common/services/publicationService';
import { Dictionary } from '@common/types';

import classnames from 'classnames';

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

import './charts.scss';

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

const LineChartBlock = (props: ChartProps) => {
  const {
    data,
    meta,
    height,
    axes,
    labels,
    legend,
    legendHeight,
    width,
  } = props;

  if (
    axes === undefined ||
    axes.major === undefined ||
    axes.minor === undefined ||
    data === undefined ||
    meta === undefined
  )
    return <div>Unable to render chart</div>;

  const chartData: ChartDataB[] = createSortedAndMappedDataForAxis(
    axes.major,
    data.result,
    meta,
    labels,
  );

  const keysForChart = getKeysForChart(chartData);

  const minorDomainTicks = GenerateMinorAxis(chartData, axes.minor);
  const majorDomainTicks = GenerateMajorAxis(chartData, axes.major);

  return (
    <ResponsiveContainer width={width || '100%'} height={height || 300}>
      <LineChart
        data={chartData}
        className={classnames({ 'legend-bottom': legend === 'bottom' })}
        margin={{
          left: 30,
          top: legend === 'top' ? 10 : 0,
        }}
      >
        <Tooltip content={CustomToolTip} />

        {(legend === 'top' || legend === 'bottom') && (
          <Legend verticalAlign={legend} height={+(legendHeight || '50')} />
        )}

        <CartesianGrid
          strokeDasharray="3 3"
          horizontal={axes.minor.showGrid !== false}
          vertical={axes.major.showGrid !== false}
        />

        <YAxis
          type="number"
          dataKey="value"
          hide={axes.minor.visible === false}
          unit={
            (axes.minor.unit && axes.minor.unit !== '' && axes.minor.unit) || ''
          }
          scale="auto"
          {...minorDomainTicks}
          width={conditionallyAdd(axes.minor.size)}
        />

        <XAxis
          type="category"
          dataKey="name"
          hide={axes.major.visible === false}
          unit={
            (axes.major.unit && axes.major.unit !== '' && axes.major.unit) || ''
          }
          scale="auto"
          {...majorDomainTicks}
          padding={{ left: 20, right: 20 }}
          height={conditionallyAdd(
            axes.major.size,
            legend === 'bottom' ? 0 : undefined,
          )}
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

        {axes.major.referenceLines &&
          axes.major.referenceLines.map(referenceLine => (
            <ReferenceLine
              key={`${referenceLine.position}_${referenceLine.label}`}
              x={referenceLine.position}
              label={referenceLine.label}
            />
          ))}

        {axes.minor.referenceLines &&
          axes.minor.referenceLines.map(referenceLine => (
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
  },

  data: [
    {
      type: 'line',
      title: 'Line',
      entryCount: 'multiple',
      targetAxis: 'xaxis',
    },
  ],

  axes: [
    {
      id: 'xaxis',
      title: 'X Axis',
      type: 'major',
      defaultDataType: 'timePeriods',
    },
    {
      id: 'yaxis',
      title: 'Y Axis',
      type: 'minor',
    },
  ],
};

LineChartBlock.definition = definition;

export default LineChartBlock;
