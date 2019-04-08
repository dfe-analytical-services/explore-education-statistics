import React from "react";

interface Props {
  sectionId?: string;
  action?: string;
}

const PrototypePublicationSummary = ({ sectionId, action }: Props) => {
  return (
    <>
      <h2 className="govuk-heading-m">Release setup summary</h2>
      <dl className="govuk-summary-list">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Publication title</dt>
          <dd className="govuk-summary-list__value">
            Pupil absence statistics and data for schools in England
          </dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Release type</dt>
          <dd className="govuk-summary-list__value">Academic year</dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Release period</dt>
          <dd className="govuk-summary-list__value">2018 to 2019</dd>
        </div>
      </dl>
      <a href="/prototypes/publication-create-new-absence-config-edit">
        Edit release setup details
      </a>
    </>
  );
};

export default PrototypePublicationSummary;
