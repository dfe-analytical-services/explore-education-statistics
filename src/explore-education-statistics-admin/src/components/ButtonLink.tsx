import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Link as RouterLink, LinkProps } from 'react-router-dom';

type Props = {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
} & LinkProps;

const ButtonLink = ({
  children,
  className,
  disabled = false,
  to,
  ...props
}: Props) => {
  return (
    <RouterLink
      {...props}
      to={to}
      className={classNames(
        'govuk-button',
        {
          'govuk-button--disabled': disabled,
        },
        className,
      )}
      role="button"
      aria-disabled={disabled}
    >
      {children}
    </RouterLink>
  );
};

export default ButtonLink;
