import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './FluidWidthContainer.module.scss';

interface Props {
  className?: string;
  children: ReactNode;
}

const FluidWidthContainer = ({ children, className }: Props) => {
  return (
    <div className={classNames(styles.wrapper, className)}>{children}</div>
  );
};

export default FluidWidthContainer;
