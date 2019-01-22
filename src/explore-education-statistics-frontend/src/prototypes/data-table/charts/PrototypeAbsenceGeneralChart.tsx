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
import { generalChartData } from '../test-data/absenceRateData';

interface Props {
  enrolments: boolean;
  schools: boolean;
}

const PrototypeAbsenceGeneralChart: FunctionComponent<Props> = ({
  enrolments,
  schools,
}) => {
  return (
    <FluidChartContainer>
      <LineChart
        data={generalChartData}
        margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
      >
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="name" padding={{ left: 20, right: 20 }} tickMargin={10}>
          <Label position="bottom" offset={5} value="Academic year" />
        </XAxis>
        <YAxis scale="auto">
          <Label angle={-90} position="left" value="Number" />
        </YAxis>
        {enrolments && (
          <Line
            type="linear"
            dataKey="enrolments"
            name="Enrolments"
            stroke="#28A197"
            strokeWidth="1"
            activeDot={{ r: 3 }}
          />
        )}
        {schools && (
          <Line
            type="linear"
            dataKey="schools"
            name="Schools"
            stroke="#6F72AF"
            strokeWidth="1"
            activeDot={{ r: 3 }}
          />
        )}
        <Tooltip />
      </LineChart>
    </FluidChartContainer>
  );
};

export default PrototypeAbsenceGeneralChart;
