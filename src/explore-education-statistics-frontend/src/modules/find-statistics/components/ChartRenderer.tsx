import React, { Component } from 'react';
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  Symbols,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

interface ChartRendererProps {
  data: any;
}

const colours: string[] = [
  '#b10e1e',
  '#006435',
  '#005ea5',
  '#800080',
  '#C0C0C0',
];

const symbols: any[] = ['circle', 'square', 'triangle', 'cross', 'star'];

export class ChartRenderer extends Component<ChartRendererProps> {
  public render() {
    const { chartData, chartDataKeys } = this.props.data;

    const CustomToolTip = <div>Hello</div>;
    const yAxisLabel = 'Absence Rate';
    const xAxisLabel = 'School Year';

    return (
      <div className="dfe-content-overflow">
        <LineChart
          width={900}
          height={300}
          data={chartData}
          margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
        >
          <Tooltip content={CustomToolTip} />
          <Legend verticalAlign="top" height={36} />
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="name"
            label={{
              offset: 5,
              position: 'bottom',
              value: xAxisLabel,
            }}
            padding={{ left: 20, right: 20 }}
            tickMargin={10}
          />
          <YAxis
            label={{
              angle: -90,
              offset: 0,
              position: 'left',
              value: yAxisLabel,
            }}
            scale="auto"
            unit="%"
          />
          {chartDataKeys.map((dataKey: any, index: any) => (
            <Line
              key={index}
              name={dataKey}
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
      </div>
    );
  }
}
