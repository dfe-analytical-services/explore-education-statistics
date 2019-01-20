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
import { sessionsAbsentChartData } from '../test-data/absenceRateData';

interface Props {
  authorised: boolean;
  unauthorised: boolean;
  overall: boolean;
}

const PrototypeAbsenceRateChart: FunctionComponent<Props> = ({
  authorised,
  overall,
  unauthorised,
}) => {
  return (
    <FluidChartContainer>
      <LineChart
        data={sessionsAbsentChartData}
        margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
      >
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="name" padding={{ left: 20, right: 20 }} tickMargin={10}>
          <Label position="bottom" offset={5} value="Academic year" />
        </XAxis>
        <YAxis scale="auto" unit="%">
          <Label angle={-90} position="left" value="Absence rate" />
        </YAxis>
        {unauthorised && (
          <Line
            type="linear"
            dataKey="unauthorised"
            name="Unauthorised absence rate"
            stroke="#28A197"
            strokeWidth="1"
            unit="%"
            activeDot={{ r: 3 }}
          />
        )}
        {authorised && (
          <Line
            type="linear"
            dataKey="authorised"
            name="Authorised absence rate"
            stroke="#6F72AF"
            strokeWidth="1"
            unit="%"
            activeDot={{ r: 3 }}
          />
        )}
        {overall && (
          <Line
            type="linear"
            dataKey="overall"
            name="Overall absence rate"
            stroke="#DF3034"
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

export default PrototypeAbsenceRateChart;
