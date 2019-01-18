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

const PrototypeChartSample = () => {
  const chartData = [
    { name: '2012/13', unauthorised: 1.1, authorised: 4.2, overall: 5.3 },
    { name: '2013/14', unauthorised: 1.1, authorised: 3.5, overall: 4.5 },
    { name: '2014/15', unauthorised: 1.1, authorised: 3.5, overall: 4.6 },
    { name: '2015/16', unauthorised: 1.1, authorised: 3.4, overall: 4.6 },
    { name: '2016/17', unauthorised: 1.3, authorised: 3.4, overall: 4.7 },
  ];
  return (
    <>
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
              value: 'School year',
            }}
            padding={{ left: 20, right: 20 }}
            tickMargin={10}
          />
          <YAxis
            label={{
              angle: -90,
              offset: 0,
              position: 'left',
              value: 'Absence rate',
            }}
            scale="auto"
            unit="%"
          />
          <Line
            type="linear"
            dataKey="unauthorised"
            stroke="#b10e1e"
            strokeWidth="5"
            unit="%"
            activeDot={{ r: 3 }}
          />
          <Line
            type="linear"
            dataKey="authorised"
            stroke="#006435"
            strokeWidth="5"
            unit="%"
            activeDot={{ r: 3 }}
          />
          <Line
            type="linear"
            dataKey="overall"
            stroke="#005ea5"
            strokeWidth="5"
            unit="%"
            activeDot={{ r: 3 }}
          />
        </LineChart>
      </div>
    </>
  );
};

export default PrototypeChartSample;
