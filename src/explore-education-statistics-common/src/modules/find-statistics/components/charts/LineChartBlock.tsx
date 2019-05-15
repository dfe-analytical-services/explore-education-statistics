import { ChartProps } from '@common/modules/find-statistics/components/charts/ChartFunctions';

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
  public render() {
    const { data, indicators, height, xAxis, yAxis, labels, meta } = this.props;

    const chartData = data.result.map(result => {
      return indicators.reduce(
        (v, indicatorName) => {
          return {
            ...v,
            [indicatorName]: result.measures[indicatorName],
          };
        },
        { name: `${meta.timePeriods[result.year].label}` },
      );
    });

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
          {indicators.length > 1 ? (
            <Legend verticalAlign="top" height={36} />
          ) : (
            ''
          )}
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="name"
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
            unit="%"
            domain={yAxisDomain}
          />
          {indicators.map((dataKey, index) => {
            const key = index;

            return (
              <Line
                key={key}
                name={labels[dataKey]}
                type="linear"
                dataKey={dataKey}
                stroke={colours[index]}
                fill={colours[index]}
                strokeWidth="5"
                unit="%"
                legendType={symbols[index]}
                activeDot={{ r: 3 }}
                dot={props => <Symbols {...props} type={symbols[index]} />}
                isAnimationActive={false}
              />
            );
          })}
        </LineChart>
      </ResponsiveContainer>
    );
  }
}
