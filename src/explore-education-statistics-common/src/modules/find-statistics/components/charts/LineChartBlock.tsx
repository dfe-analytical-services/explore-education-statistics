import {
  calculateDataRange,
  ChartDataB,
  ChartDefinition,
  ChartProps,
  conditionallyAdd,
  createDataForAxis,
  getKeysForChart,
  mapNameToNameLabel,
  populateDefaultChartProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';

import React from 'react';
import {
  AxisDomain,
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
  TooltipProps,
  XAxis,
  YAxis,
} from 'recharts';
import { Dictionary } from '@common/types';

import classnames from 'classnames';

import './charts.scss';
import { ChartSymbol } from '@common/services/publicationService';

const CustomToolTip = ({ active, payload, label }: TooltipProps) => {
  if (active) {
    return (
      <div className="graph-tooltip">
        <p>{label}</p>
        {payload &&
          payload
            .sort((a, b) => {
              if (typeof b.value === 'number' && typeof a.value === 'number') {
                return b.value - a.value;
              }

              return 0;
            })
            .map((_, index) => {
              return (
                // eslint-disable-next-line react/no-array-index-key
                <p key={index}>
                  {`${payload[index].name} : ${payload[index].value}`}
                </p>
              );
            })}
      </div>
    );
  }

  return null;
};

const LineStyles: Dictionary<string> = {
  solid: '',
  dashed: '5 5',
  dotted: '2 2',
};

const parseNumberOrDefault = (
  number: string | undefined,
  def: number,
): number => {
  const parsed = number === undefined ? undefined : Number.parseFloat(number);

  if (parsed === undefined || Number.isNaN(parsed)) return def;
  return parsed;
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
    data === undefined ||
    meta === undefined
  )
    return <div>Unable to render chart</div>;

  const chartData: ChartDataB[] = createDataForAxis(
    axes.major,
    data.result,
    meta,
  ).map(mapNameToNameLabel(labels, meta.timePeriods, meta.locations));

  const keysForChart = getKeysForChart(chartData);

  const { min, max } = calculateDataRange(chartData);
  const minorAxisDomain: [AxisDomain, AxisDomain] = [
    parseNumberOrDefault(axes.minor.min, min),
    parseNumberOrDefault(axes.minor.max, max),
  ];

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
          horizontal={axes.minor && axes.minor.showGrid !== false}
          vertical={axes.major.showGrid !== false}
        />

        {axes.major && (
          <XAxis
            dataKey="name"
            hide={axes.major.visible === false}
            label={{
              offset: 5,
              position: 'bottom',
              value: '',
            }}
            scale="auto"
            interval={
              axes.minor && !axes.minor.visible ? 'preserveStartEnd' : undefined
            }
            height={conditionallyAdd(
              axes.major && axes.major.size,
              legend === 'bottom' ? 0 : undefined,
            )}
            padding={{ left: 20, right: 20 }}
            tickMargin={10}
          />
        )}

        {axes.minor && axes.minor.visible && (
          <YAxis
            label={{
              angle: -90,
              offset: 0,
              position: 'left',
              value: '',
            }}
            scale="auto"
            domain={minorAxisDomain}
            dataKey="value"
            width={conditionallyAdd(axes.minor && axes.minor.size)}
          />
        )}

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

        {axes.major &&
          axes.major.referenceLines &&
          axes.major.referenceLines.map(referenceLine => (
            <ReferenceLine
              key={`${referenceLine.position}_${referenceLine.label}`}
              x={referenceLine.position}
              label={referenceLine.label}
            />
          ))}

        {axes.minor &&
          axes.minor.referenceLines &&
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
