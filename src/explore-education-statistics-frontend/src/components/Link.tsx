import classNames from 'classnames';
// eslint-disable-next-line import/no-unresolved
import { UrlLike } from 'next-server/router';
import RouterLink from 'next/link';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';

import { SetLinkRenderer } from '@common/components/Link';

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
  href,
  unvisited = false,
  ...props
}: Props) => {
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

SetLinkRenderer(Link);
export default Link;
