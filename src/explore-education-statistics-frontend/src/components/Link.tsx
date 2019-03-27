import classNames from 'classnames';
import { UrlLike } from 'next-server/router';
import { default as RouterLink, LinkProps } from 'next/link';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';
import { Omit } from '../types/util';

type Props = {
  as?: string | UrlLike;
  children: ReactNode;
  className?: string;
  to?: string | UrlLike;
  unvisited?: boolean;
} & AnchorHTMLAttributes<HTMLAnchorElement> &
  Omit<LinkProps, 'children'>;

const Link = ({
  as,
  children,
  className,
  to,
  unvisited = false,
  ...props
}: Props) => {
  // We support href and to for backwards
  // compatibility with react-router.
  const href = props.href || to;

  return (
    <RouterLink href={href} as={as}>
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
