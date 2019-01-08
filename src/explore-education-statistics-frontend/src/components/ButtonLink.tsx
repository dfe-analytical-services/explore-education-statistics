import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { LinkProps } from 'react-router-dom';
import Link from './Link';

type Props = {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  to: string;
} & Partial<LinkProps>;

const ButtonLink = ({
  children,
  className,
  disabled = false,
  to,
  type = 'button',
  ...linkProps
}: Props) => {
  const classes = classNames(
    'govuk-button',
    {
      'govuk-button--disabled': disabled,
    },
    className,
  );

  return (
    <Link
      {...linkProps}
      to={to}
      className={classes}
      role="button"
      aria-disabled={disabled}
    >
      {children}
    </Link>
  );
};

export default ButtonLink;
