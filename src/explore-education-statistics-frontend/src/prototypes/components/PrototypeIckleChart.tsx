import React from 'react';
import { Area, AreaChart } from 'recharts';

const PrototypeChartSample = () => {
  const chartData = [
    { name: '2012/13', overall: (Math.random()*2)+4 },
    { name: '2013/14', overall: (Math.random()*2)+4 },
    { name: '2014/15', overall: (Math.random()*2)+4 },
    { name: '2015/16', overall: (Math.random()*2)+4 },
    { name: '2016/17', overall: (Math.random()*2)+4 },
  ];
  return (
    <>
      <div className="dfe-content-overflow">
        <AreaChart
          width={600}
          height={300}
          data={chartData}
          margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
        >
          <Area
            type="linear"
            dataKey="overall"
            stroke="#208279"
            fill="#28A197"
            strokeWidth="1"
            unit="%"
            activeDot={{ r: 3 }}
          />
        </AreaChart>
      </div>
    </>
  );
};

export default PrototypeChartSample;
