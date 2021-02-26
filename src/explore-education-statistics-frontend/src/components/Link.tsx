import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import RouterLink, { LinkProps as RouterLinkProps } from 'next/link';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';

export type LinkProps = {
  as?: RouterLinkProps['as'];
  children: ReactNode;
  className?: string;
  prefetch?: boolean;
  to: RouterLinkProps['href'];
  unvisited?: boolean;
} & OmitStrict<AnchorHTMLAttributes<HTMLAnchorElement>, 'href'>;

const Link = ({
  as,
  children,
  className,
  prefetch,
  to,
  unvisited = false,
  ...props
}: LinkProps) => {
  const isAbsolute = typeof to === 'string' && to.startsWith('http');

  const link = (
    <a
      // eslint-disable-next-line react/jsx-props-no-spreading
      {...props}
      href={isAbsolute ? (to as string) : undefined}
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
  );

  if (isAbsolute) {
    return link;
  }

  return (
    <RouterLink href={to} as={as} prefetch={prefetch} passHref>
      {link}
    </RouterLink>
  );
};

export default Link;
