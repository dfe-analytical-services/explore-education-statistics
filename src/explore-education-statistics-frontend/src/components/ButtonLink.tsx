import classNames from 'classnames';
import Link from 'next/link';
import React from 'react';
import { LinkProps } from './Link';

type Props = {
  disabled?: boolean;
} & LinkProps;

const ButtonLink = ({
  as,
  children,
  className,
  disabled = false,
  to,
  href,
  prefetch,
  ...props
}: Props) => {
  return (
    <Link href={href || to} as={as} prefetch={prefetch}>
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
