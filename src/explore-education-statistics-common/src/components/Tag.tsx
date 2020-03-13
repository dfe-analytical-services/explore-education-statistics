import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  strong?: boolean;
}

const Tag = ({ children, className, strong = false }: Props) => {
  const classes = classNames('govuk-tag', {
    [className || '']: className,
  });

  return strong ? (
    <strong className={classes}>{children}</strong>
  ) : (
    <span className={classes}>{children}</span>
  );
};

export default Tag;
