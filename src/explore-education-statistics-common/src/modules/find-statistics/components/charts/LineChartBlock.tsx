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
import { colours, parseCondensedTimePeriodRange, symbols } from './Charts';
import { ChartProps } from '@common/modules/find-statistics/components/charts/AbstractChart';

const CustomToolTip = (props: TooltipProps) => {
  if (props.active) {
    const { payload, label } = props;

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
            .map((_, index) => (
              <p key={index}>
                {`${payload[index].name} : ${payload[index].value}`}
              </p>
            ))}
      </div>
    );
  }
};

export class LineChartBlock extends Component<ChartProps> {
  public render() {
    const {
      characteristicsData,
      chartDataKeys,
      xAxis,
      yAxis,
      labels,
    } = this.props;

    const chartData = characteristicsData.result.map(result => {
      return chartDataKeys.reduce(
        (v, indicatorName) => {
          return {
            ...v,
            [indicatorName]: result.indicators[indicatorName],
          };
        },
        { name: parseCondensedTimePeriodRange(`${result.timePeriod}`) },
      );
    });

    let yAxisDomain: [AxisDomain, AxisDomain] = [0, 0];

    if (yAxis.min !== undefined && yAxis.max !== undefined) {
      yAxisDomain = [yAxis.min, yAxis.max];
    }

    return (
      <ResponsiveContainer width={900} height={this.props.height || 300}>
        <LineChart
          data={chartData}
          margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
        >
          <Tooltip content={CustomToolTip} />
          {chartDataKeys.length > 1 ? (
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
          {chartDataKeys.map((dataKey, index) => (
            <Line
              key={index}
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
            />
          ))}
        </LineChart>
      </ResponsiveContainer>
    );
  }
}
