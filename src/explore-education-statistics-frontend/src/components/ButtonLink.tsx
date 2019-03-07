import classNames from 'classnames';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';
import Link from './Link';

type Props = {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  to: string;
} & AnchorHTMLAttributes<HTMLAnchorElement>

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
