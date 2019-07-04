import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { NavLink, NavLinkProps } from 'react-router-dom';

type Props = {
  children: ReactNode;
  className?: string;
  unvisited?: boolean;
} & NavLinkProps;

const DfeNavLink = ({
  children,
  className,
  to,
  unvisited = false,
  ...props
}: Props) => {
  return (
    <NavLink
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
    </NavLink>
  );
};

export default DfeNavLink;
