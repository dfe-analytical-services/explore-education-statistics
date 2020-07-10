import NavLink from '@admin/components/NavLink';
import classNames from 'classnames';
import React from 'react';
import styles from '@admin/components/NavBar.module.scss';

const NavBar = () => {
  return (
    <nav
      className={classNames(
        styles.navigation,
        'govuk-!-margin-top-6 govuk-!-margin-bottom-9',
      )}
    >
      <ul className={classNames(styles.list, 'govuk-!-margin-bottom-0')}>
        <li>
          <NavLink to="example">Release summary</NavLink>
        </li>
        <li>
          <NavLink to="public-metadata">Manage data</NavLink>
        </li>
        <li>
          <NavLink to="example">Manage data blocks</NavLink>
        </li>
        <li>
          <NavLink to="example">Manage content</NavLink>
        </li>
        <li>
          <NavLink key="5" to="example">
            Release status
          </NavLink>
        </li>
      </ul>
    </nav>
  );
};

export default NavBar;
