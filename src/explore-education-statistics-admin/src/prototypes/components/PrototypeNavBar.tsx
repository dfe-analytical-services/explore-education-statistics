import NavLink from '@admin/components/NavLink';
import classNames from 'classnames';
import React from 'react';
import styles from '@admin/components/NavBar.module.scss';

const PrototypeNavBar = () => {
  return (
    <nav
      className={classNames(
        styles.navigation,
        'govuk-!-margin-top-6 govuk-!-margin-bottom-9',
      )}
    >
      <ul className={classNames(styles.list, 'govuk-!-margin-bottom-0')}>
        <li>
          <NavLink to="/prototypes/admin-release-summary">
            Release summary
          </NavLink>
        </li>
        <li>
          <NavLink to="metadata#test-4">Manage data</NavLink>
        </li>
        <li>
          <NavLink to="/prototypes2">Manage data blocks</NavLink>
        </li>
        <li>
          <NavLink to="/prototypes3">Manage content</NavLink>
        </li>
        <li>
          <NavLink to="/prototypes4">Release status</NavLink>
        </li>
        <li>
          <NavLink to="pre-release">Pre-release access</NavLink>
        </li>
      </ul>
    </nav>
  );
};

export default PrototypeNavBar;
