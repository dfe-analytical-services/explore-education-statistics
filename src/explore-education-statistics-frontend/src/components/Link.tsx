import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { Link as RouterLink, LinkProps } from 'react-router-dom';

type Props = {
  children: ReactNode;
  unvisited?: boolean;
} & LinkProps;

const Link = ({ children, unvisited = false, ...props }: Props) => (
  <RouterLink
    className={classNames('govuk-link', {
      'govuk-link--no-visited-state': unvisited,
    })}
    {...props}
  >
    {children}
  </RouterLink>
);

export default Link;
