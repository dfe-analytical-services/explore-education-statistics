import React from 'react';
import Link from '@admin/components/Link';

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
          <dt className="govuk-summary-list__key">Time identifier</dt>
          <dd className="govuk-summary-list__value">Academic year</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Time period</dt>
          <dd className="govuk-summary-list__value">2018 to 2019</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Scheduled publish date</dt>
          <dd className="govuk-summary-list__value">20 September 2019</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">
            Expected next release date
          </dt>
          <dd className="govuk-summary-list__value">18 September 2020</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Release type</dt>
          <dd className="govuk-summary-list__value">National statistics</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Release author</dt>
          <dd className="govuk-summary-list__value">Ann Evans</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">
            Publication and release contact
          </dt>
          <dd className="govuk-summary-list__value">Alex Miller</dd>
        </div>

        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Template</dt>
          <dd className="govuk-summary-list__value">
            Copied existing template (2017/18)
          </dd>
        </div>

        <h2 className="govuk-heading-m govuk-!-margin-top-6">
          Production team
        </h2>

        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Team lead</dt>
          <dd className="govuk-summary-list__value">
            <a href="mailto: example@email.co.uk">John Smith</a>
          </dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Primary analyst</dt>
          <dd className="govuk-summary-list__value">
            <a href="mailto: example@email.co.uk">Ann Evans</a>
          </dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Primary analyst</dt>
          <dd className="govuk-summary-list__value">
            <a href="mailto: example@email.co.uk">Alex Miller</a>
          </dd>
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
