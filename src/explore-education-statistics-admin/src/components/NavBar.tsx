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
}

const NavBar = ({ className, routes }: Props) => {
  return (
    <nav
      className={classNames(styles.navigation, className)}
      aria-label="tab-navigation"
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
