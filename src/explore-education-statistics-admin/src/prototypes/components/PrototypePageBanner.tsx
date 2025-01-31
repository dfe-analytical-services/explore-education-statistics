import Link from '@admin/components/Link';
import classNames from 'classnames';
import React from 'react';

const PrototypePageBanner = () => {
  return (
    <div className="govuk-phase-banner">
      <p className="govuk-phase-banner__content">
        <strong
          className={classNames(
            'govuk-tag',
            'govuk-phase-banner__content__tag',
          )}
        >
          Prototype
        </strong>

        <span className="govuk-phase-banner__text">
          This is a prototype page –{' '}
          <Link to="/prototypes">View prototype index</Link>
        </span>
      </p>
    </div>
  );
};

export default PrototypePageBanner;
