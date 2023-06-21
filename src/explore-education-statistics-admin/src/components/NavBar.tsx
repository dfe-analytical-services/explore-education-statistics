import NavLink from '@admin/components/NavLink';
import classNames from 'classnames';
import React from 'react';
import styles from './NavBar.module.scss';

interface Props {
  className?: string;
  routes: {
    to: string;
    title: string;
  }[];
  label: string;
}

const NavBar = ({ className, routes, label }: Props) => {
  return (
    <nav
      className={classNames(styles.navigation, className)}
      aria-label={label}
    >
      <ul className={styles.list}>
        {routes.map(route => (
          <li key={route.to}>
            <NavLink key={route.to} to={route.to}>
              {route.title}
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
};

export default NavBar;
