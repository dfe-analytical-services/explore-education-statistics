import classNames from 'classnames';
import React, { ReactNode } from 'react';

export interface TagProps {
  children: ReactNode;
  colour?:
    | 'grey'
    | 'green'
    | 'turquoise'
    | 'blue'
    | 'purple'
    | 'pink'
    | 'red'
    | 'orange'
    | 'yellow';
  className?: string;
  id?: string | undefined;
  strong?: boolean;
}

const Tag = ({
  children,
  className,
  colour,
  id = undefined,
  strong = false,
}: TagProps) => {
  const classes = classNames('govuk-tag', className, {
    [`govuk-tag--${colour}`]: !!colour,
  });

  return strong ? (
    <strong className={classes} id={id}>
      {children}
    </strong>
  ) : (
    <span className={classes} id={id}>
      {children}
    </span>
  );
};

export default Tag;
