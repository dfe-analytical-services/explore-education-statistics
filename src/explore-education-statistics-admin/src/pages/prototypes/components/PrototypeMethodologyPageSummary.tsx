import React from 'react';

const PrototypeMethodologySummary = () => {
  return (
    <>
      <h2 className="govuk-heading-m">Methodology summary</h2>

      <dl className="govuk-summary-list">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Title</dt>
          <dd className="govuk-summary-list__value">
            Example statistics: methodology
          </dd>
        </div>

        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Scheduled publish date</dt>
          <dd className="govuk-summary-list__value">20 September 2019</dd>
        </div>

        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Scheduled publish date</dt>
          <dd className="govuk-summary-list__value">20 September 2019</dd>
        </div>

        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Methodology author</dt>
          <dd className="govuk-summary-list__value">Ann evans</dd>
        </div>

        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">
            Methodology primary contact
          </dt>
          <dd className="govuk-summary-list__value">
            Alex Millar <br />
            <a href="mailto:#">example@email.co.uk</a>
            <br />
            07954 765423
          </dd>
        </div>
      </dl>
    </>
  );
};

export default PrototypeMethodologySummary;
