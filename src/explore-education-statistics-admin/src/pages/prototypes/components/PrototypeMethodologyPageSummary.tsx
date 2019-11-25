import React from 'react';
import Link from '@admin/components/Link';

interface Props {
  sectionId?: string;
  action?: string;
}

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
      </dl>
    </>
  );
};

export default PrototypeMethodologySummary;
