import React, { FunctionComponent } from 'react';
import { ResponsiveContainer } from 'recharts';
import FluidWidthContainer from './FluidWidthContainer';

interface Props {
  className?: string;
}

const FluidChartContainer: FunctionComponent<Props> = ({ className, children }) => {
  return (
    <FluidWidthContainer className={className}>
      <ResponsiveContainer>{children}</ResponsiveContainer>
    </FluidWidthContainer>
  );
};

export default FluidChartContainer;
