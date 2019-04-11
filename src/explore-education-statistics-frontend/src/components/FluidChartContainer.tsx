import FluidWidthContainer from '@common/components/FluidWidthContainer';
import React, { FunctionComponent } from 'react';
import { ResponsiveContainer } from 'recharts';

interface Props {
  className?: string;
}

const FluidChartContainer: FunctionComponent<Props> = ({
  className,
  children,
}) => {
  return (
    <FluidWidthContainer className={className}>
      <ResponsiveContainer>{children}</ResponsiveContainer>
    </FluidWidthContainer>
  );
};

export default FluidChartContainer;
