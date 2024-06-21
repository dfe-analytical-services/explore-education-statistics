import { OmitStrict } from '@common/types';
import getTargetRelation from '@common/utils/url/getTargetRelation';
import buildRel from '@common/utils/url/buildRel';
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
      {...props}
      href={to}
      prefetch={prefetch}
      scroll={scroll}
      shallow={shallow}
      rel={buildRel(
        getTargetRelation(to.toString()) === 'internal-public'
          ? []
          : ['noopener', 'noreferrer', 'nofollow', 'external'],
        props.rel,
      )}
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
