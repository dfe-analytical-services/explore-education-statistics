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
  return (
    <RouterLink
      href={to}
      prefetch={prefetch}
      scroll={scroll}
      shallow={shallow}
      className={classNames(
        'govuk-link',
        {
          'govuk-link--no-visited-state': unvisited,
        },
        className,
      )}
      data-testid={testId}
      {...props}
    >
      {children}
    </RouterLink>
  );
};

export default Link;
