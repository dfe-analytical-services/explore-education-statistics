import classNames from 'classnames';
// eslint-disable-next-line import/no-unresolved
import { UrlLike } from 'next-server/router';
import RouterLink from 'next/link';
import React, { AnchorHTMLAttributes, ReactNode } from 'react';
import {
  AnalyticProps,
  logEvent,
} from '@frontend/services/googleAnalyticsService';

export type LinkProps = {
  as?: string | UrlLike;
  children: ReactNode;
  className?: string;
  prefetch?: boolean;
  to?: string | UrlLike;
  unvisited?: boolean;
} & AnchorHTMLAttributes<HTMLAnchorElement> &
  AnalyticProps;

const Link = ({
  as,
  children,
  className,
  prefetch,
  to,
  href,
  analytics,
  unvisited = false,
  ...props
}: LinkProps) => {
  const handleAnalytics = () => {
    if (analytics) {
      logEvent(analytics.category, analytics.action, window.location.pathname);
    }
  };

  /* eslint-disable jsx-a11y/no-static-element-interactions */
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
    </RouterLink>
  );
  /* eslint-enable jsx-a11y/no-static-element-interactions */
};

export default Link;
