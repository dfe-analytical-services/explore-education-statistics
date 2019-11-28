import classNames from 'classnames';
import React, { ReactNode } from 'react';
import {
  Link as RouterLink,
  LinkProps as RouterLinkProps,
} from 'react-router-dom';

export type LinkProps = {
  children: ReactNode;
  className?: string;
  unvisited?: boolean;
} & RouterLinkProps;

const Link = ({
  children,
  className,
  to,
  unvisited = false,
  href,
  ...props
}: LinkProps) => {
  if (href && !to) {
    return (
      <a
        className={classNames(
          'govuk-link',
          {
            'govuk-link--no-visited-state': unvisited,
          },
          className,
        )}
        href={href}
        {...props}
        rel="noopener noreferrer"
        target="_blank"
      >
        {children}
      </a>
    );
  }
  return (
    <RouterLink
      {...props}
      to={to}
      className={classNames(
        'govuk-link',
        {
          'govuk-link--no-visited-state': unvisited,
        },
        className,
      )}
    >
      {children}
    </RouterLink>
  );
};

export default Link;
