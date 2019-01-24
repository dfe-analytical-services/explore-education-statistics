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
import { permanentExclusionChartData } from '../test-data/exclusionsDataV1';

interface Props {
  exclusions: boolean;
  exclusionsRate: boolean;
}

const PrototypePermanentExclusionsChart: FunctionComponent<Props> = ({
  exclusions,
  exclusionsRate,
}) => {
  return (
    <FluidChartContainer>
      <LineChart
        data={permanentExclusionChartData}
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
              value="Permanent exclusions"
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
              value="Permanent exclusion rate"
              dx={40}
            />
          </YAxis>
        )}
        {exclusions && (
          <Line
            yAxisId="left"
            type="linear"
            dataKey="exclusions"
            name="Permanent exclusions"
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
            name="Permanent exclusion rate"
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

export default PrototypePermanentExclusionsChart;
