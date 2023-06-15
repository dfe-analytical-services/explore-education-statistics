import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import RouterLink, { LinkProps as RouterLinkProps } from 'next/link';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';

export type LinkProps = {
  children: ReactNode;
  className?: string;
  prefetch?: boolean;
  scroll?: RouterLinkProps['scroll'];
  shallow?: RouterLinkProps['shallow'];
  to: RouterLinkProps['href'];
  unvisited?: boolean;
  testId?: string;
} & OmitStrict<AnchorHTMLAttributes<HTMLAnchorElement>, 'href'>;

const Link = ({
  children,
  className,
  prefetch,
  scroll = true,
  shallow,
  to,
  unvisited = false,
  testId,
  ...props
}: LinkProps) => {
  const isAbsolute = typeof to === 'string' && to.startsWith('http');

  const link = (
    <a
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
      href={isAbsolute ? (to as string) : undefined}
      rel={isAbsolute ? 'external' : undefined}
      className={classNames(
        'govuk-link',
        {
          'govuk-link--no-visited-state': unvisited,
        },
        className,
      )}
      data-testid={testId}
    >
      {children}
    </a>
  );

  if (isAbsolute) {
    return link;
  }

  return (
    <RouterLink
      href={to}
      prefetch={prefetch}
      scroll={scroll}
      shallow={shallow}
      passHref
    >
      {link}
    </RouterLink>
  );
};

export default Link;
