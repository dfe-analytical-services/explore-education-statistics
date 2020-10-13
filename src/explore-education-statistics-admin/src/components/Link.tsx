import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import React, { ReactNode } from 'react';
import {
  Link as RouterLink,
  LinkProps as RouterLinkProps,
} from 'react-router-dom';

export type LinkProps = {
  back?: boolean;
  children: ReactNode;
  className?: string;
  unvisited?: boolean;
} & OmitStrict<RouterLinkProps, 'href'>;

const Link = ({
  back,
  children,
  className,
  to,
  unvisited = false,
  ...props
}: LinkProps) => {
  const isAbsolute = typeof to === 'string' && to.startsWith('http');

  if (isAbsolute) {
    return (
      <a
        rel="noopener noreferrer"
        target="_blank"
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...props}
        href={to as string}
        className={classNames(
          'govuk-link',
          {
            'govuk-back-link': back,
            'govuk-link--no-visited-state': unvisited,
          },
          className,
        )}
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
          'govuk-back-link': back,
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
