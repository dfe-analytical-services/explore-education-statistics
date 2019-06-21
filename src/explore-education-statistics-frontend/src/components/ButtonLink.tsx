import classNames from 'classnames';
import Link from 'next/link';
import React from 'react';
import { LinkProps } from './Link';

type Props = {
  disabled?: boolean;
} & LinkProps;

const ButtonLink = ({
  children,
  className,
  disabled = false,
  to,
  href,
  ...props
}: Props) => {
  return (
    <Link {...props} href={href || to}>
      <a
        {...props}
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
      </a>
    </Link>
  );
};

export default ButtonLink;
