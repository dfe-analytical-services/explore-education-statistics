import classNames from 'classnames';
import React from 'react';
import Link from '../../components/Link';
import styles from './PrototypePageBanner.module.scss';

const PrototypePageBanner = () => {
  return (
    <div className="govuk-phase-banner">
      <p className="govuk-phase-banner__content">
        <strong
          className={classNames(
            'govuk-tag',
            'govuk-phase-banner__content__tag',
            [styles.prototypeTag],
          )}
        >
          Prototype
        </strong>

        <span className="govuk-phase-banner__text">
          This is a prototype site –{' '}
          <Link to="/prototypes">View prototype index</Link>
        </span>
      </p>
    </div>
  );
};

export default PrototypePageBanner;
