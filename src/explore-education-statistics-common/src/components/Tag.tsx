import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  className?: string;
  strong?: boolean;
  id?: string | undefined;
}

const Tag = ({
  children,
  className,
  strong = false,
  id = undefined,
}: Props) => {
  const classes = classNames('govuk-tag', className);

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
