import React from 'react';
import Link from '../../../components/Link';

interface Props {
  sectionId?: string;
  action?: string;
}

const PrototypePublicationSummary = () => {
  return (
    <>
      <h2 className="govuk-heading-m">Release summary</h2>

      <p className="govuk-bo">
        These details will be shown to users to help identify this release.
      </p>

      <dl className="govuk-summary-list govuk-!-margin-bottom-9">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Release type</dt>
          <dd className="govuk-summary-list__value">Academic year</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Release period</dt>
          <dd className="govuk-summary-list__value">2018 to 2019</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Lead statistician</dt>
          <dd className="govuk-summary-list__value">Alex Miller</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Scheduled publish date</dt>
          <dd className="govuk-summary-list__value">20 September 2019</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key" />
          <dd className="govuk-summary-list__actions">
            <Link to="/prototypes/publication-create-new-absence-config-edit">
              Edit release summary
            </Link>
          </dd>
        </div>
      </dl>

      <div className="govuk-!-margin-top-9 dfe-align--right">
        <Link to="/prototypes/publication-create-new-absence-data">
          <span className="govuk-heading-m govuk-!-margin-bottom-0">
            Next step
          </span>
          Manage data
        </Link>
      </div>
    </>
  );
};

export default PrototypePublicationSummary;
