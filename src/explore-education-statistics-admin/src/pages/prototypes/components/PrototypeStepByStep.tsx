import React, { ReactNode } from 'react';
import styles from './PrototypeStepByStep.module.scss';

interface Props {
  children: ReactNode;
}

const StepByStep = ({ children }: Props) => {
  return <ol>{children}</ol>;
};

export default StepByStep;
