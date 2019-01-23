import React from 'react';
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

interface Props {
  xAxisLabel?: string;
  yAxisLabel?: string;
  chartData?: any;
  chartDataKeys: string[];
}

const colours = ['#b10e1e', '#006435', '#005ea5', '#800080', '#C0C0C0'];

const PrototypeChartSample = ({
  xAxisLabel,
  yAxisLabel,
  chartData,
  chartDataKeys,
}: Props) => {
  return (
    <div className="dfe-content-overflow">
      <LineChart
        width={900}
        height={300}
        data={chartData}
        margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
      >
        <Tooltip />
        <Legend iconType="square" verticalAlign="top" height={36} />
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
        {chartDataKeys.map((dataKey, index) => (
          <Line
            key={index}
            type="linear"
            dataKey={dataKey}
            stroke={colours[index]}
            strokeWidth="5"
            unit="%"
            activeDot={{ r: 3 }}
          />
        ))}
      </LineChart>
    </div>
  );
};

export default PrototypeChartSample;
