import classNames from 'classnames';
// eslint-disable-next-line import/no-unresolved
import { UrlLike } from 'next-server/router';
import RouterLink from 'next/link';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';

export type LinkProps = {
  as?: string | UrlLike;
  children: ReactNode;
  className?: string;
  prefetch?: boolean;
  to?: string | UrlLike;
  unvisited?: boolean;
} & AnchorHTMLAttributes<HTMLAnchorElement>;

const Link = ({
  as,
  children,
  className,
  prefetch,
  to,
  href,
  unvisited = false,
  ...props
}: LinkProps) => {
  return (
    <RouterLink href={href || to} as={as} prefetch={prefetch}>
      <a
        {...props}
        className={classNames(
          'govuk-link',
          {
            'govuk-link--no-visited-state': unvisited,
          },
          className,
        )}
      >
        {children}
      </a>
    </RouterLink>
  );
};

export default Link;
