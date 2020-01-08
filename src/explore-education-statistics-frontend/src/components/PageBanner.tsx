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
            href="https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-XMiKzsnr8xJoWM_DeGwIu9UNDJHOEJDRklTNVA1SDdLOFJITEwyWU1OQS4u"
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
