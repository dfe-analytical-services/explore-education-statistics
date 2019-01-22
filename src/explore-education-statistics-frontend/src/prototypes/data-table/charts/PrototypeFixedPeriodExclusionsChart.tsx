import React, { FunctionComponent } from 'react';
import {
  CartesianGrid,
  Label,
  Line,
  LineChart,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import FluidChartContainer from '../../../components/FluidChartContainer';
import { fixedPeriodExclusionChartData } from '../test-data/exclusionRateData';

interface Props {
  exclusions: boolean;
  exclusionsRate: boolean;
}

const PrototypeFixedPeriodExclusionsChart: FunctionComponent<Props> = ({
  exclusions,
  exclusionsRate,
}) => {
  return (
    <FluidChartContainer>
      <LineChart
        data={fixedPeriodExclusionChartData}
        margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
      >
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="name" padding={{ left: 20, right: 20 }} tickMargin={10}>
          <Label position="bottom" offset={5} value="Academic year" />
        </XAxis>
        {exclusions && (
          <YAxis scale="auto" yAxisId="left">
            <Label
              angle={-90}
              fontSize={14}
              position="center"
              value="Fixed period exclusions"
              dx={-40}
            />
          </YAxis>
        )}
        {exclusionsRate && (
          <YAxis scale="auto" unit="%" yAxisId="right" orientation="right">
            <Label
              angle={90}
              fontSize={14}
              position="center"
              value="Fixed period exclusion rate"
              dx={20}
            />
          </YAxis>
        )}
        {exclusions && (
          <Line
            yAxisId="left"
            type="linear"
            dataKey="exclusions"
            name="Fixed period exclusions"
            stroke="#28A197"
            strokeWidth="1"
            activeDot={{ r: 3 }}
          />
        )}
        {exclusionsRate && (
          <Line
            yAxisId="right"
            type="linear"
            dataKey="exclusionsRate"
            name="Fixed period exclusion rate"
            stroke="#6F72AF"
            strokeWidth="1"
            unit="%"
            activeDot={{ r: 3 }}
          />
        )}
        <Tooltip />
      </LineChart>
    </FluidChartContainer>
  );
};

export default PrototypeFixedPeriodExclusionsChart;
