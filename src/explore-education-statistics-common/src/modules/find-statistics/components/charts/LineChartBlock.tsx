import ChartFunctions, {
  ChartData,
  ChartDefinition,
  ChartProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';

import React, { Component } from 'react';
import {
  AxisDomain,
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Symbols,
  Tooltip,
  TooltipProps,
  XAxis,
  YAxis,
} from 'recharts';
import { colours, symbols } from './Charts';

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

export default class LineChartBlock extends Component<ChartProps> {
  public static definition: ChartDefinition = {
    type: 'line',
    name: 'Line',

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
      },
      {
        id: 'yaxis',
        title: 'Y Axis',
        type: 'value',
      },
    ],
  };

  public render() {
    const {
      data,
      height,
      xAxis,
      yAxis,
      meta,
      dataSets,
      dataLabels,
    } = this.props;

    // const timePeriods = meta.timePeriods || {};

    const chartData: ChartData[] = ChartFunctions.generateDataGroupedByIndicators(
      // @ts-ignore
      ChartFunctions.filterResultsByDataSet(dataSets, data.result),
      meta,
    );

    let yAxisDomain: [AxisDomain, AxisDomain] = [0, 0];

    if (yAxis.min !== undefined && yAxis.max !== undefined) {
      yAxisDomain = [yAxis.min, yAxis.max];
    }

    return (
      <ResponsiveContainer width={900} height={height || 300}>
        <LineChart
          data={chartData}
          margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
        >
          <Tooltip content={CustomToolTip} />
          <Legend verticalAlign="top" height={36} />
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="name"
            type="category"
            allowDuplicatedCategory={false}
            label={{
              offset: 5,
              position: 'bottom',
              value: xAxis.title,
            }}
            padding={{ left: 20, right: 20 }}
            tickMargin={10}
          />
          <YAxis
            label={{
              angle: -90,
              offset: 0,
              position: 'left',
              value: yAxis.title,
            }}
            scale="auto"
            domain={yAxisDomain}
            dataKey="value"
          />

          {chartData.map((cd, index) => (
            <Line
              dataKey="value"
              data={cd.data}
              name={cd.name && dataLabels[cd.name] && dataLabels[cd.name].label}
              key={cd.name}
              legendType={symbols[index]}
              dot={props => <Symbols {...props} type={symbols[index]} />}
              type="linear"
              stroke={colours[index]}
              fill={colours[index]}
              strokeWidth="5"
              unit={cd.name && dataLabels[cd.name] && dataLabels[cd.name].unit}
              isAnimationActive={false}
            />
          ))}
        </LineChart>
      </ResponsiveContainer>
    );
  }
}
