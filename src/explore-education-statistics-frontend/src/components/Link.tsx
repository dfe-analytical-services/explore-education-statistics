import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import RouterLink, { LinkProps as RouterLinkProps } from 'next/link';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';
import {
  AnalyticProps,
  logEvent,
} from '@frontend/services/googleAnalyticsService';

export type LinkProps = {
  as?: RouterLinkProps['as'];
  children: ReactNode;
  className?: string;
  prefetch?: boolean;
  to: RouterLinkProps['href'];
  unvisited?: boolean;
} & OmitStrict<AnchorHTMLAttributes<HTMLAnchorElement>, 'href'> &
  AnalyticProps;

const Link = ({
  as,
  children,
  className,
  prefetch,
  to,
  analytics,
  unvisited = false,
  ...props
}: LinkProps) => {
  const handleAnalytics = () => {
    if (analytics) {
      logEvent(
        analytics.category,
        analytics.action,
        analytics.label ? analytics.label : window.location.pathname,
      );
    }
  };

  const isAbsolute = typeof to === 'string' && to.startsWith('http');

  const link = (
    <a
      {...props}
      href={isAbsolute ? (to as string) : undefined}
      className={classNames(
        'govuk-link',
        {
          'govuk-link--no-visited-state': unvisited,
        },
        className,
      )}
      onClick={() => {
        handleAnalytics();
      }}
      onKeyDown={event => {
        if (event.key === 'Enter' || event.key === ' ') {
          handleAnalytics();
        }
      }}
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
