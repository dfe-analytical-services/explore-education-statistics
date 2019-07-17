import React from 'react';

const PageBanner = () => {
  return (
    <div className="govuk-phase-banner">
      <p className="govuk-phase-banner__content">
        <strong className="govuk-tag govuk-phase-banner__content__tag">
          Beta
        </strong>

        <span className="govuk-phase-banner__text">
          This is a new service – your{' '}
          <a
            rel="noopener noreferrer"
            target="_blank"
            href="https://www.smartsurvey.co.uk/s/7YS0I/"
          >
            feedback
          </a>{' '}
          will help us to improve it.
        </span>
      </p>
    </div>
  );
};

export default PageBanner;
