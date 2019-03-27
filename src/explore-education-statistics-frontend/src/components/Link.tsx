import classNames from 'classnames';
import { UrlLike } from 'next-server/router';
import { default as RouterLink } from 'next/link';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';

type Props = {
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
  unvisited = false,
  ...props
}: Props) => {
  // We support href and to for backwards
  // compatibility with react-router.
  const href = props.href || to;

  return (
    <RouterLink href={href} as={as} prefetch={prefetch}>
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
