import NavLink from '@admin/components/NavLink';
import classNames from 'classnames';
import React from 'react';
import styles from '@admin/components/NavBar.module.scss';

const PrototypeNavBarPublication = () => {
  return (
    <nav
      className={classNames(
        styles.navigation,
        'govuk-!-margin-top-6 govuk-!-margin-bottom-9',
      )}
    >
      <ul className={classNames(styles.list, 'govuk-!-margin-bottom-0')}>
        <li>
          <NavLink to="admin-publication">Releases</NavLink>
        </li>
        <li>
          <NavLink to="admin-methodology">Methodology</NavLink>
        </li>
        <li>
          <NavLink to="admin-contact">Contact</NavLink>
        </li>
        <li>
          <NavLink to="admin-details">Publication details</NavLink>
        </li>
        <li>
          <NavLink to="admin-access">Team access</NavLink>
        </li>
        <li>
          <NavLink to="admin-legacy">Legacy releases</NavLink>
        </li>
      </ul>
    </nav>
  );
};

export default PrototypeNavBarPublication;
