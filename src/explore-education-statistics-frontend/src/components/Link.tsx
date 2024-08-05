import { OmitStrict } from '@common/types';
import getContentLinkProps from '@common/utils/url/getContentLinkProps';
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
  rel: originalRel,
  ...props
}: LinkProps) => {
  const { target, rel } = getContentLinkProps({
    url: to.toString(),
    rel: originalRel,
  });

  return (
    <RouterLink
      {...props}
      href={to}
      target={target}
      prefetch={prefetch}
      scroll={scroll}
      shallow={shallow}
      rel={rel}
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
    </RouterLink>
  );
};

export default Link;
