import React from 'react';
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  Symbols,
  Tooltip,
  TooltipProps,
  XAxis,
  YAxis,
} from 'recharts';
import styles from './PrototypeChartSample.module.scss';

interface Props {
  xAxisLabel?: string;
  yAxisLabel?: string;
  chartData?: object[];
  chartDataKeys: string[];
}

const colours: string[] = [
  '#b10e1e',
  '#006435',
  '#005ea5',
  '#800080',
  '#C0C0C0',
];

const symbols: ('circle' | 'square' | 'triangle' | 'cross' | 'star')[] = [
  'circle',
  'square',
  'triangle',
  'cross',
  'star',
];

const CustomToolTip = (props: TooltipProps) => {
  const { active, payload, label } = props;

  if (active) {
    if (!payload) {
      return null;
    }

    return (
      <div className={styles.tooltip}>
        <p>{label}</p>
        {payload
          .sort((a, b) => {
            return (b.value as number) - (a.value as number);
          })
          .map((_, index) => (
            <p key={index}>{`${payload[index].name} : ${
              payload[index].value
            }`}</p>
          ))}
      </div>
    );
  }
};

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
        {chartDataKeys.map((dataKey, index) => (
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
};

export default PrototypeChartSample;
