import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { LinkProps } from 'react-router-dom';
import Link from './Link';

type Props = {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  to?: string;
  type?: 'button' | 'submit' | 'reset';
} & Partial<LinkProps>;

const Button = ({
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

  return to ? (
    <Link
      {...linkProps}
      to={to}
      className={classes}
      role="button"
      aria-disabled={disabled}
    >
      {children}
    </Link>
  ) : (
    <button
      aria-disabled={disabled}
      className={classes}
      disabled={disabled}
      type={type}
    >
      {children}
    </button>
  );
};

export default Button;
