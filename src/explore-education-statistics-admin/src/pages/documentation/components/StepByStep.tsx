import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

const StepByStep = ({ children }: Props) => {
  return <ol>{children}</ol>;
};

export default StepByStep;
