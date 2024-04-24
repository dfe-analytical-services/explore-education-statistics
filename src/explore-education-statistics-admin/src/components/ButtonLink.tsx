import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Link as RouterLink, LinkProps } from 'react-router-dom';

type Props = {
  children: ReactNode;
  className?: string;
  variant?: 'secondary' | 'warning';
} & LinkProps;

const ButtonLink = ({ children, className, to, variant, ...props }: Props) => {
  return (
    <RouterLink
      {...props}
      to={to}
      className={classNames(
        'govuk-button',
        {
          'govuk-button--secondary': variant === 'secondary',
          'govuk-button--warning': variant === 'warning',
        },
        className,
      )}
    >
      {children}
    </RouterLink>
  );
};

export default ButtonLink;
