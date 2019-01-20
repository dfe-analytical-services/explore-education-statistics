import classNames from 'classnames';
import React, { FunctionComponent } from 'react';
import styles from './FluidWidthContainer.module.scss';

interface Props {
  className?: string;
}

const FluidWidthContainer: FunctionComponent<Props> = ({
  children,
  className,
}) => {
  return (
    <div className={classNames(styles.wrapper, className)}>{children}</div>
  );
};

export default FluidWidthContainer;
