import FluidWidthContainer from '@common/components/FluidWidthContainer';
import React, { ReactElement } from 'react';
import { ResponsiveContainer } from 'recharts';

interface Props {
  className?: string;
  children: ReactElement;
}

const FluidChartContainer = ({ className, children }: Props) => {
  return (
    <FluidWidthContainer className={className}>
      <ResponsiveContainer>{children}</ResponsiveContainer>
    </FluidWidthContainer>
  );
};

export default FluidChartContainer;
