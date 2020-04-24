import NavLink from '@admin/components/NavLink';
import classNames from 'classnames';
import React from 'react';
import styles from './NavBar.module.scss';

interface Props {
  routes: {
    path: string;
    to: string;
    title: string;
  }[];
}

const NavBar = ({ routes }: Props) => {
  return (
    <nav
      className={classNames(
        styles.navigation,
        'govuk-!-margin-top-6 govuk-!-margin-bottom-9',
      )}
    >
      <ul className={classNames(styles.list, 'govuk-!-margin-bottom-0')}>
        {routes.map(route => (
          <li key={route.path}>
            <NavLink key={route.path} to={route.to}>
              {route.title}
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
};

export default NavBar;
