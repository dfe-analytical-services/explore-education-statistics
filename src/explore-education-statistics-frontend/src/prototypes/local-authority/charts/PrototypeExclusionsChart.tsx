import React from 'react';
import { CartesianGrid, Label, Line, LineChart, Tooltip, XAxis, YAxis } from 'recharts';
import FluidChartContainer from '../../../components/FluidChartContainer';
import exclusionRateData from '../test-data/exclusionRateData';

const PrototypeExclusionsChart = () => {
  return (
    <FluidChartContainer>
      <LineChart
        data={exclusionRateData}
        margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
      >
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="name" padding={{ left: 20, right: 20 }} tickMargin={10}>
          <Label position="bottom" offset={5} value="Academic year" />
        </XAxis>
        <YAxis scale="auto" unit="%">
          <Label
            angle={-90}
            fontSize={14}
            position="center"
            value="Permanent exclusion percentage"
            dx={-40}
          />
        </YAxis>
        <Line
          type="linear"
          dataKey="Primary"
          stroke="#28A197"
          strokeWidth="1"
          unit="%"
          activeDot={{ r: 3 }}
        />
        <Line
          type="linear"
          dataKey="Secondary"
          stroke="#6F72AF"
          strokeWidth="1"
          unit="%"
          activeDot={{ r: 3 }}
        />
        <Line
          type="linear"
          dataKey="Special"
          stroke="#DF3034"
          strokeWidth="1"
          unit="%"
          activeDot={{ r: 3 }}
        />
        <Line
          type="linear"
          dataKey="Total"
          stroke="#ffbf47"
          strokeWidth="1"
          unit="%"
          activeDot={{ r: 3 }}
        />
        <Tooltip />
      </LineChart>
    </FluidChartContainer>
  );
};

export default PrototypeExclusionsChart;
