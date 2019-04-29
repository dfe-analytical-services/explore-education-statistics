import classNames from 'classnames';
// eslint-disable-next-line import/no-unresolved
import { UrlLike } from 'next-server/router';
import Link from 'next/link';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';

type Props = {
  children: ReactNode;
  className?: string;
  disabled?: boolean;
  to?: string | UrlLike;
} & AnchorHTMLAttributes<HTMLAnchorElement>;

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
