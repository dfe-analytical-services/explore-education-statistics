import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  maxHeight: number | string;
}

const ScrollableContainer = ({ children, className, maxHeight }: Props) => {
  return (
    <div
      className={classNames('dfe-overflow-y--auto', className)}
      style={{ maxHeight }}
    >
      {children}
    </div>
  );
};

export default ScrollableContainer;
