import classNames from 'classnames';
import React, { ReactNode } from 'react';
import { NavLink as RouterNavLink, NavLinkProps } from 'react-router-dom';
import styles from './NavLink.module.scss';

type Props = {
  children: ReactNode;
  className?: string;
  unvisited?: boolean;
} & NavLinkProps;

const NavLink = ({ children, className, to, ...props }: Props) => {
  return (
    <RouterNavLink
      {...props}
      end
      to={to}
      className={({ isActive }) =>
        classNames(
          'govuk-link',
          'govuk-link--no-visited-state',
          className,
          isActive ? styles.currentPage : undefined,
        )
      }
    >
      {children}
    </RouterNavLink>
  );
};

export default NavLink;
